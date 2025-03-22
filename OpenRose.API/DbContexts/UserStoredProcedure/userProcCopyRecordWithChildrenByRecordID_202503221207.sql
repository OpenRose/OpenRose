-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0.
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ('userProcCopyRecordWithChildrenByRecordID', 'P') IS NOT NULL
    DROP PROCEDURE userProcCopyRecordWithChildrenByRecordID
GO

CREATE PROCEDURE userProcCopyRecordWithChildrenByRecordID
@RecordID UNIQUEIDENTIFIER,
@OUTPUT_Id UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RetryCount INT = 3;
    DECLARE @RetryWaitTime INT = 500; -- 500 milliseconds (0.5 second)
    DECLARE @RetryAttempt INT = 0;
    DECLARE @DeadlockError INT = 1205; -- SQL Server error code for deadlock

    WHILE @RetryAttempt < @RetryCount
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            DECLARE @MainRecordHierarchyID HIERARCHYID;
            DECLARE @MainRecordName NVARCHAR(128);
            DECLARE @MainRecordType NVARCHAR(128);
            
            DECLARE @NewMainRecordID UNIQUEIDENTIFIER = NEWID();
            DECLARE @NewMainHierarchyID HIERARCHYID;
            DECLARE @NewMainRecordName NVARCHAR(128);
            DECLARE @ParentHierarchyID HIERARCHYID;

            DECLARE @ProjectId UNIQUEIDENTIFIER;
            DECLARE @ItemzTypeId UNIQUEIDENTIFIER;
            DECLARE @NextSiblingHierarchyID HIERARCHYID;

            DECLARE @ChangeEvent NVARCHAR(128) = 'Copied';
            DECLARE @CreatedDate DATETIMEOFFSET = SYSDATETIMEOFFSET();
            DECLARE @Level INT;
            DECLARE @RandomNumber CHAR(5) = RIGHT('00000' + CAST(ABS(CHECKSUM(NEWID())) % 100000 AS VARCHAR(5)), 5);
            DECLARE @Suffix NVARCHAR(11) = '_COPY' + @RandomNumber;

            -- Temporary table to store the hierarchy
            CREATE TABLE #TempRecordHierarchy (
                OldRecordID UNIQUEIDENTIFIER,
                NewRecordID UNIQUEIDENTIFIER,
                OldHierarchyID HIERARCHYID,
                NewHierarchyID HIERARCHYID,
                RecordType NVARCHAR(128),
                Name NVARCHAR(128),
                ParentItemzTypeId UNIQUEIDENTIFIER -- Added to store the parent ItemzTypeId for Itemz records
            );

            -- Temporary table to map old to new ItemzTypeId
            CREATE TABLE #ItemzTypeMapping (
                OldItemzTypeId UNIQUEIDENTIFIER,
                NewItemzTypeId UNIQUEIDENTIFIER
            );

            -- Get the HierarchyID, Name, and RecordType of the main record with lock to prevent concurrent modifications
            SELECT @MainRecordHierarchyID = ih.ItemzHierarchyId,
                   @MainRecordName = ih.Name,
                   @MainRecordType = ih.RecordType
            FROM ItemzHierarchy ih WITH (UPDLOCK, HOLDLOCK)
            WHERE ih.Id = @RecordID;

            -- Check if the record ID exists in the ItemzHierarchy table
            IF @MainRecordHierarchyID IS NULL
            BEGIN
                THROW 50000, 'Record ID not found in ItemzHierarchy table.', 1;
            END

            -- Check if the record is Level 1 or above
            SELECT @Level = @MainRecordHierarchyID.GetLevel();
            IF @Level < 1
            BEGIN
                THROW 50006, 'Record ID must be Level 1 or above.', 1;
            END

            -- Generate the new name for the main record
            SET @NewMainRecordName = LEFT(@MainRecordName, 128 - LEN(@Suffix)) + @Suffix;

            -- Get the Parent Hierarchy ID
            SET @ParentHierarchyID = @MainRecordHierarchyID.GetAncestor(1);

            -- Ensure ParentRecordID is found
            DECLARE @ParentRecordID UNIQUEIDENTIFIER;
            SELECT @ParentRecordID = ih.Id
            FROM ItemzHierarchy ih
            WHERE ih.ItemzHierarchyId = @ParentHierarchyID;

            IF @ParentRecordID IS NULL
            BEGIN
                DECLARE @ErrorMessage NVARCHAR(255); -- Declare a variable for the error message
                SET @ErrorMessage = 'ParentRecordID with Parent Record was not found in ItemzHierarchy for ' + CAST(@ParentHierarchyID AS NVARCHAR(255));
                THROW 50000, @ErrorMessage, 1;
            END

            -- Handle copying based on RecordType
            IF @MainRecordType = 'Project'
            BEGIN
                -- Find the last child of the repository to generate the new main project's hierarchy ID
                SELECT TOP 1 @NextSiblingHierarchyID = ih.ItemzHierarchyId
                FROM ItemzHierarchy ih WITH (UPDLOCK, HOLDLOCK)
                WHERE ih.ItemzHierarchyId.GetAncestor(1) = HIERARCHYID::GetRoot()
                ORDER BY ih.ItemzHierarchyId DESC;

                -- Generate the new main project's hierarchy ID
                SET @NewMainHierarchyID = HIERARCHYID::GetRoot().GetDescendant(@NextSiblingHierarchyID, NULL);

                -- Check that @MainRecordHierarchyID is not the same as @NewMainHierarchyID
                IF @MainRecordHierarchyID = @NewMainHierarchyID
                BEGIN
                    THROW 50005, 'New HierarchyId is the same as the Main HierarchyId.', 1;
                END

                -- Check for existing duplicates for @NewMainHierarchyID
                IF EXISTS (SELECT 1 FROM ItemzHierarchy WHERE ItemzHierarchyId = @NewMainHierarchyID)
                BEGIN
                    THROW 50003, 'Duplicate HierarchyId found in ItemzHierarchy table.', 1;
                END

                -- Insert a copy of the Project record
                INSERT INTO Projects (Id, Name, Status, Description, CreatedBy, CreatedDate)
                SELECT @NewMainRecordID, @NewMainRecordName, Status, Description, CreatedBy, @CreatedDate
                FROM Projects
                WHERE Id = @RecordID;

                -- Set the ProjectId for later use
                SET @ProjectId = @NewMainRecordID;

                -- Insert the Project hierarchy record
                INSERT INTO ItemzHierarchy (Id, RecordType, Name, ItemzHierarchyId)
                VALUES (@NewMainRecordID, 'Project', @NewMainRecordName, @NewMainHierarchyID);

                -- Get all child ItemzType and Itemz of the Project and insert them into the temporary table
                INSERT INTO #TempRecordHierarchy (
                    OldRecordID, NewRecordID, OldHierarchyID, NewHierarchyID, RecordType, Name, ParentItemzTypeId
                )
                SELECT ih.Id,
                       NEWID(),
                       ih.ItemzHierarchyId,
                       HIERARCHYID::Parse(
                           '/' + SUBSTRING(
                               @NewMainHierarchyID.ToString(), 
                               2, 
                               LEN(@NewMainHierarchyID.ToString()) - 1
                           ) + 
                           SUBSTRING(
                               ih.ItemzHierarchyId.ToString(), 
                               LEN(@MainRecordHierarchyID.ToString()) + 1, 
                               LEN(ih.ItemzHierarchyId.ToString()) - LEN(@MainRecordHierarchyID.ToString())
                           )
                       ),
                       ih.RecordType,
                       ih.Name,
                       NULL -- Initially set to NULL
                FROM ItemzHierarchy ih
                WHERE ih.ItemzHierarchyId.IsDescendantOf(@MainRecordHierarchyID) = 1
                  AND ih.Id <> @RecordID -- Exclude the main Project record
                ORDER BY ih.ItemzHierarchyId;
            END
            ELSE IF @MainRecordType = 'ItemzType'
            BEGIN
                -- Find the next sibling hierarchy ID with lock to prevent concurrent modifications
                SELECT TOP 1 @NextSiblingHierarchyID = ih.ItemzHierarchyId
                FROM ItemzHierarchy ih WITH (UPDLOCK, HOLDLOCK)
                WHERE ih.ItemzHierarchyId.GetAncestor(1) = @ParentHierarchyID
                  AND ih.ItemzHierarchyId > @MainRecordHierarchyID
                ORDER BY ih.ItemzHierarchyId;

                -- Generate the new main item's hierarchy ID
                IF @NextSiblingHierarchyID IS NOT NULL
                BEGIN
                    SET @NewMainHierarchyID = @ParentHierarchyID.GetDescendant(@MainRecordHierarchyID, @NextSiblingHierarchyID);
                END
                ELSE
                BEGIN
                    SET @NewMainHierarchyID = @ParentHierarchyID.GetDescendant(@MainRecordHierarchyID, NULL);
                END

                -- Check that @MainRecordHierarchyID is not the same as @NewMainHierarchyID
                IF @MainRecordHierarchyID = @NewMainHierarchyID
                BEGIN
                    THROW 50005, 'New HierarchyId is the same as the Main HierarchyId.', 1;
                END

                -- Check for existing duplicates for @NewMainHierarchyID
                IF EXISTS (SELECT 1 FROM ItemzHierarchy WHERE ItemzHierarchyId = @NewMainHierarchyID)
                BEGIN
                    THROW 50003, 'Duplicate HierarchyId found in ItemzHierarchy table.', 1;
                END

                -- Insert a copy of the ItemzType record
                INSERT INTO ItemzTypes (Id, Name, Status, Description, CreatedBy, CreatedDate, ProjectId, IsSystem)
                SELECT @NewMainRecordID, @NewMainRecordName, Status, Description, CreatedBy, @CreatedDate, @ParentRecordID, 0
                FROM ItemzTypes
                WHERE Id = @RecordID;

                -- Set the ItemzTypeId for later use
                SET @ItemzTypeId = @NewMainRecordID;

                -- Insert mapping of old to new ItemzTypeId
                INSERT INTO #ItemzTypeMapping (OldItemzTypeId, NewItemzTypeId)
                VALUES (@RecordID, @NewMainRecordID);

                -- Insert the ItemzType hierarchy record
                INSERT INTO ItemzHierarchy (Id, RecordType, Name, ItemzHierarchyId)
                VALUES (@NewMainRecordID, 'ItemzType', @NewMainRecordName, @NewMainHierarchyID);

                -- Get all child Itemz of the ItemzType and insert them into the temporary table
                INSERT INTO #TempRecordHierarchy (
                    OldRecordID, NewRecordID, OldHierarchyID, NewHierarchyID, RecordType, Name, ParentItemzTypeId
                )
                SELECT ih.Id,
                       NEWID(),
                       ih.ItemzHierarchyId,
                       HIERARCHYID::Parse(
                           '/' + SUBSTRING(
                               @NewMainHierarchyID.ToString(), 
                               2, 
                               LEN(@NewMainHierarchyID.ToString()) - 1
                           ) + 
                           SUBSTRING(
                               ih.ItemzHierarchyId.ToString(), 
                               LEN(@MainRecordHierarchyID.ToString()) + 1, 
                               LEN(ih.ItemzHierarchyId.ToString()) - LEN(@MainRecordHierarchyID.ToString())
                           )
                       ),
                       ih.RecordType,
                       ih.Name,
                       NULL -- Initially set to NULL
                FROM ItemzHierarchy ih
                WHERE ih.ItemzHierarchyId.IsDescendantOf(@MainRecordHierarchyID) = 1
                  AND ih.Id <> @RecordID -- Exclude the main ItemzType record
                ORDER BY ih.ItemzHierarchyId;
            END
            ELSE IF @MainRecordType = 'Itemz'
            BEGIN
                -- Handle copying for Itemz
                -- Get the level of the RecordID
                SELECT @Level = @MainRecordHierarchyID.GetLevel();

                -- Get the ItemzTypeId from the immediate parent in the hierarchy if the level is 3
                IF @Level = 3
                BEGIN
                    SELECT @ItemzTypeId = ih2.Id
                    FROM ItemzHierarchy ih2 WITH (UPDLOCK, HOLDLOCK)
                    WHERE ih2.ItemzHierarchyId = @MainRecordHierarchyID.GetAncestor(1);

                    -- Check if the ItemzTypeId is found
                    IF @ItemzTypeId IS NULL
                    BEGIN
                        THROW 50001, 'ItemzTypeId not found at level 1 in ItemzHierarchy table.', 1;
                    END

                    -- Verify that the ItemzTypeId exists in the ItemzTypes table
                    IF NOT EXISTS (SELECT 1 FROM ItemzTypes WHERE Id = @ItemzTypeId)
                    BEGIN
                        PRINT 'ItemzTypeId: ' + CAST(@ItemzTypeId AS NVARCHAR(36)); -- Debug output
                        THROW 50002, 'ItemzTypeId not found in ItemzTypes table.', 1;
                    END
                END

                -- Find the next sibling hierarchy ID with lock to prevent concurrent modifications
                SELECT TOP 1 @NextSiblingHierarchyID = ih.ItemzHierarchyId
                FROM ItemzHierarchy ih WITH (UPDLOCK, HOLDLOCK)
                WHERE ih.ItemzHierarchyId.GetAncestor(1) = @ParentHierarchyID
                  AND ih.ItemzHierarchyId > @MainRecordHierarchyID
                ORDER BY ih.ItemzHierarchyId;

                -- Generate the new main item's hierarchy ID
                IF @NextSiblingHierarchyID IS NOT NULL
                BEGIN
                    SET @NewMainHierarchyID = @ParentHierarchyID.GetDescendant(@MainRecordHierarchyID, @NextSiblingHierarchyID);
                END
                ELSE
                BEGIN
                    SET @NewMainHierarchyID = @ParentHierarchyID.GetDescendant(@MainRecordHierarchyID, NULL);
                END

                -- Check that @MainRecordHierarchyID is not the same as @NewMainHierarchyID
                IF @MainRecordHierarchyID = @NewMainHierarchyID
                BEGIN
                    THROW 50005, 'New HierarchyId is the same as the Main HierarchyId.', 1;
                END

                -- Check for existing duplicates for @NewMainHierarchyID
                IF EXISTS (SELECT 1 FROM ItemzHierarchy WHERE ItemzHierarchyId = @NewMainHierarchyID)
                BEGIN
                    THROW 50003, 'Duplicate HierarchyId found in ItemzHierarchy table.', 1;
                END

                -- Insert the main Itemz and its hierarchy into the temporary table
                INSERT INTO #TempRecordHierarchy (
                    OldRecordID, NewRecordID, OldHierarchyID, NewHierarchyID, RecordType, Name, ParentItemzTypeId
                )
                SELECT 
                    ih.Id, 
                    CASE WHEN ih.Id = @RecordID THEN @NewMainRecordID ELSE NEWID() END, 
                    ih.ItemzHierarchyId, 
                    HIERARCHYID::Parse(
                        '/' + SUBSTRING(
                            @NewMainHierarchyID.ToString(), 
                            2, 
                            LEN(@NewMainHierarchyID.ToString()) - 1
                        ) + 
                        SUBSTRING(
                            ih.ItemzHierarchyId.ToString(), 
                            LEN(@MainRecordHierarchyID.ToString()) + 1, 
                            LEN(ih.ItemzHierarchyId.ToString()) - LEN(@MainRecordHierarchyID.ToString())
                        )
                    ),
                    ih.RecordType, 
                    CASE WHEN ih.Id = @RecordID THEN @NewMainRecordName ELSE ih.Name END,
                    NULL -- Initially set to NULL
                FROM ItemzHierarchy ih
                WHERE ih.ItemzHierarchyId.IsDescendantOf(@MainRecordHierarchyID) = 1
                ORDER BY ih.ItemzHierarchyId;
            END

            -- Check for duplicates within new records
            IF EXISTS (
                SELECT NewHierarchyID, COUNT(*) AS DuplicateCount
                FROM #TempRecordHierarchy
                GROUP BY NewHierarchyID
                HAVING COUNT(*) > 1
            )
            BEGIN
                THROW 50004, 'Duplicate HierarchyId found in new records.', 1;
            END

            -- Update ParentItemzTypeId for Itemz records
            UPDATE trh
            SET ParentItemzTypeId = ih2.Id
            FROM #TempRecordHierarchy trh
            INNER JOIN ItemzHierarchy ih1 ON trh.OldHierarchyID = ih1.ItemzHierarchyId
            INNER JOIN ItemzHierarchy ih2 ON ih1.ItemzHierarchyId.GetAncestor(1) = ih2.ItemzHierarchyId
            WHERE trh.RecordType = 'Itemz' AND ih1.ItemzHierarchyId.GetLevel() = 3;

            -- Copy the main ItemzType and its children with lock to prevent concurrent modifications
            INSERT INTO ItemzTypes WITH (TABLOCKX) (Id, Name, Status, Description, CreatedBy, CreatedDate, ProjectId, IsSystem)
            SELECT 
                trh.NewRecordID, 
                trh.Name, 
                it.Status, 
                it.Description, 
                it.CreatedBy, 
                @CreatedDate, 
                @ProjectId, 
                it.IsSystem
            FROM #TempRecordHierarchy trh
            INNER JOIN ItemzTypes it ON trh.OldRecordID = it.Id
            WHERE trh.RecordType = 'ItemzType';

            -- Insert the hierarchy records for the copied ItemzTypes with lock to prevent concurrent modifications
            INSERT INTO ItemzHierarchy WITH (TABLOCKX) (Id, RecordType, Name, ItemzHierarchyId)
            SELECT NewRecordID, RecordType, Name, NewHierarchyID
            FROM #TempRecordHierarchy
            WHERE RecordType = 'ItemzType';

            -- Insert mapping of old to new ItemzTypeId for all copied ItemzTypes
            INSERT INTO #ItemzTypeMapping (OldItemzTypeId, NewItemzTypeId)
            SELECT trh.OldRecordID, trh.NewRecordID
            FROM #TempRecordHierarchy trh
            WHERE trh.RecordType = 'ItemzType';

            -- Copy the main Itemz and its children with lock to prevent concurrent modifications
            INSERT INTO Itemzs WITH (TABLOCKX) (Id, Name, Status, Priority, Description, CreatedBy, CreatedDate, Severity)
            SELECT 
                trh.NewRecordID, 
                trh.Name, 
                iz.Status, 
                iz.Priority, 
                iz.Description, 
                iz.CreatedBy, 
                @CreatedDate, 
                iz.Severity
            FROM #TempRecordHierarchy trh
            INNER JOIN Itemzs iz ON trh.OldRecordID = iz.Id
            WHERE trh.RecordType = 'Itemz';

            -- Insert the hierarchy records for the copied Itemzs with lock to prevent concurrent modifications
            INSERT INTO ItemzHierarchy WITH (TABLOCKX) (Id, RecordType, Name, ItemzHierarchyId)
            SELECT NewRecordID, RecordType, Name, NewHierarchyID
            FROM #TempRecordHierarchy
            WHERE RecordType = 'Itemz';

            -- Insert the ItemzTypeJoinItemz records for the copied Itemzs at level 3
            INSERT INTO ItemzTypeJoinItemz (ItemzTypeId, ItemzId)
            SELECT itmap.NewItemzTypeId, trh.NewRecordID
            FROM #TempRecordHierarchy trh
            INNER JOIN #ItemzTypeMapping itmap ON trh.ParentItemzTypeId = itmap.OldItemzTypeId
            WHERE trh.RecordType = 'Itemz' AND trh.OldHierarchyID.GetLevel() = 3;

            -- Insert change history records for the copied Itemzs with lock to prevent concurrent modifications
            INSERT INTO ItemzChangeHistory WITH (TABLOCKX) (ItemzId, CreatedDate, OldValues, NewValues, ChangeEvent)
            SELECT 
                trh.NewRecordID, @CreatedDate, NULL, 
                CONCAT('CreatedBy: ', iz.CreatedBy, '     Description: ', iz.Description, '     Name: ', iz.Name, '     Priority: ', iz.Priority, '     Severity: ', iz.Severity, '     Status: ', iz.Status), 
                @ChangeEvent
            FROM #TempRecordHierarchy trh
            INNER JOIN Itemzs iz ON trh.OldRecordID = iz.Id
            WHERE trh.RecordType = 'Itemz';

            -- Ensure all new IDs exist in the Itemzs table before inserting into ItemzJoinItemzTrace
            SELECT DISTINCT t.FromItemzId, t.ToItemzId, COALESCE(n1.NewRecordID, t.FromItemzId) AS NewFromItemzId, COALESCE(n2.NewRecordID, t.ToItemzId) AS NewToItemzId
            INTO #TempJoinItemzTrace
            FROM ItemzJoinItemzTrace t
            LEFT JOIN #TempRecordHierarchy n1 ON t.FromItemzId = n1.OldRecordID
            LEFT JOIN #TempRecordHierarchy n2 ON t.ToItemzId = n2.OldRecordID
            WHERE t.FromItemzId IN (SELECT OldRecordID FROM #TempRecordHierarchy)
            OR t.ToItemzId IN (SELECT OldRecordID FROM #TempRecordHierarchy);

            -- Debug output for #TempJoinItemzTrace
            SELECT * FROM #TempJoinItemzTrace;

            -- Debug output for items to be inserted into ItemzJoinItemzTrace
            SELECT DISTINCT NewFromItemzId, NewToItemzId
            FROM #TempJoinItemzTrace
            WHERE NewFromItemzId IS NOT NULL AND NewToItemzId IS NOT NULL;

            -- Insert into ItemzJoinItemzTrace with validations and lock to prevent concurrent modifications
            INSERT INTO ItemzJoinItemzTrace WITH (TABLOCKX) (FromItemzId, ToItemzId)
            SELECT DISTINCT NewFromItemzId, NewToItemzId
            FROM #TempJoinItemzTrace
            WHERE NewFromItemzId IS NOT NULL AND NewToItemzId IS NOT NULL;

            -- Set the output parameter to the ID of the newly copied main record
            SET @OUTPUT_Id = @NewMainRecordID;

            -- Drop the temporary tables
            IF OBJECT_ID('tempdb..#TempRecordHierarchy') IS NOT NULL
                DROP TABLE #TempRecordHierarchy;
            IF OBJECT_ID('tempdb..#TempJoinItemzTrace') IS NOT NULL
                DROP TABLE #TempJoinItemzTrace;
            IF OBJECT_ID('tempdb..#ItemzTypeMapping') IS NOT NULL
                DROP TABLE #ItemzTypeMapping;

            COMMIT TRANSACTION;
            BREAK; -- Exit the retry loop if the transaction is successful
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            -- Drop the temporary tables in case of an error
            IF OBJECT_ID('tempdb..#TempRecordHierarchy') IS NOT NULL
                DROP TABLE #TempRecordHierarchy;
            IF OBJECT_ID('tempdb..#TempJoinItemzTrace') IS NOT NULL
                DROP TABLE #TempJoinItemzTrace;
            IF OBJECT_ID('tempdb..#ItemzTypeMapping') IS NOT NULL
                DROP TABLE #ItemzTypeMapping;

            -- Check if the error is a deadlock error
            IF ERROR_NUMBER() = @DeadlockError
            BEGIN
                SET @RetryAttempt = @RetryAttempt + 1;
                WAITFOR DELAY '00:00:00.500'; -- Wait for 0.5 second
            END
            ELSE
            BEGIN
                -- Rethrow the error if it's not a deadlock error
                THROW;
            END
        END CATCH
    END
END
GO			
			
   