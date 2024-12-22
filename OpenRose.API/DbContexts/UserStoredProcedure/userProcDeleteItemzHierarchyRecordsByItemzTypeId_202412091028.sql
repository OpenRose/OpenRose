-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcDeleteItemzHierarchyRecordsByItemzTypeId', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcDeleteItemzHierarchyRecordsByItemzTypeId
GO

CREATE PROCEDURE userProcDeleteItemzHierarchyRecordsByItemzTypeId
@ItemzTypeID [uniqueidentifier],
@OUTPUT_Success [bit] OUT

AS

BEGIN
BEGIN TRY
BEGIN TRANSACTION

-- FIND ITEMZ HIERARCHY RECORD BY ITEMZTYPE ID 
-- IT SHOULD ONLY BE ONE UNIQUE RECORD IN THE ITEMZ HIERARCHY DB
-- BECAUSE id FIELD IN ItemzHierarchy TABLE IS A PRIMARY KEY...
-- WHEN WE SEARCH BY A SINGLE @ItemzTypeID THEN ONLY ONE RECORD SHOULD BE RETURNED

DECLARE @ItemzTypeAsRoot hierarchyID;
DECLARE @RecordTypeToBeItemzType NVARCHAR(128)
declare @ItemzHierarchyRowCount int

-- FIRST set @@OUTPUT_Success to 0 (ZERO) so that default is set to value false.
SET @OUTPUT_Success = 0; -- 1 means true and 0 means false


SELECT @ItemzTypeAsRoot = ItemzHierarchyId, @RecordTypeToBeItemzType = RecordType
FROM ItemzHierarchy
WHERE id = @ItemzTypeID

set @ItemzHierarchyRowCount = @@rowcount

If (@ItemzHierarchyRowCount <> 1) -- If RowCount is not exactly 1 then raise error
	BEGIN
		SET @OUTPUT_Success = 0
		BEGIN
		RAISERROR (N'Root element in ItemzHierarchy is not exactly one. Check if the ItemzHierarchy record is found for the given ItemzType ID in the Database.', -- Message text.  
					16, -- Severity.  
					1 -- State.  
					)
		END
		SET @OUTPUT_Success = 1
	END

IF (@RecordTypeToBeItemzType <> 'ItemzType')
	BEGIN
		SET @OUTPUT_Success = 0
		BEGIN
		RAISERROR ( N'Actual record found by provided ItemzTypeID within ItemzHierarchy is not of Type ''ItemzType'' instead it is of type ''%s''' -- Message text.  
					, 16 -- Severity.  
					, 1  -- State.  
					, @RecordTypeToBeItemzType 
					)
		END
		SET @OUTPUT_Success = 1
	END


-- DELETE ALL FROM TRACES FOR ITEMZ THAT ARE FOUND WITHIN A GIVEN PROJECT

DELETE FROM [dbo].[ItemzJoinItemzTrace] 
where [dbo].[ItemzJoinItemzTrace].FromItemzId IN
    (SELECT ih.Id
    FROM [dbo].[ItemzHierarchy] AS ih
    WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzTypeAsRoot) = 1
	AND RecordType = 'Itemz'
	AND ih.ItemzHierarchyId.GetLevel() > 2)	

-- DELETE ALL TO TRACES FOR ITEMZ THAT ARE FOUND WITHIN A GIVEN PROJECT

DELETE FROM [dbo].[ItemzJoinItemzTrace] 
where [dbo].[ItemzJoinItemzTrace].ToItemzId IN
    (SELECT ih.Id
    FROM [dbo].[ItemzHierarchy] AS ih
    WHERE ih.ItemzHierarchyId.IsDescendantOf(@ItemzTypeAsRoot) = 1
	AND RecordType = 'Itemz'
	AND ih.ItemzHierarchyId.GetLevel() > 2)	


-- DELETE ALL RECORDS WITHIN ITMEZ HIERARCHY FOR ALL DECENDENTS OF A GIVEN ITEMZTYPE AS ROOT.

DELETE FROM ItemzHierarchy 
WHERE ItemzHierarchyId.IsDescendantOf(@ItemzTypeAsRoot) = 1

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
