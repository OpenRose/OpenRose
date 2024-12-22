// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.


using ItemzApp.API.Entities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace ItemzApp.API.DbContexts.SQLHelper
{
    public static class SQLStatements
    {
        #region ItemzChangeHistoryByItemzType

        public static readonly string SQLStatementFor_ItemzChangeHistoryByItemzType =
            "select count(ItemzId) from ItemzChangeHistory " +
            "where ItemzId in (select distinct(ItemzId) " +
            "from ItemzTypeJoinItemz where ItemzTypeId = @__ItemzTypeId__)";

        public static readonly string SQLStatementFor_ItemzChangeHistoryByItemzTypeWithUptoDateTime  = 
            "select count(ItemzId) from ItemzChangeHistory " +
            "where CreatedDate < @__GetUptoDateTime__ " +
            "and ItemzId in " +
            "(select distinct(ItemzId) from ItemzTypeJoinItemz " +
            "where ItemzTypeId = @__ItemzTypeId__)";

        #endregion ItemzChangeHistoryByItemzType

        #region ItemzChangeHistoryByProject

        public static readonly string SQLStatementFor_ItemzChangeHistoryByProject = 
            "select count(ItemzId) from ItemzChangeHistory " +
            "where ItemzId in (select distinct(ItemzId) " +
            "from ItemzTypeJoinItemz " +
            "where ItemzTypeId in " +
            "(select ItemzTypes.Id from ItemzTypes " +
            "where ProjectId = @__ProjectId__))";

        public static readonly string SQLStatementFor_ItemzChangeHistoryByProjectWithUptoDateTime =
            "select count(ItemzId) from ItemzChangeHistory " +
            "where CreatedDate < @__GetUptoDateTime__ " +
            "and ItemzId in " +
            "(select distinct(ItemzId) from ItemzTypeJoinItemz " +
            "where ItemzTypeId in " +
            "(select ItemzTypes.Id from ItemzTypes " +
            "where ProjectId = @__ProjectId__))";

        #endregion ItemzChangeHistoryByProject

        #region ItemzChangeHistoryByRepository

        public static readonly string SQLStatementFor_ItemzChangeHistoryByRepository =
            "select count(ItemzId) from ItemzChangeHistory";

        public static readonly string SQLStatementFor_ItemzChangeHistoryByRepositoryWithUptoDateTime =
            "select count(ItemzId) from ItemzChangeHistory " +
            "where CreatedDate < @__GetUptoDateTime__ ";

        #endregion ItemzChangeHistoryByRepository

        //#region ProjectItemzCount

        //// TODO :  DELETE FOLLOWING SQL STATEMENT FOR SQLStatementFor_GetItemzCountByProject
        ////          AS WE HAVE MOVED NOW TO USE HIERARCHY VIA EF CORE TO INCLUDE SUBITEMZ

        //public static readonly string SQLStatementFor_GetItemzCountByProject = 
        //    "select count(Id) from Itemzs " +
        //    "where Id in (select distinct(ItemzId) " +
        //    "from ItemzTypeJoinItemz " +
        //    "where ItemzTypeId in (select distinct(Id) from ItemzTypes " +
        //    "where ProjectId = @__ProjectID__))";

        //#endregion ProjectItemzCount

        //#region ItemzTypeItemzCount

        ////// TODO :  DELETE FOLLOWING SQL STATEMENT FOR SQLStatementFor_GetItemzCountByItemzType
        //////          AS WE HAVE MOVED NOW TO USE HIERARCHY VIA EF CORE TO INCLUDE SUBITEMZ

        //public static readonly string SQLStatementFor_GetItemzCountByItemzType =
        //    "select count(Id) from Itemzs " +
        //    "where Id in (select distinct(ItemzId) " +
        //    "from ItemzTypeJoinItemz " +
        //    "where ItemzTypeId = @__ItemzTypeID__)";

        //#endregion ItemzTypeItemzCount

        #region GetBaselineItemz

        public static readonly string SQLStatementFor_GetBaselineItemzByItemzIdOrderByCreatedDate =
                "SELECT [bi].* " +
                "FROM [dbo].[BaselineItemz] as [bi] " +
                "INNER JOIN [BaselineItemzHierarchy] as [bih] " +
                "	ON [bih].Id = [bi].Id " +
                "WHERE [bi].ItemzId = @__ItemzID__ AND [bi].id IS NOT NULL " +
                "Order By [bih].BaselineItemzHierarchyId";

        #endregion GetBaselineItemz

        #region BaselineItemzCount

        public static readonly string SQLStatementFor_GetBaselineItemzTraceCountByBaseline =
            "SELECT count(1) as BaselineItemzTraceCount " +
            "from BaselineItemzJoinItemzTrace bijit " +
            "WHERE bijit.BaselineFromItemzId in " +
                "(" +
                    "SELECT bi.id from BaselineItemz bi " +
                    "WHERE bi.id IN ( SELECT Id FROM BaselineItemzHierarchy  " +
                    "WHERE BaselineItemzHierarchyId.IsDescendantOf(  " +
                    "	(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineID__)) = 1 " +
                    "AND RecordType = 'BaselineItemz' " +
                    "AND BaselineItemzHierarchyId.GetLevel() > 3 " +
                    "AND isIncluded = 1)" +
                ")" +
            "AND " +
            "bijit.BaselineToItemzId in " +
                "(" +
                    "SELECT bi.id from BaselineItemz bi " +
                    "WHERE bi.id IN ( SELECT Id FROM BaselineItemzHierarchy  " +
                    "WHERE BaselineItemzHierarchyId.IsDescendantOf(  " +
                    "	(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineID__)) = 1 " +
                    "AND RecordType = 'BaselineItemz' " +
                    "AND BaselineItemzHierarchyId.GetLevel() > 3 " +
                    "AND isIncluded = 1)" +
                ")";

        public static readonly string SQLStatementFor_GetIncludedBaselineItemzCountByBaseline =

            "select count(Id) from BaselineItemzHierarchy " +
            "where BaselineItemzHierarchyId.IsDescendantOf( " +
                "(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineID__)" +
            ") = 1 " +
            "AND RecordType = 'BaselineItemz' " +
            "AND BaselineItemzHierarchyId.GetLevel() > 3 " +
            "AND isIncluded = 1";

        public static readonly string SQLStatementFor_GetExcludedBaselineItemzCountByBaseline =

            "select count(Id) from BaselineItemzHierarchy " +
            "where BaselineItemzHierarchyId.IsDescendantOf( " +
                "(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineID__)" +
            ") = 1 " +
            "AND RecordType = 'BaselineItemz' " +
            "AND BaselineItemzHierarchyId.GetLevel() > 3 " +
            "AND isIncluded = 0";  

        // NOTE: With respect to SQLStatementFor_GetBaselineItemzCountByBaseline
        // System is ignoring IsIncluded property of BaselineItemz while cloning. 
        // i.e. we copy all BaselineItemz without filtering data out based on IsIncluded
        public static readonly string SQLStatementFor_GetBaselineItemzCountByBaseline =

            "select count(Id) from BaselineItemzHierarchy " +
            "where BaselineItemzHierarchyId.IsDescendantOf( " +
                "(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineID__)" +
            ") = 1 " +
            "AND RecordType = 'BaselineItemz' " +
            "AND BaselineItemzHierarchyId.GetLevel() > 3 ";

        public static readonly string SQLStatementFor_GetBaselineItemzByItemzId =
            "select count(Id) from [dbo].[BaselineItemz] as bi " +
            "where bi.ItemzId = @__ItemzId__";

        public static readonly string SQLStatementFor_GetTotalBaselineItemzInRepository =
            "select count(1) from BaselineItemz";

        #endregion BaselineItemzCount

        #region BaselineItemzCountByBaselineItemzType

        public static readonly string SQLStatementFor_GetBaselineItemzCountByBaselineItemzType =
            "SELECT count(Id) FROM BaselineItemz " + 
            "WHERE Id IN (SELECT id FROM BaselineItemzHierarchy  " + 
            "WHERE BaselineItemzHierarchyId.IsDescendantOf(  " + 
            "	(SELECT BaselineItemzHierarchyId FROM BaselineItemzHierarchy WHERE id = @__BaselineItemzTypeID__)) = 1 " + 
            "AND RecordType = 'BaselineItemz'  " + 
            "AND BaselineItemzHierarchyId.GetLevel() > 3)";

        #endregion BaselineItemzCountByBaselineItemzType

        #region Baseline_OrphanedBaseilneItemzCount

        // TODO :: Improve logic for getting count of OrphanedBaselineItemz now that we implement
        // BaselineItemzHierarchy.

        public static readonly string SQLStatementFor_GetOrphanedBaselineItemzCount =
            "SELECT COUNT(bi.id) FROM [dbo].[BaselineItemz] AS bi " +
            "LEFT JOIN [dbo].[BaselineItemzHierarchy] AS bih " +
            "ON bih.Id = bi.id " +
            "WHERE bih.id IS NULL";

		#endregion Baseline_OrphanedBaseilneItemzCount

		#region OrphanItemz_DeleteAllOrphanedItemz

		public static readonly string SQLStatementFor_DeleteAllOrphanedItemz =
		    "DELETE FROM ITEMZS WHERE ID NOT IN (SELECT id FROM ItemzHierarchy) AND " + 
            "ID NOT IN (SELECT FromItemzId FROM dbo.ItemzJoinItemzTrace) and " + 
            "ID NOT IN (SELECT ToItemzId FROM dbo.ItemzJoinItemzTrace)";

		#endregion OrphanItemz_DeleteAllOrphanedItemz

	}
}
