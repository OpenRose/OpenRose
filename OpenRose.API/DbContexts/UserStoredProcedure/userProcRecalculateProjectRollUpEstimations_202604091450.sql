-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcRecalculateProjectRollUpEstimations', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcRecalculateProjectRollUpEstimations
GO

CREATE PROCEDURE userProcRecalculateProjectRollUpEstimations
@ProjectHierarchyRecordId [uniqueidentifier]

AS

BEGIN
BEGIN TRY
BEGIN TRANSACTION

	-- EXPLANATION: This stored procedure recalculates roll-up estimations for all records 
	-- under a specific project using a CTE-based recursive query combined with a set-based UPDATE.
	-- This approach leverages SQL Server's built-in HierarchyId optimizations and is significantly
	-- more efficient than iterative approaches.
	-- 
	-- The procedure:
	-- 1. Validates the Project record exists
	-- 2. Uses recursive CTE to build the tree of all descendants
	-- 3. Updates all records in a single set-based operation
	-- 4. Calculates RolledUpEstimation = OwnEstimation + SUM(AllDescendants.OwnEstimation)
	-- 
	-- This is an on-demand operation that user can trigger manually from UI.

	DECLARE @ProjectRecord TABLE (
		RecordId [uniqueidentifier],
		RecordType [nvarchar](128),
		HierarchyId [hierarchyid]
	)

	-- Step 1: Validate that the provided ID is a valid Project record
	INSERT INTO @ProjectRecord
	SELECT Id, RecordType, ItemzHierarchyId
	FROM [dbo].[ItemzHierarchy]
	WHERE Id = @ProjectHierarchyRecordId

	IF ((SELECT COUNT(*) FROM @ProjectRecord) = 0)
	BEGIN
		-- EXPLANATION: RAISERROR cannot use expressions directly in parameters.
		-- Must assign to variable first, then pass the variable.
		DECLARE @ProjectIdAsString NVARCHAR(36) = CAST(@ProjectHierarchyRecordId AS NVARCHAR(36))
		RAISERROR (N'Project hierarchy record not found for ID: %s', 
					16, 
					1,
					@ProjectIdAsString)
	END

	IF ((SELECT RecordType FROM @ProjectRecord) != 'Project')
	BEGIN
		RAISERROR (N'Provided record ID is not a Project. Only Project records can be recalculated.', 
					16, 
					2)
	END

	DECLARE @ProjectHierarchyId [hierarchyid]
	SELECT @ProjectHierarchyId = HierarchyId FROM @ProjectRecord

	-- EXPLANATION: Use recursive CTE to fetch all descendants of the project.
	-- This is more efficient than iterating through records because:
	-- 1. Single tree traversal via HierarchyId
	-- 2. All descendants identified in one pass
	-- 3. SQL Server optimizer can parallelize the CTE
	;WITH Tree AS (
		-- Base case: Start with the project record itself
		SELECT 
			Id,
			ItemzHierarchyId,
			OwnEstimation,
			RolledUpEstimation
		FROM [dbo].[ItemzHierarchy]
		WHERE Id = @ProjectHierarchyRecordId

		UNION ALL

		-- Recursive case: Get all immediate children
		SELECT 
			c.Id,
			c.ItemzHierarchyId,
			c.OwnEstimation,
			c.RolledUpEstimation
		FROM [dbo].[ItemzHierarchy] c
		INNER JOIN Tree p
			ON c.ItemzHierarchyId.GetAncestor(1) = p.ItemzHierarchyId
	)
	-- Step 2: Update all records in the tree with their new rolled-up estimations
	-- EXPLANATION: For each record in the tree, calculate its rolled-up estimation as:
	-- OwnEstimation + SUM of all descendant records' OwnEstimation
	-- The IsDescendantOf() function efficiently leverages SQL Server's HierarchyId index
	UPDATE ih
	SET RolledUpEstimation = (
		SELECT 
			ih.OwnEstimation + ISNULL(SUM(descendant.OwnEstimation), 0)
		FROM [dbo].[ItemzHierarchy] descendant
		WHERE descendant.ItemzHierarchyId.IsDescendantOf(ih.ItemzHierarchyId) = 1
			AND descendant.Id != ih.Id  -- Exclude the record itself from the sum
	)
	FROM [dbo].[ItemzHierarchy] ih
	INNER JOIN Tree t ON ih.Id = t.Id

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
