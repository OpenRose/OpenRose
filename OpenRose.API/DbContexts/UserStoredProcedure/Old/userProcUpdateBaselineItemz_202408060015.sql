-- OpenRose - Requirements Management
-- Licensed under the Apache License, Version 2.0. 
-- See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

IF OBJECT_ID ( 'userProcUpdateBaselineItemz', 'P' ) IS NOT NULL
    DROP PROCEDURE userProcUpdateBaselineItemz
GO

CREATE PROCEDURE userProcUpdateBaselineItemz
@BaselineId [uniqueidentifier],
@ShouldBeIncluded [bit],
@SingleNodeInclusion [bit],
@BaselineItemzIds [varchar](max),
@OUTPUT_Success [bit] out

AS

BEGIN
BEGIN TRY
DECLARE @TempInputBaselineItemzIterator int
DECLARE @TempInputBaselineItemzNumberOfRows int
DECLARE @FoundInputBaselineItemzWithinBaseline int
DECLARE @CurrentInputBaselineItemzId [uniqueidentifier] 
DECLARE @BaselineIdAsString as NVARCHAR(36)
DECLARE @CurrentInputBaselineItemzIdAsString as NVARCHAR(36)

DECLARE @TempInputBaselineItemzIdsTable TABLE (
	idx int Primary Key IDENTITY(1,1),
	TempInputBaselineItemzId [uniqueidentifier],
	TempInputCurrentBaselineItemzHierarchyId [hierarchyid])

DECLARE @TempSECONDInputBaselineItemzIdsTable TABLE (
	idx int Primary Key IDENTITY(1,1),
	TempInputBaselineItemzId [uniqueidentifier],
	TempInputCurrentBaselineItemzHierarchyId [hierarchyid])

-- FIRST set @OUTPUT_Id to 0 (ZERO) so that default is set to value false.
SET @OUTPUT_Success = 0; -- 1 means true and 0 means false
SET @TempInputBaselineItemzIterator = 1
SET @TempInputBaselineItemzNumberOfRows = 0

INSERT into @TempInputBaselineItemzIdsTable (TempInputBaselineItemzId)
SELECT DISTINCT(RTRIM(LTRIM(value))) from (
					SELECT value FROM STRING_SPLIT( @BaselineItemzIds, ',') 
					) as TempInputBaselineItemzId

UPDATE temp_tibit SET temp_tibit.TempInputCurrentBaselineItemzHierarchyId = bih.BaselineItemzHierarchyId
FROM @TempInputBaselineItemzIdsTable AS temp_tibit 
JOIN BaselineItemzHierarchy as bih
	ON bih.id = temp_tibit.TempInputBaselineItemzId

INSERT INTO @TempSECONDInputBaselineItemzIdsTable (TempInputBaselineItemzId, TempInputCurrentBaselineItemzHierarchyId)
SELECT temp_tibit.TempInputBaselineItemzId, temp_tibit.TempInputCurrentBaselineItemzHierarchyId
FROM @TempInputBaselineItemzIdsTable as temp_tibit
ORDER BY temp_tibit.TempInputCurrentBaselineItemzHierarchyId ASC

SET @TempInputBaselineItemzNumberOfRows = (SELECT COUNT(1) FROM @TempSECONDInputBaselineItemzIdsTable)

IF @TempInputBaselineItemzNumberOfRows > 0
	WHILE (@TempInputBaselineItemzIterator <= @TempInputBaselineItemzNumberOfRows)
	BEGIN

		SET @CurrentInputBaselineItemzId = (SELECT TempInputBaselineItemzId from @TempSECONDInputBaselineItemzIdsTable WHERE idx = @TempInputBaselineItemzIterator)

		SET @FoundInputBaselineItemzWithinBaseline = (SELECT count(1) from BaselineItemzHierarchy
														where id = @CurrentInputBaselineItemzId
														AND BaselineItemzHierarchyId.GetLevel() > 3
														AND RecordType = 'BaselineItemz'
														AND BaselineItemzHierarchyId.IsDescendantOf(
														(select BaselineItemzHierarchyiD FROM BaselineItemzHierarchy 
															WHERE id = @BaselineId
															AND RecordType = 'Baseline'
															AND BaselineItemzHierarchyId.GetLevel() = 2
														) ) = 1)

		IF (@FoundInputBaselineItemzWithinBaseline = 1)
		BEGIN
			IF (@ShouldBeIncluded = 1)
			BEGIN
				DECLARE @ImmediateParentBaselineItemzId [uniqueidentifier]
				DECLARE @ImmediateParentBaselineItemzIncluded [bit]

				SELECT @ImmediateParentBaselineItemzId = bih.id, @ImmediateParentBaselineItemzIncluded = bih.isIncluded	
				FROM BaselineItemzHierarchy AS bih
				WHERE bih.BaselineItemzHierarchyID = (SELECT BaselineItemzHierarchyId.GetAncestor(1) FROM BaselineItemzHierarchy 
														WHERE id = @CurrentInputBaselineItemzId
														AND BaselineItemzHierarchyId.GetLevel() > 3
														AND RecordType = 'BaselineItemz')
				
				IF (@ImmediateParentBaselineItemzIncluded = 0)
				BEGIN
					SET @OUTPUT_Success = 0

					DECLARE @ImmediateParentBaselineItemzIdAsString as NVARCHAR(36) = CAST(@ImmediateParentBaselineItemzId AS NVARCHAR(36))
					SET @CurrentInputBaselineItemzIdAsString = CAST(@CurrentInputBaselineItemzId AS NVARCHAR(36))
					SET @BaselineIdAsString = CAST(@BaselineId AS NVARCHAR(36))

					RAISERROR (N'Could not mark BaselineItemz with Id %s for inclusion as its Immediate Parent with Id %s is currently marked as excluded within baseline with Id %s. Cannot process request to include child BaselineItemz while its parent is excluded.', -- Message text.  
								16, -- Severity.  
								1, -- State.  
								@CurrentInputBaselineItemzIdAsString,
								@ImmediateParentBaselineItemzIdAsString,
								@BaselineIdAsString
								)	
				END
			END

			IF (@ShouldBeIncluded = 1 OR @ShouldBeIncluded = 0)
				BEGIN 
				IF (@ShouldBeIncluded = 1 AND @SingleNodeInclusion =  1 )
					BEGIN
						UPDATE BaselineItemzHierarchy
						SET isIncluded = @ShouldBeIncluded
						WHERE id = @CurrentInputBaselineItemzId

						UPDATE BaselineItemz
						SET isIncluded = @ShouldBeIncluded
						Where id = @CurrentInputBaselineItemzId
					END 
				ELSE
					BEGIN
						UPDATE BaselineItemzHierarchy
						SET isIncluded = @ShouldBeIncluded
						where BaselineItemzHierarchyId.IsDescendantOf(
							(select BaselineItemzHierarchyiD FROM BaselineItemzHierarchy WHERE id = @CurrentInputBaselineItemzId) ) = 1

						UPDATE BaselineItemz
						SET isIncluded = @ShouldBeIncluded
						Where id in ( select id
										from BaselineItemzHierarchy 
										where BaselineItemzHierarchyId.IsDescendantOf(
											(select BaselineItemzHierarchyiD FROM BaselineItemzHierarchy WHERE id = @CurrentInputBaselineItemzId) ) = 1
									)
					END

				SET @TempInputBaselineItemzIterator = @TempInputBaselineItemzIterator + 1
				END
			ELSE
				BEGIN
					SET @OUTPUT_Success = 0
					RAISERROR (N'Value for isIncluded column shall either be 0 (ZERO) or 1 (ONE)', -- Message text.  
								16, -- Severity.  
								1 -- State.  
								)			
				END

			SET @OUTPUT_Success = 1
		END
		ELSE
		BEGIN
			SET @OUTPUT_Success = 0
			
			SET @CurrentInputBaselineItemzIdAsString = CAST(@CurrentInputBaselineItemzId AS NVARCHAR(36))
			SET @BaselineIdAsString = CAST(@BaselineId AS NVARCHAR(36))
			RAISERROR (N'Could not find BaselineItemz with ID %s as child of Baseline with ID %s', -- Message text.  
						16, -- Severity.  
						1, -- State.  
						@CurrentInputBaselineItemzIdAsString,
						@BaselineIdAsString
						)			
		END
	END
END TRY
BEGIN CATCH

--  TODO: TRANSACTION ROLLBACK LOGIC SHOULD BE COVERED VIA WEB API LAYER AS WE ARE BATCHING CHANGES.
--	WE HAVE TO OTHERWISE EVALUATE NESTED TRANSACTION LOG.

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
