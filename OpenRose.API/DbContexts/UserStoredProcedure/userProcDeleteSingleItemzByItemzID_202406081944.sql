-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcDeleteSingleItemzByItemzID', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcDeleteSingleItemzByItemzID
GO

CREATE PROCEDURE userProcDeleteSingleItemzByItemzID
@ItemzId [uniqueidentifier],
@OUTPUT_Success [bit] OUT

AS

BEGIN
BEGIN TRY

DECLARE @ItemzAsRoot hierarchyID;
DECLARE @RecordTypeToBeItemz NVARCHAR(128)
DECLARE @HierarchyLevel int
DECLARE @ItemzHierarchyRowCount int
DECLARE @DeletedItemzRowCount int
DECLARE @DeletedItemzHierarchyRowCount int

-- FIRST set @OUTPUT_Success to 0 (ZERO) so that default is set to value false.
SET @OUTPUT_Success = 0; -- 1 means true and 0 means false

SELECT @ItemzAsRoot = ItemzHierarchyId
		, @RecordTypeToBeItemz = RecordType
		,  @HierarchyLevel = ItemzHierarchyId.GetLevel()
FROM ItemzHierarchy
WHERE id = @ItemzId

IF ( @@rowcount > 0)
	BEGIN
		SELECT ih.Id
		FROM ItemzHierarchy ih
		WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzAsRoot) = 1 
			AND ih.RecordType = 'Itemz' 
			AND ih.ItemzHierarchyId.GetLevel() > 2 -- WAS THREE
		ORDER BY ih.ItemzHierarchyId

		set @ItemzHierarchyRowCount = @@rowcount
	END;
ELSE
	BEGIN
		set @ItemzHierarchyRowCount = 0
	END;

BEGIN TRANSACTION  

IF ( @ItemzHierarchyRowCount > 0 )
    BEGIN
		
		-- DELETE ITEMZ TRACES FOR THOSE ITEMZ WHERE 
		-- ITEMZ IS A PARENT - TO TRACES
		DELETE FROM [dbo].[ItemzJoinItemzTrace] 
		where [dbo].[ItemzJoinItemzTrace].FromItemzId IN 
		(
			SELECT ih.Id
			FROM ItemzHierarchy ih
			WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzAsRoot) = 1 
				AND ih.RecordType = 'Itemz' 
				AND ih.ItemzHierarchyId.GetLevel() > 2 -- WAS THREE
		)

		-- DELETE ITEMZ TRACES FOR THOSE ITEMZ WHERE 
		-- ITEMZ IS A CHILD - TO TRACES

		DELETE FROM [dbo].[ItemzJoinItemzTrace] 
		where [dbo].[ItemzJoinItemzTrace].ToItemzId IN 
		(
			SELECT ih.Id
			FROM ItemzHierarchy ih
			WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzAsRoot) = 1 
				AND ih.RecordType = 'Itemz' 
				AND ih.ItemzHierarchyId.GetLevel() > 2 -- WAS THREE
		)

		-- NOW FINALLY DELETE Itemz itself.   
		DELETE FROM [dbo].[Itemzs] 
		where [dbo].[Itemzs].Id IN 
		(
			SELECT ih.Id
			FROM ItemzHierarchy ih
			WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzAsRoot) = 1 
				AND ih.RecordType = 'Itemz' 
				AND ih.ItemzHierarchyId.GetLevel() > 2 -- WAS THREE
		)	

		set @DeletedItemzRowCount = @@ROWCOUNT
	
    END;
ELSE
    BEGIN
		-- DELETE ITEMZ TRACES FOR THOSE ITEMZ WHERE 
		-- ITEMZ IS A PARENT - TO TRACES
		DELETE FROM [dbo].[ItemzJoinItemzTrace] 
		where [dbo].[ItemzJoinItemzTrace].FromItemzId = @ItemzId

		-- DELETE ITEMZ TRACES FOR THOSE ITEMZ WHERE 
		-- ITEMZ IS A CHILD - TO TRACES
		DELETE FROM [dbo].[ItemzJoinItemzTrace] 
		where [dbo].[ItemzJoinItemzTrace].ToItemzId = @ItemzId

		-- NOW FINALLY DELETE Itemz itself.   
		DELETE FROM [dbo].[Itemzs] 
		where [dbo].[Itemzs].Id = @ItemzID

		set @DeletedItemzRowCount = @@ROWCOUNT
    END; 

-- DELETE ALL RECORDS WITHIN ITMEZ HIERARCHY FOR ALL DECENDENTS OF A GIVEN ITEMZ AS ROOT.

DELETE FROM ItemzHierarchy 
WHERE ItemzHierarchyId.IsDescendantOf(@ItemzAsRoot) = 1
set @DeletedItemzHierarchyRowCount = @@ROWCOUNT

-- EXPLANATION: In following if condition, we are checking for condition where number of records deleted is greater then 1 instead of 0
-- becauase we use this same stored procedure to delete Orphand Itemz too. It's possible that user may ask to delete one single 
-- ItemzID which is actually an Orphand Itemz and so we are letting 1 additional record be deleted. 
-- One of the thought here is that we can re-establish ItemzHeirarchy manually if we do have access to Itemz data but if we 
-- delete Itemz data from the system then it's not possible to simply bring it back. So we are checking before committing transaction.

IF ( @DeletedItemzRowCount - @DeletedItemzHierarchyRowCount > 1)
	BEGIN
		RAISERROR (N'Number of Itemz deleted are %i which is more then Number of ItemzHierarchy record deleted %i. This Error is raised to safeguard against unexpected data loss in the repository', -- Message text.  
							16, -- Severity.  
							1, -- State.  
							@DeletedItemzRowCount,
							@DeletedItemzHierarchyRowCount
							)	
	END

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
