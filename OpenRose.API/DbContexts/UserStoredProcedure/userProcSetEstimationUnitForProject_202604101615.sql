-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcSetEstimationUnitForProject', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcSetEstimationUnitForProject
GO

CREATE PROCEDURE userProcSetEstimationUnitForProject
@ProjectHierarchyRecordId [uniqueidentifier]

AS

BEGIN
BEGIN TRY
BEGIN TRANSACTION

	-- EXPLANATION: This stored procedure synchronizes EstimationUnit for all descendants to match the Project record's value.
	-- Business Rules:
	-- 1. Only Project-type records are the source of EstimationUnit value
	-- 2. All child records (ItemzType, Itemz, etc.) MUST have the SAME EstimationUnit as their Project
	-- 3. Project's EstimationUnit always takes precedence and overrides all child values
	-- 4. Case-sensitive comparison: 'Gbp' and 'GBP' are considered different values
	-- 5. Leading/trailing whitespace is trimmed before comparison and update
	-- 6. Project's EstimationUnit can be NULL or empty string - all descendants will be synchronized to match
	-- 7. Safe to call at any time - synchronizes to current Project EstimationUnit value
	--
	-- Hierarchy Structure:
	-- Project (Level 1) ← Source of EstimationUnit value (takes precedence, can be NULL or empty)
	--   ├─ ItemzType (Level 2)  ← MUST match Project value exactly
	--   │   └─ Itemz (Level 3+)  ← MUST match Project value exactly
	--   └─ ItemzType (Level 2)   ← MUST match Project value exactly
	--       └─ Itemz (Level 3+)   ← MUST match Project value exactly

	DECLARE @TempProjectRecord TABLE (
		RecordId [uniqueidentifier],
		RecordType [nvarchar](128),
		HierarchyId [hierarchyid],
		EstimationUnit [nvarchar](16)
	)

	-- Step 1: Validate that the provided ID is a valid Project record
	INSERT INTO @TempProjectRecord
	SELECT Id, RecordType, ItemzHierarchyId, EstimationUnit
	FROM [dbo].[ItemzHierarchy]
	WHERE Id = @ProjectHierarchyRecordId

	IF ((SELECT COUNT(*) FROM @TempProjectRecord) = 0)
	BEGIN
		-- EXPLANATION: Project record does not exist in ItemzHierarchy
		DECLARE @ProjectIdAsString NVARCHAR(36) = CAST(@ProjectHierarchyRecordId AS NVARCHAR(36))
		RAISERROR (N'Project hierarchy record not found for ID: %s', 
					16, 
					1,
					@ProjectIdAsString)
	END

	IF ((SELECT RecordType FROM @TempProjectRecord) != 'Project')
	BEGIN
		-- EXPLANATION: Only Project-type records can be the source of EstimationUnit
		RAISERROR (N'Provided record ID is not a Project. EstimationUnit synchronization can only be performed from Project level.', 
					16, 
					2)
	END

	-- Step 2: Extract Project-level EstimationUnit
	-- EXPLANATION: 
	-- - @ProjectEstimationUnitTrimmed: Trimmed value used for comparison (removes leading/trailing spaces for cleaner storage)
	-- - Preserves the exact Project value including NULL for consistent synchronization
	DECLARE @ProjectHierarchyId [hierarchyid]
	DECLARE @ProjectEstimationUnitTrimmed [nvarchar](16)
	
	SELECT @ProjectHierarchyId = HierarchyId, 
	       @ProjectEstimationUnitTrimmed = LTRIM(RTRIM(EstimationUnit))
	FROM @TempProjectRecord

	-- Step 3: Use recursive CTE to identify all descendants of the project
	-- EXPLANATION: This CTE efficiently traverses the entire hierarchy tree starting from the Project record
	-- and includes all ItemzType and Itemz records at any depth level
	;WITH Tree AS (
		-- Base case: Start with the project record itself
		SELECT 
			Id,
			ItemzHierarchyId,
			EstimationUnit
		FROM [dbo].[ItemzHierarchy]
		WHERE Id = @ProjectHierarchyRecordId

		UNION ALL

		-- Recursive case: Get all immediate children and their descendants
		SELECT 
			c.Id,
			c.ItemzHierarchyId,
			c.EstimationUnit
		FROM [dbo].[ItemzHierarchy] c
		INNER JOIN Tree p
			ON c.ItemzHierarchyId.GetAncestor(1) = p.ItemzHierarchyId
	)
	-- Step 4: Update all descendant records with Project's EstimationUnit value
	-- EXPLANATION: 
	-- - Project's EstimationUnit ALWAYS takes precedence and overrides ALL child values
	-- - Updates child records ONLY if their trimmed value differs from Project's trimmed value (case-sensitive)
	-- - Handles NULL values: If Project is NULL, all children become NULL; if Project has value, all children get that value
	-- - Case-sensitive comparison using COLLATE Latin1_General_CS_AS
	-- - Trims whitespace from child values for comparison but stores Project's trimmed value
	UPDATE [dbo].[ItemzHierarchy]
	SET EstimationUnit = @ProjectEstimationUnitTrimmed
	FROM [dbo].[ItemzHierarchy] ih
	INNER JOIN Tree t ON ih.Id = t.Id
	WHERE ih.Id != @ProjectHierarchyRecordId  -- Exclude the Project record itself
		AND (
			-- Update if child's trimmed value differs from Project's trimmed value (case-sensitive)
			LTRIM(RTRIM(ih.EstimationUnit)) COLLATE Latin1_General_CS_AS != @ProjectEstimationUnitTrimmed COLLATE Latin1_General_CS_AS
			-- Also update if child's EstimationUnit is NULL but Project's is NOT NULL
			OR (ih.EstimationUnit IS NULL AND @ProjectEstimationUnitTrimmed IS NOT NULL)
			-- Also update if child's EstimationUnit is NOT NULL but Project's IS NULL
			OR (ih.EstimationUnit IS NOT NULL AND @ProjectEstimationUnitTrimmed IS NULL)
		)

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

GO