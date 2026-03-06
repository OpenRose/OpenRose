
-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


IF OBJECT_ID ( 'userProcCreateBaselineByItemzTypeID', 'P' ) IS NOT NULL
		DROP PROCEDURE userProcCreateBaselineByItemzTypeID
GO

CREATE PROCEDURE userProcCreateBaselineByItemzTypeID
@ProjectId [uniqueidentifier],
@ItemzTypeId [uniqueidentifier],
@Name [nvarchar](128),
@Description [nvarchar](max),
@CreatedBy [nvarchar](128) = N'Some User',
@OUTPUT_Id [uniqueidentifier] out

AS

-- FIRST set @OUTPUT_Id to be Empty uniqueidentifier.
SET @OUTPUT_Id = (SELECT CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER))

BEGIN
BEGIN TRY
BEGIN TRANSACTION
DECLARE @NewBaselineID [uniqueidentifier]
SET @NewBaselineID = NEWID()

DECLARE @TempBaselineItemzTypeIterator int
DECLARE @TempBaselineItemzTypeNumberOfRows int
DECLARE @TempBaselineItemzNumberOfRows int
DECLARE @CurrentBaselineItemzTypeID [uniqueidentifier]
DECLARE @CurrentItemzTypeID [uniqueidentifier]
DECLARE @FoundItemzTypeInItemzHierarchy hierarchyid

DECLARE @FoundRootBaselineItemzHierarchy hierarchyid
DECLARE @FoundRootBaselineitemzHierarchyRecordCount int
DECLARE @FoundProjectBaselineItemzHierarchy hierarchyid
DECLARE @FoundProjectBaselineItemzHierarchyRecordCount int
DECLARE @SourceProjectItemzHierarchyId hierarchyid
DECLARE @NewBaselineItemzHierarchyIDString varchar(127)
DECLARE @NextAvailableBaselineHierarchyIdUnderProject hierarchyid


	INSERT into [dbo].[Baseline] (Id, Name, Description, CreatedBy, ProjectId)
	VALUES (@NewBaselineID, @Name, @Description, @CreatedBy, @ProjectId)


	INSERT into [dbo].[BaselineItemzType] (ItemzTypeId, Name, Status, Description, CreatedBy,CreatedDate,IsSystem,BaselineId)	
	SELECT Id, Name, Status, Description, CreatedBy, CreatedDate, IsSystem, @NewBaselineID
	FROM [dbo].[ItemzTypes]
	WHERE ProjectId=@ProjectId and Id = @ItemzTypeId


	-- Make sure that BaselineItemzHierarchy table has root repository hierarchyID

	SELECT @FoundRootBaselineItemzHierarchy = BaselineItemzHierarchyId
	FROM BaselineItemzHierarchy
	WHERE BaselineItemzHierarchyId.GetLevel() = 0 and RecordType = 'Repository'
	
	SET @FoundRootBaselineitemzHierarchyRecordCount = @@ROWCOUNT

	IF(@FoundRootBaselineitemzHierarchyRecordCount <> 1) -- If no records found for Repository root level in BaselineItemzHierarchy
		BEGIN
			INSERT into [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
			SELECT id,RecordType, ItemzHierarchyId, ItemzHierarchyId, 1 as isIncluded , Name
			FROM ItemzHierarchy
			WHERE ItemzHierarchyId.GetLevel() = 0 and RecordType = 'Repository'
		END
	
	-- FIND OUT ROOT BASELINE ITEMZ HIERARCHY ONE MORE TIME AS IT MIGHT HAVE BEEN INSERTED JUST NOW AS PER ABOVE IF BLOCK

	SELECT @FoundRootBaselineItemzHierarchy = BaselineItemzHierarchyId
	FROM BaselineItemzHierarchy
	WHERE BaselineItemzHierarchyId.GetLevel() = 0 
			AND RecordType = 'Repository'

	SELECT @FoundProjectBaselineItemzHierarchy = BaselineItemzHierarchyId
	FROM BaselineItemzHierarchy
	WHERE Id = @ProjectId 
			AND RecordType = 'Project'
			AND BaselineItemzHierarchyId.GetLevel() = 1

	SET @FoundProjectBaselineItemzHierarchyRecordCount = @@ROWCOUNT

	IF(@FoundProjectBaselineItemzHierarchyRecordCount <> 1) -- If no records found for target project in BaselineItemzHierarchy
		BEGIN
			INSERT into [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
			values (@ProjectId, 'Project',
						@FoundRootBaselineItemzHierarchy.GetDescendant(
							(
								select max(BaselineItemzHierarchyId).ToString()
								from BaselineItemzHierarchy 
								WHERE BaselineItemzHierarchyId.GetAncestor(1) =  @FoundRootBaselineItemzHierarchy
							)
						,null),
						(
							SELECT ItemzHierarchyId
								FROM ItemzHierarchy
								WHERE Id = @ProjectId and RecordType = 'Project'
						)
						, 1 -- Value for isIncluded
						,(
							SELECT TOP (1) [Name] 
							FROM [dbo].[Projects]
							WHERE [dbo].[Projects].[Id] = @ProjectId
						)
				   )
		END

	-- FIND OUT PROJECT BASELINE ITEMZ HIERARCHY ONE MORE TIME AS IT MIGHT HAVE BEEN INSERTED JUST NOW AS PER ABOVE IF BLOCK

	SELECT @FoundProjectBaselineItemzHierarchy = BaselineItemzHierarchyId
	FROM BaselineItemzHierarchy
	WHERE Id = @ProjectId 
			AND RecordType = 'Project'
			AND BaselineItemzHierarchyId.GetLevel() = 1


		-- INSERT NEW ENTRY IN BASELINE ITEMZ HIERARCHY TABLE FOR THE NEWLY ADDED BASELINE.
		-- WE WILL THEM MOVE IT UNDER PROJECT. 

		SET @NewBaselineItemzHierarchyIDString = (select  CONCAT('/', floor(99999 * RAND(convert(varbinary, newid()))), '/'))

		INSERT into [dbo].[BaselineItemzHierarchy] (Id, RecordType,BaselineItemzHierarchyId,isIncluded,Name)
					values (@NewBaselineID, 'Baseline', @NewBaselineItemzHierarchyIDString,1,@Name)



		-- NOW INSERT BASELINE ITEMZTYPE HIERARCHY ENTRIES IN BASELINEITEMZHIERARCHY TABLE
		SELECT @SourceProjectItemzHierarchyId = ItemzHierarchyId FROM ItemzHierarchy where id = @ProjectId
		INSERT into [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
		SELECT baselineIT.id as Id
			, 'BaselineItemzType'
			,	CASE 
				WHEN LEFT(ith.ItemzHierarchyId.ToString(), LEN(@SourceProjectItemzHierarchyId.ToString())) = @SourceProjectItemzHierarchyId.ToString()
					THEN @NewBaselineItemzHierarchyIDString + SUBSTRING(ith.ItemzHierarchyId.ToString(), (LEN(@SourceProjectItemzHierarchyId.ToString())+1), LEN(ith.ItemzHierarchyId.ToString()))
				ELSE '/999' +  LEFT(@NewBaselineItemzHierarchyIDString, LEN(@NewBaselineItemzHierarchyIDString) - 1) + ith.ItemzHierarchyId.ToString()
				END
			as BaselineItemzHierarchyId
			,	CASE 
				WHEN LEFT(ith.ItemzHierarchyId.ToString(), LEN(@SourceProjectItemzHierarchyId.ToString())) = @SourceProjectItemzHierarchyId.ToString()
					THEN ith.ItemzHierarchyId.ToString()
				ELSE '/999' + ith.ItemzHierarchyId.ToString()
				END
			as SourceItemzHierarchyId
			, 1 as isIncluded
			,ith.Name
		FROM [dbo].[ItemzHierarchy] as ith
		INNER JOIN [dbo].[BaselineItemzType] as baselineIT 
			ON baselineIT.ItemzTypeId = ith.Id
		WHERE ith.ItemzHierarchyId.GetAncestor(1) = (SELECT ItemzHierarchyId from [dbo].[ItemzHierarchy] where [dbo].[ItemzHierarchy].[Id] = @ProjectId  )
				AND baselineIT.BaselineId = @NewBaselineID
		ORDER BY ith.ItemzHierarchyId

	

	DECLARE @TempBaselineItemzType TABLE (
		idx int Primary Key IDENTITY(1,1),
		TempBaselineItemzTypeID [uniqueidentifier],
		TempItemzTypeID [uniqueidentifier])

	INSERT @TempBaselineItemzType
	SELECT Id, ItemzTypeId from [dbo].[BaselineItemzType] WHERE BaselineId=@NewBaselineId

	-- Enumerate through @TempBaselineItemzType
	SET @TempBaselineItemzTypeIterator = 1
	SET @TempBaselineItemzTypeNumberOfRows = (SELECT COUNT(*) FROM @TempBaselineItemzType)
	SET @TempBaselineItemzNumberOfRows = 0

	IF @TempBaselineItemzTypeNumberOfRows > 0
		WHILE (@TempBaselineItemzTypeIterator <= @TempBaselineItemzTypeNumberOfRows)
		BEGIN
			-- get the next BaselineItemzType
			SET @CurrentBaselineItemzTypeID = (SELECT TempBaselineItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
			SET @CurrentItemzTypeID = (SELECT TempItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
			
			-- Insert records in BaselineItemz from Itemzs table.

			-- EXPLANATION: Local declare of variables within WHILE block

			SET @FoundItemzTypeInItemzHierarchy = (Select ItemzHierarchyId FROM [dbo].[ItemzHierarchy] where Id = @CurrentItemzTypeID)

			-- EXPLANATION: We are explicitely looking for Itemz within ItemzType where GetLevel() is EQUAL to 3. 
			-- This way, we only insert immediate child of ItemzType instead of entire tree. This is important because
			-- we need to establish BaselineItemz and BaselineItemzTypeJoinBaselinItemz relationship 
			-- before inserting remaining Itemzchild hierarchy tree nodes. 

			INSERT into [dbo].[BaselineItemz] (ItemzId, Name, Status, Priority, Description, CreatedBy,CreatedDate,Severity,IgnoreMeBaselineItemzTypeId,isIncluded)   
			SELECT itz.Id, itz.Name, itz.Status, itz.Priority, itz.Description, itz.CreatedBy, itz.CreatedDate, itz.Severity , @CurrentBaselineItemzTypeID, 1 as isIncluded
			FROM [dbo].[Itemzs] as itz
			WHERE itz.Id in ( 
								SELECT ith.id
								FROM [dbo].[ItemzHierarchy] as ith
								WHERE ItemzHierarchyId.IsDescendantOf(@FoundItemzTypeInItemzHierarchy) = 1 
								and RecordType = 'Itemz' 
								and ith.ItemzHierarchyId.GetLevel() = 3
							)

			--EXPLANATION: Marking following INSERT into statement as commented becauase we are now switching over to ItemzHierarchy to find all tree Itemz nodes below a given ItemzType.
			--INSERT into [dbo].[BaselineItemz] (ItemzId, Name, Status, Priority, Description, CreatedBy,CreatedDate,Severity,IgnoreMeBaselineItemzTypeId,isIncluded)   
			--SELECT itz.Id, itz.Name, itz.Status, itz.Priority, itz.Description, itz.CreatedBy, itz.CreatedDate, itz.Severity , @CurrentBaselineItemzTypeID, 1 as isIncluded
			--FROM [dbo].[Itemzs] as itz
			--LEFT JOIN [dbo].[ItemzTypeJoinItemz] as itji ON itji.ItemzId = itz.id
			--WHERE itji.ItemzTypeId=@CurrentItemzTypeID

			-- Insert records into BaselineItemzTypeJoinBaselineItemz
			-- EXPLANATION: Because we have just added records in 
			-- BaselineItemz table that included details about BaselineItemzType as
			-- part of [dbo].[BaselineItemz].IgnoreMeBaselineItemzTypeId column, we are
			-- now able to run a simple Select Querty as part of INSERT INTO command
			-- that identifies all the latest records that were added into BaselineItemz. 
			-- Remember that BaselineItemz table auto generates GUID for it's ID column. This ID
			-- is unknown to the Stored Procedure and instead of creating temporary table
			-- we decided to include a new column called as IgnoreMeBaselineItemzTypeId. Now we
			-- are able to query the actual BaselineItemz that were added as part of the currently
			-- processed Itemz.

			INSERT INTO [dbo].[BaselineItemzTypeJoinBaselineItemz] (BaselineItemzTypeId,BaselineItemzId)
			SELECT blitz.IgnoreMeBaselineItemzTypeId, blitz.id
			FROM [dbo].[BaselineItemz] AS blitz
			Where blitz.IgnoreMeBaselineItemzTypeId = @CurrentBaselineItemzTypeID

			-- EXPLANATION: We are explicitely looking for Itemz within ItemzType where GetLevel() is GREATER THEN 3. 
			-- This way, we insert remaining child node tree Itemz into BaselineItemz. 

			INSERT into [dbo].[BaselineItemz] (ItemzId, Name, Status, Priority, Description, CreatedBy,CreatedDate,Severity,IgnoreMeBaselineItemzTypeId,isIncluded)   
			SELECT itz.Id, itz.Name, itz.Status, itz.Priority, itz.Description, itz.CreatedBy, itz.CreatedDate, itz.Severity , @CurrentBaselineItemzTypeID, 1 as isIncluded
			FROM [dbo].[Itemzs] as itz
			WHERE itz.Id in ( 
								SELECT ith.id
								FROM [dbo].[ItemzHierarchy] as ith
								WHERE ItemzHierarchyId.IsDescendantOf(@FoundItemzTypeInItemzHierarchy) = 1 
								and RecordType = 'Itemz' 
								and ith.ItemzHierarchyId.GetLevel() > 3
							)

			--increment Number of BaselineItemz created in this iteration of BaselineType
			SET @TempBaselineItemzNumberOfRows = @TempBaselineItemzNumberOfRows + ( 
					SELECT count(1) 
					FROM [dbo].[BaselineItemz] AS blitz
					Where blitz.IgnoreMeBaselineItemzTypeId =  @CurrentBaselineItemzTypeID)

			-- increment counter for next employee
			SET @TempBaselineItemzTypeIterator = @TempBaselineItemzTypeIterator + 1
		END



		SET @TempBaselineItemzTypeIterator = 1
		WHILE (@TempBaselineItemzTypeIterator <= @TempBaselineItemzTypeNumberOfRows)
			BEGIN
				-- get the next BaselineItemzType
				DECLARE @CurrentItemzTypeHierarchyId hierarchyId
			
				SET @CurrentBaselineItemzTypeID = (SELECT TempBaselineItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
				SET @CurrentItemzTypeID = (SELECT TempItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
				SET @CurrentItemzTypeHierarchyId = (SELECT ItemzHierarchyId from [dbo].[ItemzHierarchy] where [dbo].[ItemzHierarchy].[Id] = @CurrentItemzTypeID)

				-- NOW INSERT BASELINE ITEMZ AT LEVEL 3 HIERARCHY ENTRIES IN BASELINEITEMZHIERARCHY TABLE
				SELECT @SourceProjectItemzHierarchyId = ItemzHierarchyId FROM ItemzHierarchy where id = @ProjectId
				INSERT into [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
				SELECT bi.id as Id
					, 'BaselineItemz'
					,	CASE 
						WHEN LEFT(ith.ItemzHierarchyId.ToString(), LEN(@SourceProjectItemzHierarchyId.ToString())) = @SourceProjectItemzHierarchyId.ToString()
							THEN @NewBaselineItemzHierarchyIDString + SUBSTRING(ith.ItemzHierarchyId.ToString(), (LEN(@SourceProjectItemzHierarchyId.ToString())+1), LEN(ith.ItemzHierarchyId.ToString()))
						ELSE '/999' +  LEFT(@NewBaselineItemzHierarchyIDString, LEN(@NewBaselineItemzHierarchyIDString) - 1) + ith.ItemzHierarchyId.ToString()
						END
					as BaselineItemzHierarchyId
					,	CASE 
						WHEN LEFT(ith.ItemzHierarchyId.ToString(), LEN(@SourceProjectItemzHierarchyId.ToString())) = @SourceProjectItemzHierarchyId.ToString()
							THEN ith.ItemzHierarchyId.ToString()
						ELSE '/999' + ith.ItemzHierarchyId.ToString()
						END
					as SourceItemzHierarchyId
					, 1 as isIncluded
					, ith.Name
				FROM [dbo].[ItemzHierarchy] as ith
				INNER JOIN [dbo].[BaselineItemz] as bi
					ON bi.ItemzId = ith.Id
				INNER JOIN [dbo].[BaselineItemzType] as baselineIT
					on baselineIT.Id = bi.IgnoreMeBaselineItemzTypeId
				WHERE ith.ItemzHierarchyId.IsDescendantOf(@CurrentItemzTypeHierarchyId) = 1
						AND ith.ItemzHierarchyId.GetLevel() > 2 -- only pickup Itemz and not ItemzType
						AND baselineIT.BaselineId = @NewBaselineID
				ORDER BY ith.ItemzHierarchyId

				-- increment counter for next employee
				SET @TempBaselineItemzTypeIterator = @TempBaselineItemzTypeIterator + 1
			END

		-- HERE IS THE MOVE FROM TEMPORARY BASELINE ITEMZ HIERARCHY OVER TO 
		-- IMMEDIATE CHILD BELOW TARGET PROJECT WITHIN BASELINEITEMZHIERARCHY TABLE

		SET @NextAvailableBaselineHierarchyIdUnderProject = @FoundProjectBaselineItemzHierarchy.GetDescendant(
																(
																	select max(BaselineItemzHierarchyId).ToString()
																	from BaselineItemzHierarchy 
																	WHERE BaselineItemzHierarchyId.GetAncestor(1) =  @FoundProjectBaselineItemzHierarchy
																)
															,null)

		UPDATE BaselineItemzHierarchy
		SET BaselineItemzHierarchyId =  @NextAvailableBaselineHierarchyIdUnderProject.ToString()
											+ SUBSTRING(BaselineItemzHierarchyId.ToString(), (LEN(@NewBaselineItemzHierarchyIDString)+1), LEN(BaselineItemzHierarchyId.ToString()))
		WHERE BaselineItemzHierarchyId.IsDescendantOf(@NewBaselineItemzHierarchyIDString) = 1 ; 


		---- Inserting BaselineItemzJoinItemzTraces

		DECLARE @TempNewlyCreatedBaselineItemzIds TABLE (
		idx int Primary Key IDENTITY(1,1),
		TempBaselineItemzID [uniqueidentifier],
		TempItemzId [uniqueidentifier])

		INSERT @TempNewlyCreatedBaselineItemzIds
			SELECT bi.id, bi.ItemzId FROM BaselineItemz bi
			WHERE bi.id IN (SELECT id FROM BaselineItemzHierarchy AS bih WHERE BaselineItemzHierarchyId.IsDescendantOf
			( (SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @NewBaselineID)) = 1
			AND bih.RecordType = 'BaselineItemz'
			AND bih.BaselineItemzHierarchyId.GetLevel() > 3)

		INSERT INTO [dbo].[BaselineItemzJoinItemzTrace] (BaselineFromItemzId,BaselineToItemzId,BaselineId,TraceLabel)
		SELECT bi1.Id AS BaselineItemzFromIdTrace
		, bi2.id AS BaselineItemzToIdTrace 
		, @NewBaselineID AS NewBaselineID
		, ijit.TraceLabel
		FROM [dbo].[ItemzJoinItemzTrace] ijit
		INNER JOIN BaselineItemz bi1 ON bi1.ItemzId = ijit.FromItemzId
		INNER JOIN BaselineItemz bi2 ON bi2.ItemzId = ijit.ToItemzId
		WHERE ijit.FromItemzId IN (SELECT TempItemzId FROM @TempNewlyCreatedBaselineItemzIds)
			AND
			ijit.ToItemzId IN (SELECT TempItemzId FROM @TempNewlyCreatedBaselineItemzIds)
			AND 
			bi1.Id IN (SELECT TempBaselineItemzID FROM @TempNewlyCreatedBaselineItemzIds)
			AND
			bi2.Id IN (SELECT TempBaselineItemzID FROM @TempNewlyCreatedBaselineItemzIds)

if @TempBaselineItemzNumberOfRows = 0
	BEGIN
	RAISERROR (N'ZERO Number of Itemzs records found for the new baseline and so cancelling Baseline Creation operation', -- Message text.  
				16, -- Severity.  
				1 -- State.  
				)
	END

SET @OUTPUT_Id = @NewBaselineID
COMMIT TRANSACTION
END TRY
BEGIN CATCH

IF @@TRANCOUNT > 0
	ROLLBACK TRAN --RollBack in case of Error

DECLARE @ErrorMessage NVARCHAR(4000);  
DECLARE @ErrorSeverity INT;  
DECLARE @ErrorState INT;  
  
SELECT   
	@ErrorMessage = ERROR_MESSAGE(),  
	@ErrorSeverity = ERROR_SEVERITY(),  
	@ErrorState = ERROR_STATE();  
  
-- Use RAISERROR inside the CATCH block to return error  
-- information about the original error that caused  
-- execution to jump to the CATCH block.  
RAISERROR (@ErrorMessage, -- Message text.  
			@ErrorSeverity, -- Severity.  
			@ErrorState -- State.  
			);  
END CATCH
END
