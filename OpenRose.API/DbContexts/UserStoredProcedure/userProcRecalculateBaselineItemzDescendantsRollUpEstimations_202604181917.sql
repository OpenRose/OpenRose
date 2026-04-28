-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcRecalculateBaselineItemzDescendantsRollUpEstimations', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcRecalculateBaselineItemzDescendantsRollUpEstimations
GO

CREATE PROCEDURE userProcRecalculateBaselineItemzDescendantsRollUpEstimations
@BaselineItemzHierarchyRecordId [uniqueidentifier]

AS

BEGIN
BEGIN TRY
BEGIN TRANSACTION

	-- EXPLANATION: This stored procedure handles PHASE 5: SCENARIO 3 - INCLUDE ALL CHILDREN
	-- 
	-- Complete process in a single atomic transaction:
	-- 1. Validate that the passed record ID is of type 'BaselineItemz'
	-- 2. Recalculate target and all INCLUDED descendants (bottom-up from leaf nodes)
	-- 3. Update all descendants with their new roll-up estimations
	-- 4. Update the target record with its calculated roll-up estimation
	-- 5. Propagate the target's final roll-up value up to all ancestors (up to and including Baseline)
	-- 
	-- Business Rules:
	-- - Process from leaf nodes (deepest level) upward to target record
	-- - For each record: RolledUpEstimation = OwnEstimation + SUM(INCLUDED immediate children's RolledUpEstimation)
	-- - EXCLUDED records: RolledUpEstimation = 0 (not included in parent calculations)
	-- - Stop processing descendants at the target record (don't process ancestors yet)
	-- - Then propagate target's final value up to Baseline record (including Baseline)
	-- - Don't go above Baseline to Project level
	--
	-- CRITICAL OPTIMIZATION: Only processes target's descendants, then target's ancestors up to Baseline

	DECLARE @TargetHierarchyId [hierarchyid]
	DECLARE @TargetLevel INT
	DECLARE @MaxDescendantLevel INT
	DECLARE @CurrentProcessingLevel INT
	DECLARE @TargetRecordType [nvarchar](128)

	-- Step 1: Get target record's hierarchy ID, level, and record type
	SELECT @TargetHierarchyId = BaselineItemzHierarchyId, 
	       @TargetLevel = BaselineItemzHierarchyId.GetLevel(),
	       @TargetRecordType = RecordType
	FROM [dbo].[BaselineItemzHierarchy]
	WHERE Id = @BaselineItemzHierarchyRecordId

	-- EXPLANATION: If target record not found, raise error with record ID for debugging
	IF @TargetHierarchyId IS NULL
	BEGIN
		DECLARE @RecordIdAsString NVARCHAR(36) = CAST(@BaselineItemzHierarchyRecordId AS NVARCHAR(36))
		RAISERROR (N'BaselineItemz record not found for ID: %s', 16, 1, @RecordIdAsString)
	END

	-- Step 2: Validate that the target record is of type 'BaselineItemz'
	-- EXPLANATION: This is critical - we only want to process BaselineItemz records for Scenario 3
	-- We do not want to process BaselineItemzType, Baseline, or any other record types
	IF @TargetRecordType != 'BaselineItemz'
	BEGIN
		DECLARE @InvalidRecordTypeAsString NVARCHAR(36) = CAST(@BaselineItemzHierarchyRecordId AS NVARCHAR(36))
		DECLARE @ActualRecordType NVARCHAR(128) = @TargetRecordType
		RAISERROR (N'Invalid record type for Scenario 3. Record ID %s has type %s, but only BaselineItemz records are allowed. Cannot process roll-up estimation for record type: %s', 
			16, 1, @InvalidRecordTypeAsString, @ActualRecordType, @ActualRecordType)
	END

	-- Step 3: Get all descendants (children) of target record
	-- EXPLANATION: This table will hold all descendants we need to process (excluding target itself)
	DECLARE @DescendantsTable TABLE (
		Id [uniqueidentifier],
		HierarchyId [hierarchyid],
		Level INT,
		OwnEstimation [decimal](18,2),
		IsIncluded [bit]
	)

	INSERT INTO @DescendantsTable
	SELECT 
		Id,
		BaselineItemzHierarchyId,
		BaselineItemzHierarchyId.GetLevel(),
		OwnEstimation,
		isIncluded
	FROM [dbo].[BaselineItemzHierarchy]
	WHERE BaselineItemzHierarchyId.IsDescendantOf(@TargetHierarchyId) = 1
		AND BaselineItemzHierarchyId != @TargetHierarchyId  -- Exclude target itself

	-- EXPLANATION: Find the deepest level to start bottom-up processing from leaf nodes
	SELECT @MaxDescendantLevel = ISNULL(MAX(Level), @TargetLevel)
	FROM @DescendantsTable

	-- Step 4: Create temporary table to hold calculated roll-up values during bottom-up processing
	-- EXPLANATION: We use this table to calculate roll-ups level by level, from deepest to shallowest
	DECLARE @RollUpValuesTable TABLE (
		Id [uniqueidentifier],
		HierarchyId [hierarchyid],
		RolledUpEstimation [decimal](18,2)
	)

	SET @CurrentProcessingLevel = @MaxDescendantLevel

	-- PHASE 1: BOTTOM-UP PROCESSING OF DESCENDANTS
	-- Step 5: Process each level from deepest to shallowest (bottom-up processing)
	-- EXPLANATION: Start at leaf level and work upward, calculating roll-ups at each level
	WHILE @CurrentProcessingLevel > @TargetLevel
	BEGIN
		-- Step 5a: For leaf nodes (deepest level)
		-- EXPLANATION: Leaf nodes have no children, so RolledUpEstimation = OwnEstimation (or 0 if excluded)
		IF @CurrentProcessingLevel = @MaxDescendantLevel
		BEGIN
			INSERT INTO @RollUpValuesTable
			SELECT 
				dt.Id,
				dt.HierarchyId,
				CASE 
					WHEN dt.IsIncluded = 0 THEN CAST(0 AS DECIMAL(18, 2))
					ELSE dt.OwnEstimation
				END
			FROM @DescendantsTable dt
			WHERE dt.Level = @CurrentProcessingLevel
		END
		ELSE
		BEGIN
			-- Step 5b: For non-leaf nodes (parent level)
			-- EXPLANATION: RolledUpEstimation = OwnEstimation + SUM(INCLUDED children's RolledUpEstimation)
			INSERT INTO @RollUpValuesTable
			SELECT 
				dt.Id,
				dt.HierarchyId,
				CASE 
					WHEN dt.IsIncluded = 0 THEN CAST(0 AS DECIMAL(18, 2))
					ELSE dt.OwnEstimation + ISNULL((
						SELECT SUM(rv.RolledUpEstimation)
						FROM @RollUpValuesTable rv
						WHERE rv.HierarchyId.GetAncestor(1) = dt.HierarchyId
					), 0)
				END
			FROM @DescendantsTable dt
			WHERE dt.Level = @CurrentProcessingLevel
		END

		SET @CurrentProcessingLevel = @CurrentProcessingLevel - 1
	END

	-- Step 6: Update database with calculated roll-ups for all descendants
	-- EXPLANATION: Persist the calculated roll-up values back to the database for all children
	UPDATE [dbo].[BaselineItemzHierarchy]
	SET RolledUpEstimation = rv.RolledUpEstimation
	FROM [dbo].[BaselineItemzHierarchy] bih
	INNER JOIN @RollUpValuesTable rv ON bih.Id = rv.Id

	-- PHASE 2: CALCULATE AND UPDATE TARGET RECORD
	-- Step 7: Calculate the target record's roll-up estimation
	-- EXPLANATION: Now calculate the target record's roll-up based on its INCLUDED children
	DECLARE @TargetOwnEstimation [decimal](18,2)
	DECLARE @TargetIsIncluded [bit]
	DECLARE @CalculatedTargetRollUp [decimal](18,2)

	SELECT @TargetOwnEstimation = OwnEstimation, @TargetIsIncluded = isIncluded
	FROM [dbo].[BaselineItemzHierarchy]
	WHERE Id = @BaselineItemzHierarchyRecordId

	-- EXPLANATION: Calculate target's roll-up = OwnEstimation + SUM(INCLUDED immediate children's RolledUpEstimation)
	SET @CalculatedTargetRollUp = CASE 
		WHEN @TargetIsIncluded = 0 THEN CAST(0 AS DECIMAL(18, 2))
		ELSE @TargetOwnEstimation + ISNULL((
			SELECT SUM(rv.RolledUpEstimation)
			FROM @RollUpValuesTable rv
			WHERE rv.HierarchyId.GetAncestor(1) = @TargetHierarchyId
		), 0)
	END

	-- Step 8: Store the old roll-up value to calculate delta for ancestor propagation
	DECLARE @OldTargetRollUp [decimal](18,2)
	SELECT @OldTargetRollUp = RolledUpEstimation
	FROM [dbo].[BaselineItemzHierarchy]
	WHERE Id = @BaselineItemzHierarchyRecordId

	-- Step 9: Update target record with its calculated roll-up
	-- EXPLANATION: Persist the target record's calculated roll-up value
	UPDATE [dbo].[BaselineItemzHierarchy]
	SET RolledUpEstimation = @CalculatedTargetRollUp
	WHERE Id = @BaselineItemzHierarchyRecordId

		-- PHASE 3: PROPAGATE TARGET'S ROLL-UP VALUE TO ANCESTORS UP TO BASELINE
	-- Step 10: Calculate delta (change) from old to new for target record
	-- EXPLANATION: We'll use this delta to update all ancestors
	DECLARE @AncestorDelta [decimal](18,2) = @CalculatedTargetRollUp - @OldTargetRollUp

	-- Step 11: If there's a change, propagate to ancestors
	IF @AncestorDelta != 0
	BEGIN
		-- EXPLANATION: Find the closest Baseline ancestor of the target in a single statement
		-- Using HierarchyID's IsDescendantOf and GetLevel methods for efficiency
		DECLARE @BaselineHierarchyId [hierarchyid]
		
		SELECT TOP 1 @BaselineHierarchyId = BaselineItemzHierarchyId
		FROM [dbo].[BaselineItemzHierarchy]
		WHERE RecordType = 'Baseline'
			AND @TargetHierarchyId.IsDescendantOf(BaselineItemzHierarchyId) = 1
		ORDER BY BaselineItemzHierarchyId.GetLevel() DESC
		
		-- EXPLANATION: If no Baseline found, the hierarchy structure is corrupted
		IF @BaselineHierarchyId IS NULL
		BEGIN
			DECLARE @TargetIdAsString NVARCHAR(36) = CAST(@BaselineItemzHierarchyRecordId AS NVARCHAR(36))
			RAISERROR (N'No parent Baseline record found for BaselineItemz ID %s. The hierarchy structure may be corrupted.', 
				16, 1, @TargetIdAsString)
		END
		
		-- Step 12: Update all ancestors from target's parent up to (and including) Baseline
		-- EXPLANATION: Find all ancestors of the target that are also descendants of the Baseline
		-- This ensures we update all records in the chain from target up to Baseline (inclusive)
		UPDATE [dbo].[BaselineItemzHierarchy]
		SET RolledUpEstimation = RolledUpEstimation + @AncestorDelta
		WHERE @TargetHierarchyId.IsDescendantOf(BaselineItemzHierarchyId) = 1
			AND BaselineItemzHierarchyId.IsDescendantOf(@BaselineHierarchyId) = 1
			AND BaselineItemzHierarchyId != @TargetHierarchyId
			AND isIncluded = 1
		
		-- Step 13: Stop at Baseline - don't update above Baseline to Project
		-- EXPLANATION: The WHERE clause naturally stops at Baseline because:
		-- - Condition 2 (BaselineItemzHierarchyId.IsDescendantOf(@BaselineHierarchyId)) ensures
		--   we only update records that are descendants of or equal to the Baseline
		-- - This prevents updating Project level records above the Baseline
	END

	COMMIT TRANSACTION

END TRY
BEGIN CATCH

	-- EXPLANATION: If any error occurs, rollback entire transaction
	IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION

	DECLARE @ErrorMessage NVARCHAR(4000);  
	DECLARE @ErrorSeverity INT;  
	DECLARE @ErrorState INT;  
	  
	SELECT   
	    @ErrorMessage = ERROR_MESSAGE(),  
	    @ErrorSeverity = ERROR_SEVERITY(),  
	    @ErrorState = ERROR_STATE();  
	  
	-- EXPLANATION: Re-raise the error with original message and severity
	RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);  
END CATCH
END

GO
