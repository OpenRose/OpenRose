-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcRecalculateBaselineRollUpEstimations', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcRecalculateBaselineRollUpEstimations
GO

CREATE PROCEDURE userProcRecalculateBaselineRollUpEstimations
@BaselineHierarchyRecordId [uniqueidentifier]

AS

BEGIN
BEGIN TRY
BEGIN TRANSACTION

	-- EXPLANATION: This stored procedure recalculates roll-up estimations for an entire Baseline.
	-- CRITICAL DIFFERENCE from Project: Respects isIncluded flag for each BaselineItemz record.
	--
	-- Business Rules:
	-- 1. If EXCLUDED (isIncluded = 0): RolledUpEstimation = ZERO (always, no exceptions)
	-- 2. If INCLUDED (isIncluded = 1): RolledUpEstimation = OwnEstimation + SUM(INCLUDED Descendants' OwnEstimation)
	-- 3. Only INCLUDED descendants contribute to parent's roll-up calculation
	-- 4. OwnEstimation values are NEVER modified - only RolledUpEstimation is updated
	--
	-- Hierarchy Structure:
	-- Project (Level 0, no estimation needed)
	--   └─ Baseline (Level 1) ← RolledUpEstimation = OwnEstimation + SUM(INCLUDED descendants)
	--       ├─ BaselineItemzType (Level 2) ← RolledUpEstimation = OwnEstimation + SUM(INCLUDED descendants)
	--       │   └─ BaselineItemz (Level 3+) ← RolledUpEstimation = OwnEstimation + SUM(INCLUDED descendants)
	--       │       └─ BaselineItemz (Level 4+) ← RolledUpEstimation = OwnEstimation + SUM(INCLUDED descendants)

	DECLARE @BaselineRecord TABLE (
		RecordId [uniqueidentifier],
		RecordType [nvarchar](128),
		HierarchyId [hierarchyid]
	)

	-- Step 1: Validate that the provided ID is a valid Baseline record
	INSERT INTO @BaselineRecord
	SELECT Id, RecordType, BaselineItemzHierarchyId
	FROM [dbo].[BaselineItemzHierarchy]
	WHERE Id = @BaselineHierarchyRecordId

	IF ((SELECT COUNT(*) FROM @BaselineRecord) = 0)
	BEGIN
		DECLARE @BaselineIdAsString NVARCHAR(36) = CAST(@BaselineHierarchyRecordId AS NVARCHAR(36))
		RAISERROR (N'Baseline hierarchy record not found for ID: %s', 
					16, 
					1,
					@BaselineIdAsString)
	END

	IF ((SELECT RecordType FROM @BaselineRecord) != 'Baseline')
	BEGIN
		RAISERROR (N'Provided record ID is not a Baseline. Roll-up estimation recalculation can only be performed from Baseline level.', 
					16, 
					2)
	END

	DECLARE @BaselineHierarchyId [hierarchyid]
	SELECT @BaselineHierarchyId = HierarchyId FROM @BaselineRecord

	-- EXPLANATION: Use recursive CTE to fetch all descendants of the baseline.
	-- Similar approach to Project, but will respect isIncluded flag during calculation.
	;WITH Tree AS (
		-- Base case: Start with the baseline record itself
		SELECT 
			Id,
			BaselineItemzHierarchyId,
			OwnEstimation,
			isIncluded,
			RolledUpEstimation
		FROM [dbo].[BaselineItemzHierarchy]
		WHERE Id = @BaselineHierarchyRecordId

		UNION ALL

		-- Recursive case: Get all immediate children
		SELECT 
			c.Id,
			c.BaselineItemzHierarchyId,
			c.OwnEstimation,
			c.isIncluded,
			c.RolledUpEstimation
		FROM [dbo].[BaselineItemzHierarchy] c
		INNER JOIN Tree p
			ON c.BaselineItemzHierarchyId.GetAncestor(1) = p.BaselineItemzHierarchyId
	)
	-- Step 2: Update all records in the tree with their new rolled-up estimations
	-- EXPLANATION: For each record in the tree:
	-- - If EXCLUDED: RolledUpEstimation = ZERO
	-- - If INCLUDED: RolledUpEstimation = OwnEstimation + SUM(INCLUDED Descendants' OwnEstimation)
	-- The IsDescendantOf() function efficiently leverages SQL Server's HierarchyId index
	UPDATE bih
	SET RolledUpEstimation = (
		CASE 
			-- CRITICAL RULE: If EXCLUDED, RolledUpEstimation = ZERO
			WHEN bih.isIncluded = 0 THEN CAST(0 AS DECIMAL(18, 2))
			-- If INCLUDED: RolledUpEstimation = OwnEstimation + SUM(INCLUDED Descendants' OwnEstimation)
			ELSE (
				SELECT 
					bih.OwnEstimation + ISNULL(SUM(descendant.OwnEstimation), 0)
				FROM [dbo].[BaselineItemzHierarchy] descendant
				WHERE descendant.BaselineItemzHierarchyId.IsDescendantOf(bih.BaselineItemzHierarchyId) = 1
					AND descendant.Id != bih.Id  -- Exclude the record itself from the sum
					AND descendant.isIncluded = 1  -- Only sum INCLUDED descendants
			)
		END
	)
	FROM [dbo].[BaselineItemzHierarchy] bih
	INNER JOIN Tree t ON bih.Id = t.Id

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
	  
	RAISERROR (@ErrorMessage, -- Message text.  
	            @ErrorSeverity, -- Severity.  
	            @ErrorState -- State.  
	            );  
END CATCH
END

GO
