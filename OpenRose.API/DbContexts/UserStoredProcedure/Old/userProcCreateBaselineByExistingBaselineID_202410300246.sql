-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcCreateBaselineByExistingBaselineID', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcCreateBaselineByExistingBaselineID
GO

CREATE PROCEDURE userProcCreateBaselineByExistingBaselineID
@BaselineId [uniqueidentifier],
@Name [nvarchar](128),
@Description [varchar](max),
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
DECLARE @TempBaselineItemzNumberOfTrace int
DECLARE @TempNewlyCreatedBaselineItemzNumberOfTrace int
DECLARE @CurrentBaselineItemzTypeID [uniqueidentifier]
DECLARE @CurrentItemzTypeID [uniqueidentifier]
DECLARE @CurrentBaselineItemzTypeHierarchyId hierarchyid
DECLARE @FoundProjectBaselineItemzHierarchy hierarchyid
DECLARE @FoundProjectBaselineItemzHierarchyRecordCount int
DECLARE @NextAvailableBaselineHierarchyIdUnderProject hierarchyid
DECLARE @SourceBaselineItemzHierarchyId hierarchyid


	INSERT into [dbo].[Baseline] (Id, Name, Description, CreatedBy, ProjectId)
	SELECT @NewBaselineID, @Name, @Description, @CreatedBy, ProjectId from Baseline where id = @BaselineId


	INSERT into [dbo].[BaselineItemzType] (ItemzTypeId, Name, Status, Description, CreatedBy,CreatedDate,IsSystem,BaselineId)	
	SELECT ItemzTypeId, Name, Status, Description, CreatedBy, CreatedDate, IsSystem, @NewBaselineID
	FROM [dbo].[BaselineItemzType]
	WHERE BaselineId = @BaselineId


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
	SET @TempBaselineItemzNumberOfTrace = 0
	set @TempNewlyCreatedBaselineItemzNumberOfTrace = 0

	IF @TempBaselineItemzTypeNumberOfRows > 0
		WHILE (@TempBaselineItemzTypeIterator <= @TempBaselineItemzTypeNumberOfRows)
		BEGIN
			-- get the next BaselineItemzType
			SET @CurrentBaselineItemzTypeID = (SELECT TempBaselineItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
			SET @CurrentItemzTypeID = (SELECT TempItemzTypeID from  @TempBaselineItemzType WHERE idx = @TempBaselineItemzTypeIterator)
			SET @CurrentBaselineItemzTypeHierarchyId = (SELECT BaselineItemzHierarchyId from BaselineItemzHierarchy where id = (SELECT id FROM BaselineItemzType where BaselineId = @BaselineId AND ItemzTypeId = @CurrentItemzTypeID ))
			
			-- Insert records in BaselineItemz from BaselineItemz table itself.
			-- TODO : WE HAVE TO CHANGE THIS LOGIC TO USE BASELINEITEMZHIERARCHY TO FIND ALL BASELINE ITEMZ INSTEAD OF 
			-- BASELINEITEMZTYPEJOINBASEILNEITEMZ TABLE as it only shows data that is belonging to BaselineType only. 
			--INSERT into [dbo].[BaselineItemz] (ItemzId, Name, Status, Priority, Description, CreatedBy,CreatedDate,Severity,IgnoreMeBaselineItemzTypeId,isIncluded)   
			--SELECT blitz.ItemzId, blitz.Name, blitz.Status, blitz.Priority, blitz.Description, blitz.CreatedBy, blitz.CreatedDate, blitz.Severity , @CurrentBaselineItemzTypeID, blitz.isIncluded
			--FROM [dbo].[BaselineItemz] as blitz
			--LEFT JOIN BaselineItemzTypeJoinBaselineItemz as bitjbi on bitjbi.BaselineItemzId = blitz.Id
			--LEFT JOIN BaselineItemzType as tbl_bit on tbl_bit.id = bitjbi.BaselineItemzTypeId
			--WHERE tbl_bit.BaselineId = @BaselineId
			--	AND tbl_bit.ItemzTypeId = @CurrentItemzTypeID
			--	AND blitz.id IS NOT NULL
			--	AND tbl_bit.ID IS NOT NULL

			INSERT into [dbo].[BaselineItemz] (ItemzId, Name, Status, Priority, Description, CreatedBy,CreatedDate,Severity,IgnoreMeBaselineItemzTypeId,isIncluded)   
			SELECT blitz.ItemzId, blitz.Name, blitz.Status, blitz.Priority, blitz.Description, blitz.CreatedBy, blitz.CreatedDate, blitz.Severity , @CurrentBaselineItemzTypeID, blitz.isIncluded
			FROM BaselineItemzHierarchy as bih  
			LEFT JOIN [dbo].[BaselineItemz] as blitz on blitz.id = bih.Id
			WHERE bih.BaselineItemzHierarchyId.IsDescendantOf(@CurrentBaselineItemzTypeHierarchyId) = 1
				  and bih.RecordType = 'BaselineItemz'
				  and bih.BaselineItemzHierarchyId.GetLevel() > 3
			Order By bih.BaselineItemzHierarchyId


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


			-- TODO : CONVERT THIS FOLLOWING LOGIC TO USE BASELINE HIERARCHY ID TO LINK BASELINEITEMZTYPE TO BASELINEITEMZ
			--INSERT INTO [dbo].[BaselineItemzTypeJoinBaselineItemz] (BaselineItemzTypeId,BaselineItemzId)
			--SELECT blitz.IgnoreMeBaselineItemzTypeId, blitz.id
			--FROM [dbo].[BaselineItemz] AS blitz
			--Where blitz.IgnoreMeBaselineItemzTypeId = @CurrentBaselineItemzTypeID

			INSERT INTO [dbo].[BaselineItemzTypeJoinBaselineItemz] (BaselineItemzTypeId,BaselineItemzId)
			SELECT blitz.IgnoreMeBaselineItemzTypeId, blitz.id
			FROM [dbo].[BaselineItemz] AS blitz
			Where blitz.ItemzID IN ( 
										SELECT ItemzId from
										BaselineItemz WHERE id in 
										(
											SELECT id 
											FROM BaselineItemzHierarchy as bih  
											WHERE bih.BaselineItemzHierarchyId.GetAncestor(1) = @CurrentBaselineItemzTypeHierarchyId.ToString()
												and bih.RecordType = 'BaselineItemz'
										)
									)
					AND blitz.IgnoreMeBaselineItemzTypeId = @CurrentBaselineItemzTypeID

			--increment Number of BaselineItemz created in this iteration of BaselineType
			SET @TempBaselineItemzNumberOfRows = @TempBaselineItemzNumberOfRows + ( 
					SELECT count(1) 
					FROM [dbo].[BaselineItemz] AS blitz
					Where blitz.IgnoreMeBaselineItemzTypeId =  @CurrentBaselineItemzTypeID)

			-- increment counter for next BaselineItemzTypeIterator
			SET @TempBaselineItemzTypeIterator = @TempBaselineItemzTypeIterator + 1
		END

		-- >>>>>>> START >>>>>>>>

		SELECT @FoundProjectBaselineItemzHierarchy = BaselineItemzHierarchyId
		FROM BaselineItemzHierarchy
		WHERE RecordType = 'Project'
				AND BaselineItemzHierarchyId.GetLevel() = 1
				AND BaselineItemzHierarchy.Id in (SELECT TOP 1 ProjectId from  Baseline where Id = @BaselineId)

		SET @FoundProjectBaselineItemzHierarchyRecordCount = @@ROWCOUNT

		IF(@FoundProjectBaselineItemzHierarchyRecordCount <> 1) -- If no records found for target project in BaselineItemzHierarchy
			BEGIN
			RAISERROR (N'Could not identify Project Hierarchy ID for the provided BaselineId via parameter', -- Message text.  
						16, -- Severity.  
						1 -- State.  
						)
			END


		SET @NextAvailableBaselineHierarchyIdUnderProject = @FoundProjectBaselineItemzHierarchy.GetDescendant(
																(
																	select max(BaselineItemzHierarchyId).ToString()
																	from BaselineItemzHierarchy 
																	WHERE BaselineItemzHierarchyId.GetAncestor(1) =  @FoundProjectBaselineItemzHierarchy
																)
															,null)
		
		SELECT @SourceBaselineItemzHierarchyId = BaselineItemzHierarchyId FROM BaselineItemzHierarchy where id = @BaselineId

		-- INSERT NEW BASELINE RECORD IN BaselineItemzHierarchy TABLE

		INSERT INTO [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,isIncluded,Name)
		VALUES (@NewBaselineID, 'Baseline', @NextAvailableBaselineHierarchyIdUnderProject.ToString(),1,@Name)


		-- INSERT BASELINE ITEMZTYPE RECORDS IN BaselineItemzHierarchy TABLE

		INSERT INTO [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
		SELECT bit2.id AS Id
			, bih.RecordType
			,	CASE 
					WHEN LEFT(bih.BaselineItemzHierarchyId.ToString(), LEN(@SourceBaselineItemzHierarchyId.ToString())) = @SourceBaselineItemzHierarchyId.ToString()
						THEN @NextAvailableBaselineHierarchyIdUnderProject.ToString() + SUBSTRING(bih.BaselineItemzHierarchyId.ToString(), (LEN(@SourceBaselineItemzHierarchyId.ToString())+1), LEN(bih.BaselineItemzHierarchyId.ToString()))
					ELSE '/999' +  LEFT(@NextAvailableBaselineHierarchyIdUnderProject.ToString(), LEN(@NextAvailableBaselineHierarchyIdUnderProject.ToString()) - 1) + bih.BaselineItemzHierarchyId.ToString()
				END
			AS BaselineItemzHierarchyId
			, bih.BaselineItemzHierarchyId.ToString() AS SourceItemzHierarchyId
			, bih.isIncluded
			, bih.Name
		FROM BaselineItemzType AS bit1
		LEFT JOIN BaselineItemzType bit2 
			ON bit2.ItemzTypeId = bit1.ItemzTypeId and
				bit2.id != bit1.Id
		LEFT JOIN BaselineItemzHierarchy AS bih
			ON bih.id = bit1.Id
		WHERE bit2.BaselineId = @NewBaselineID  -- ADDED THIS CONDITION
		AND bit1.id IN (SELECT id FROM BaselineItemzHierarchy 
						WHERE BaselineItemzHierarchyId.IsDescendantOf(@SourceBaselineItemzHierarchyId.ToString()) = 1
								AND RecordType = 'BaselineItemzType')
			  AND bit1.ItemzTypeId IN (
									SELECT ItemzTypeId FROM
									BaselineItemzType WHERE id IN 
									(
													SELECT id 
													FROM BaselineItemzHierarchy AS bih  
													WHERE BaselineItemzHierarchyId.IsDescendantOf(@SourceBaselineItemzHierarchyId.ToString()) = 1
														AND RecordType = 'BaselineItemzType'
									)
  							  )
		ORDER BY bih.BaselineItemzHierarchyId

		-- INSERT ALL BASELINE ITEMZ ENTRIES IN BaselineItemzHierarchy TABLE
		INSERT INTO [dbo].[BaselineItemzHierarchy] (Id,RecordType,BaselineItemzHierarchyId,SourceItemzHierarchyId,isIncluded,Name)
		SELECT bi2.id AS Id
			, bih.RecordType
			,	CASE 
					WHEN LEFT(bih.BaselineItemzHierarchyId.ToString(), LEN(@SourceBaselineItemzHierarchyId.ToString())) = @SourceBaselineItemzHierarchyId.ToString()
						THEN @NextAvailableBaselineHierarchyIdUnderProject.ToString() + SUBSTRING(bih.BaselineItemzHierarchyId.ToString(), (LEN(@SourceBaselineItemzHierarchyId.ToString())+1), LEN(bih.BaselineItemzHierarchyId.ToString()))
					ELSE '/999' +  LEFT(@NextAvailableBaselineHierarchyIdUnderProject.ToString(), LEN(@NextAvailableBaselineHierarchyIdUnderProject.ToString()) - 1) + bih.BaselineItemzHierarchyId.ToString()
				END
				AS BaselineItemzHierarchyId
			, bih.BaselineItemzHierarchyId.ToString() AS SourceItemzHierarchyId
			, bih.isIncluded
			, bih.Name
		FROM BaselineItemz AS bi1
		LEFT JOIN BaselineItemz bi2 
			ON bi2.ItemzId = bi1.ItemzId and
				bi2.id != bi1.Id
		LEFT JOIN BaselineItemzHierarchy AS bih
			ON bih.id = bi1.Id
		WHERE bi2.IgnoreMeBaselineItemzTypeId IN (SELECT id FROM BaselineItemzType where BaselineId = @NewBaselineID)
		AND bi1.id IN (SELECT id FROM BaselineItemzHierarchy 
						WHERE BaselineItemzHierarchyId.IsDescendantOf(@SourceBaselineItemzHierarchyId.ToString()) = 1
								AND RecordType = 'BaselineItemz')
				AND bi1.itemzid IN (
									SELECT ItemzId FROM
									BaselineItemz WHERE id IN 
									(
													SELECT id 
													FROM BaselineItemzHierarchy AS bih  
													WHERE BaselineItemzHierarchyId.IsDescendantOf(@SourceBaselineItemzHierarchyId.ToString()) = 1
														AND RecordType = 'BaselineItemz'
									)
  								)
		ORDER BY bih.BaselineItemzHierarchyId

		-- <<<<<<< END <<<<<<<<<<


		/* START copying BaselineItemz TRACE data from source baseline to new target baseline */

		--SET @TempBaselineItemzNumberOfTrace = 
		--	(
		--	Select count(1) 
		--	from BaselineItemzJoinItemzTrace bijit
		--		INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_01 
		--			on bitjbi_01.BaselineItemzId = bijit.BaselineFromItemzId 
		--		INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_02 
		--			on bitjbi_02.BaselineItemzId = bijit.BaselineToItemzId 
		--		INNER JOIN 
		--			(
		--			SELECT bit_inner_01.Id 
		--			FROM dbo.BaselineItemzType bit_inner_01
		--			WHERE bit_inner_01.BaselineId = @BaselineId
		--			) bit_01 
		--			on bit_01.Id = bitjbi_01.BaselineItemzTypeId
		--		INNER JOIN 
		--			(
		--			SELECT bit_inner_02.Id 
		--			FROM dbo.BaselineItemzType bit_inner_02
		--			WHERE bit_inner_02.BaselineId = @BaselineId
		--			) bit_02 
		--			on bit_02.Id = bitjbi_02.BaselineItemzTypeId
		--	)


		--IF @TempBaselineItemzNumberOfTrace > 0
		--BEGIN 
		--	INSERT into [dbo].[BaselineItemzJoinItemzTrace] (BaselineFromItemzId,BaselineToItemzId, BaselineId)
		--	SELECT 
		--		NewFromData.NewFromId AS NewBaselineFromId,
		--		NewToData.NewToId AS NewBaselineToId,
		--		@NewBaselineID
		--	FROM BaselineItemzJoinItemzTrace bijit
		--	INNER JOIN 
		--		(
		--			SELECT bi_Inner_01.Id,bi_Inner_01.ItemzId -- Projection of REQUIRED columns
		--			FROM dbo.BaselineItemz bi_Inner_01
		--		) bi_01 
		--	ON bi_01.Id = bijit.BaselineFromItemzId
		--	INNER JOIN 
		--		(
		--			SELECT bi_Inner_02.Id,bi_Inner_02.ItemzId -- Projection of REQUIRED columns
		--			FROM dbo.BaselineItemz bi_Inner_02
		--		) bi_02 
		--	ON bi_02.id = bijit.BaselineToItemzId
		--	INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_01 
		--		ON bitjbi_01.BaselineItemzId = bijit.BaselineFromItemzId 
		--	INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_02 
		--		ON bitjbi_02.BaselineItemzId = bijit.BaselineToItemzId 
		--	INNER JOIN 
		--		(
		--			SELECT bit_inner_01.Id					-- Projection of REQUIRED columns
		--			FROM dbo.BaselineItemzType bit_inner_01
		--			WHERE bit_inner_01.BaselineId = @BaselineId -- SOURCE BASELINE ID
		--		) bit_01 
		--		ON bit_01.Id = bitjbi_01.BaselineItemzTypeId
		--	INNER JOIN 
		--		(
		--			SELECT bit_inner_02.Id					-- Projection of REQUIRED columns
		--			FROM dbo.BaselineItemzType bit_inner_02
		--			WHERE bit_inner_02.BaselineId = @BaselineId -- SOURCE BASELINE ID
		--		) bit_02 
		--		ON bit_02.Id = bitjbi_02.BaselineItemzTypeId
		--	INNER JOIN 
		--		(
		--			SELECT 
		--				NewFrom.Id AS NewFromId, 
		--				NewFrom.ItemzId AS NewFromItemzId, 
		--				NewFrom.Name 
		--			FROM BaselineItemz NewFrom 
		--			INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_NewFrom 
		--				ON bitjbi_NewFrom.BaselineItemzId = NewFrom.Id
		--			INNER JOIN dbo.BaselineItemzType bit_NewFrom 
		--				ON bit_NewFrom.Id = bitjbi_NewFrom.BaselineItemzTypeId
		--			WHERE bit_NewFrom.BaselineId = @NewBaselineID -- NEW TARGET BASELINE ID
		--		) NewFromData 
		--		ON NewFromData.NewFromItemzId = bi_01.ItemzId
		--	INNER JOIN 
		--		(
		--			SELECT 
		--				NewTo.Id AS NewToId, 
		--				NewTo.ItemzId AS NewToItemzId, 
		--				NewTo.Name 
		--			FROM BaselineItemz NewTo 
		--			INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_NewTo 
		--				ON bitjbi_NewTo.BaselineItemzId = NewTo.Id
		--			INNER JOIN dbo.BaselineItemzType bit_NewTo 
		--				ON bit_NewTo.Id = bitjbi_NewTo.BaselineItemzTypeId
		--			WHERE bit_NewTo.BaselineId = @NewBaselineID -- NEW TARGET BASELINE ID
		--		) NewToData 
		--		ON NewToData.NewToItemzId = bi_02.ItemzId
		--END

		--SET @TempNewlyCreatedBaselineItemzNumberOfTrace = 
		--	(
		--		Select count(1) 
		--		from BaselineItemzJoinItemzTrace bijit
		--			INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_01 
		--				on bitjbi_01.BaselineItemzId = bijit.BaselineFromItemzId 
		--			INNER JOIN dbo.BaselineItemzTypeJoinBaselineItemz bitjbi_02 
		--				on bitjbi_02.BaselineItemzId = bijit.BaselineToItemzId 
		--			INNER JOIN 
		--				(
		--				SELECT bit_inner_01.Id 
		--				FROM dbo.BaselineItemzType bit_inner_01
		--				WHERE bit_inner_01.BaselineId = @NewBaselineID
		--				) bit_01 
		--				on bit_01.Id = bitjbi_01.BaselineItemzTypeId
		--			INNER JOIN 
		--				(
		--				SELECT bit_inner_02.Id 
		--				FROM dbo.BaselineItemzType bit_inner_02
		--				WHERE bit_inner_02.BaselineId = @NewBaselineID
		--				) bit_02 
		--				on bit_02.Id = bitjbi_02.BaselineItemzTypeId
		--	)


		SET @TempBaselineItemzNumberOfTrace = 
			(Select count (1) from BaselineItemzJoinItemzTrace as bijit 
				where bijit.BaselineId = @BaselineId)
		
		-- >>>> START INSERT NEW BASELINE ITEMZ TRACE RECORDS >>>>

		INSERT INTO BaselineItemzJoinItemzTrace(BaselineFromItemzId, BaselineToItemzId, BaselineId)
		SELECT 
			--bijit.BaselineId as OldBaselineId
			--, bijit.BaselineFromItemzId
			--, bi_FromRecords.OldFromBaselineItemzId
			bi_FromRecords.NewFromBaselineItemzId
			--, bijit.BaselineToItemzId
			--, bi_ToRecords.OldToBaselineItemzId
			, bi_ToRecords.NewToBaselineItemzId
			, @NewBaselineId as NewBaselineId
		from BaselineItemzJoinItemzTrace as bijit 
		INNER JOIN (
				Select bi_Old.id as OldFromBaselineItemzId, bi_Old.ItemzId as OldFromItemzId, bi_New.id as NewFromBaselineItemzId, bi_New.NewItemzId as NewFromItemzId from BaselineItemz bi_Old
				INNER JOIN (
								Select bi.id, bi.ItemzId as NewItemzId from BaselineItemz bi
								WHERE id in (
									(SELECT id FROM BaselineItemzHierarchy AS bih WHERE bih.BaselineItemzHierarchyId.IsDescendantOf
									( (SELECT bit_inner01.BaselineItemzHierarchyId FROM BaselineItemzHierarchy AS bit_inner01 
											WHERE bit_inner01.Id = @NewBaselineId)) = 1
									AND bih.RecordType = 'BaselineItemz'
									AND bih.BaselineItemzHierarchyId.GetLevel() > 3)
								)
				)
				as bi_New ON bi_New.NewItemzId = bi_Old.ItemzId
				WHERE bi_Old.id in (
								(SELECT id FROM BaselineItemzHierarchy AS bih WHERE bih.BaselineItemzHierarchyId.IsDescendantOf
								( (SELECT bit_inner01.BaselineItemzHierarchyId FROM BaselineItemzHierarchy AS bit_inner01 
										WHERE bit_inner01.Id = @BaselineId)) = 1
								AND bih.RecordType = 'BaselineItemz'
								AND bih.BaselineItemzHierarchyId.GetLevel() > 3)
							)
	
		) as bi_FromRecords ON bijit.BaselineFromItemzId = bi_FromRecords.OldFromBaselineItemzId 
		INNER JOIN (
				Select bi_Old.id as OldToBaselineItemzId, bi_Old.ItemzId as OldToItemzId, bi_New.id as NewToBaselineItemzId, bi_New.NewItemzId as NewToItemzId from BaselineItemz bi_Old
				INNER JOIN (
								Select bi.id, bi.ItemzId as NewItemzId from BaselineItemz bi
								WHERE id in (
									(SELECT id FROM BaselineItemzHierarchy AS bih WHERE bih.BaselineItemzHierarchyId.IsDescendantOf
									( (SELECT bit_inner01.BaselineItemzHierarchyId FROM BaselineItemzHierarchy AS bit_inner01 
											WHERE bit_inner01.Id = @NewBaselineId)) = 1
									AND bih.RecordType = 'BaselineItemz'
									AND bih.BaselineItemzHierarchyId.GetLevel() > 3)
								)
				)
				as bi_New ON bi_New.NewItemzId = bi_Old.ItemzId
				WHERE bi_Old.id in (
								(SELECT id FROM BaselineItemzHierarchy AS bih WHERE bih.BaselineItemzHierarchyId.IsDescendantOf
								( (SELECT bit_inner01.BaselineItemzHierarchyId FROM BaselineItemzHierarchy AS bit_inner01 
										WHERE bit_inner01.Id = @BaselineId)) = 1
								AND bih.RecordType = 'BaselineItemz'
								AND bih.BaselineItemzHierarchyId.GetLevel() > 3)
							)
		) as bi_ToRecords ON bijit.BaselineToItemzId = bi_ToRecords.OldToBaselineItemzId 
		WHERE bijit.BaselineId = @BaselineId
		AND bijit.BaselineFromItemzId = bi_FromRecords.OldFromBaselineItemzId
		AND bijit.BaselineToItemzId = bi_ToRecords.OldToBaselineItemzId

		-- <<<< END INSERT NEW BASELINE ITEMZ TRACE RECORDS <<<<

		SET @TempNewlyCreatedBaselineItemzNumberOfTrace = 
			(Select count (1) from BaselineItemzJoinItemzTrace as bijit 
				where bijit.BaselineId = @NewBaselineID)

		IF (@TempBaselineItemzNumberOfTrace != @TempNewlyCreatedBaselineItemzNumberOfTrace)
		BEGIN
		RAISERROR (N'Number of traces between old baseline and new baseline did not match and so rolling back creation of new baseline', -- Message text.  
					16, -- Severity.  
					1 -- State.  
					)
		END

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
