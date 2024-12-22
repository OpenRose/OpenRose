// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using ItemzApp.API.DbContexts;
using ItemzApp.API.DbContexts.Extensions;
using ItemzApp.API.DbContexts.SQLHelper;
using ItemzApp.API.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace ItemzApp.API.Services
{
    public class ProjectRepository : IProjectRepository, IDisposable
    {
        private readonly ItemzContext _context;

        public ProjectRepository(ItemzContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Project?> GetProjectAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }

            return await _context.Projects!
                .Where(c => c.Id == ProjectId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<Project?> GetProjectForUpdateAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }

            return await _context.Projects!
                .Where(c => c.Id == ProjectId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Project>?> GetProjectsAsync()
        {
            try
            {
                if (await _context.Projects.CountAsync<Project>() > 0)
                {
                    var projectCollection = await _context.Projects.AsNoTracking().AsQueryable<Project>().OrderBy(p => p.Name).ToListAsync();

                    // TODO: We have to create simple implementation of sort by Project Name here 
                    // projectCollection = projectCollection.ApplySort("Name", null).AsNoTracking();

                    return projectCollection;
                }
                return null;
            }
            catch (Exception ex)
            {
                // TODO: It's not good that we capture Generic Exception and then 
                // return null here. Basically, I wanted to check if we have 
                // projects returned from the DB and if it does not then
                // it should simply return null back to the calling function.
                // One has to learn how to do this gracefully as part of Entity Framework 
                return null;
            }
        }

        public async Task<IEnumerable<ItemzType>?> GetItemzTypesAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }

            if (await _context.ItemzTypes!.Where(it => it.ProjectId == ProjectId).AnyAsync())
            {
                return await _context.ItemzTypes
                    .AsNoTracking()
                    .Where(it => it.ProjectId == ProjectId)
                    .AsQueryable<ItemzType>()
                    .OrderBy(it => it.Name)
                    .ToListAsync();
            }
            return null;
        }
        //TODO: decide if we need GetProjects by passing in collection of projectIds
        // if yes, then we need to implement action method in ProjectController for the same
        // so that Swagger docs shows GET method under Projects section.
        public async Task<IEnumerable<Project>> GetProjectsAsync(IEnumerable<Guid> projectIds)
        {
            if (projectIds == null)
            {
                throw new ArgumentNullException(nameof(projectIds));
            }

            return await _context.Projects.AsNoTracking().Where(a => projectIds.Contains(a.Id))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public void AddProject(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            // EXPLANATION: Everytime a project is created then we have to 
            // also add 'Parking Lot' ItemzType. 

            var parkingLotItemzType = new ItemzType
            {
                Name = "Parking Lot",
                Status = ((ItemzTypeStatus)Enum.Parse(
									typeof(ItemzTypeStatus),
									EntityPropertyDefaultValues.ItemzTypeStatusDefaultValue,
									true)),
                Description = "Parking Lot System ItemzType",
                IsSystem = true
            };

            if (project.ItemzTypes == null)
            {
                project.ItemzTypes = new List<Entities.ItemzType> { parkingLotItemzType };
            }
            else
            {
                var hasSystemType = false;
                foreach( var itemzType in project.ItemzTypes)
                {
                    if (itemzType.IsSystem == true)
                    {
                        hasSystemType = true;
                    }
                }
                if (hasSystemType == false)
                {
                    project.ItemzTypes.Add(parkingLotItemzType);
                }
            }

            _context.Projects!.Add(project);
            }

        public async Task AddNewProjectHierarchyAsync(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var rootItemz = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.ItemzHierarchyId == HierarchyId.Parse("/"));

            if (rootItemz.Count() != 1)
            {
                throw new ApplicationException("Either no Root Repository Hierarchy record " +
                    "found OR multiple Root Repository Hierarchy records found in the system. " +
                    "Please contact your System Administrator.");
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the ItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
            // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

            var projectHierarchyItemz = await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemz.FirstOrDefault()!.ItemzHierarchyId!)
                    .OrderByDescending(ih => ih.ItemzHierarchyId!)
                    .ToListAsync();

            var tempProjectHierarchy = new Entities.ItemzHierarchy
            {
                Id = project.Id,
                RecordType = "Project",
                Name = project.Name,
                ItemzHierarchyId = rootItemz.FirstOrDefault()!.ItemzHierarchyId!
                                    .GetDescendant(projectHierarchyItemz.Count() > 0 
                                                        ?  projectHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId 
                                                        : null
                                                   , null),
                // ItemzHierarchyId = HierarchyId.Parse(newProjectInsertionId)
            };

            _context.ItemzHierarchy!.Add(tempProjectHierarchy);

            // var newParkingLotItemzTypeInsertionId = tempProjectHierarchy.ItemzHierarchyId.ToString() + "1/";

            if (project.ItemzTypes!.Count == 1)
            {
                var tempParkingLotItemzTypeHierarchy = new Entities.ItemzHierarchy
                {
                    Id = project.ItemzTypes[0].Id,
                    RecordType = "ItemzType",
                    Name = project.ItemzTypes[0].Name,
                    ItemzHierarchyId = tempProjectHierarchy.ItemzHierarchyId.GetDescendant(null, null), 
                    //ItemzHierarchyId = HierarchyId.Parse(newParkingLotItemzTypeInsertionId)
                };
                
                _context.ItemzHierarchy!.Add(tempParkingLotItemzTypeHierarchy);
            }
        }

        public async Task DeleteOrphanedBaselineItemzAsync()
        {
            await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcDeleteAllOrphanedBaselineItemz");
        }

        public async Task<bool> SaveAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public async Task<bool> ProjectExistsAsync(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            // EXPLANATION: We expect ProjectExists to be used independently on it's own without
            // expecting it to track the Project that was found in the database. That's why it's not
            // a good idea to use "!(_context.Projects.Find(projectId) == null)" option
            // to "Find()" Project. This is because Find is designed to track the Project in the memory.
            // In "Project Delete controller method", we are first checking if ProjectExists and then 
            // we call Project Delete to actually remove it. This is going to be in the single scoped
            // DBContext. If we use "Find()" method then it will start tracking the Project and then we can't
            // get the Project once again from the DB as it's already being tracked. We have a choice here
            // to decide if we should always use Find via ProjectExists and then yet again in the subsequent
            // operations like Delete / Update or we use ProjectExists as independent method and not rely on 
            // it for subsequent operations like Delete / Update.

            return await _context.Projects.AsNoTracking().AnyAsync(p => p.Id == projectId);
        }

        public void UpdateProject(Project project)
        {
            // Due to Repository Pattern implementation, 
            // there is no code in this implementation. 
        }

        public void DeleteProject(Project project)
        {
            _context.Projects!.Remove(project);

            // EXPLANATION: We need to first get the BaselineItemzHierarchy record from the repository
            // then we use it within IsDescendantOf to find all the child nodes including Baseline node itself
            // and then we remove all those records using EF Core entity tracking feature.

            var baselineItemzHierarchyRecordForProject = _context.BaselineItemzHierarchy!.AsNoTracking()
                .Where(bih => bih.Id == project.Id).FirstOrDefault();

            if (baselineItemzHierarchyRecordForProject != null)
            {
                _context.BaselineItemzHierarchy!.RemoveRange
                    (
                        _context.BaselineItemzHierarchy.Where
                        (
                            bih => bih.BaselineItemzHierarchyId!.IsDescendantOf
                            (
                                baselineItemzHierarchyRecordForProject!.BaselineItemzHierarchyId
                            )
                        ).AsEnumerable()
                    );
            }
            // TODO: If we can't automatically remove Trace data using this method then it's better to remove all data using
            // stored procedure for deleting Baseline. This will be similar approach when it comes to removing Single Itemz like
            // userProcDeleteSingleItemzByItemzID STORED PROCEDURE.
        }

        public async Task<int> GetItemzCountByProjectAsync(Guid ProjectId)
        {
            if (ProjectId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(ProjectId));
            }
            //KeyValuePair<string, object>[] sqlArgs = new KeyValuePair<string, object>[]
            //{
            //    new KeyValuePair<string, object>("@__ProjectId__", ProjectId.ToString()),
            //};
            //var foundItemzByProject = await _context.CountByRawSqlAsync(SQLStatements.SQLStatementFor_GetItemzCountByProject, sqlArgs);

            //return foundItemzByProject;

            var rootProject = _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.Id == ProjectId).FirstOrDefault();

            //var deletemeList = (await _context.ItemzHierarchy!
            //       .AsNoTracking()
            //       .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootProject!.ItemzHierarchyId!)
            //       .ToListAsync());

            var sumOfItemzTypeInTargetProject = (await _context.ItemzHierarchy!
                   .AsNoTracking()
                   .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootProject!.ItemzHierarchyId!)
                   .CountAsync());

            return (await _context.ItemzHierarchy!
                   .AsNoTracking()
                   .Where(ih => ih.ItemzHierarchyId!.IsDescendantOf(rootProject!.ItemzHierarchyId))
                   .CountAsync())
                   - 1 - sumOfItemzTypeInTargetProject  ; // Minus ONE for Project itself and it's sub ItemzType Hierarchy records

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose resources when needed
            }
        }

        public async Task<bool> HasProjectWithNameAsync(string projectName)
        {
            return await _context.Projects.AsNoTracking().AnyAsync(p => p.Name!.ToLower() == projectName.ToLower());
        }

        public async Task<bool> DeleteProjectItemzHierarchyAsync(Guid projectId)
        {
            bool returnValue;
            var OUTPUT_Success = new SqlParameter
            {
                ParameterName = "OUTPUT_Success",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Bit,
            };

            var sqlParameters = new[]
            {
                new SqlParameter
                {
                    ParameterName = "ProjectId",
                    Value = projectId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                }
            };

            sqlParameters = sqlParameters.Append(OUTPUT_Success).ToArray();

            var _ = await _context.Database.ExecuteSqlRawAsync(sql: "EXEC userProcDeleteItemzHierarchyRecordsByProjectId  @ProjectId, @OUTPUT_Success  = @OUTPUT_Success OUT", parameters: sqlParameters);
            returnValue = (bool)OUTPUT_Success.Value;
            return returnValue;
        }

        public async Task<string?> GetLastProjectHierarchyID()
        {
            var rootItemz =  _context.ItemzHierarchy!.AsNoTracking()
                            .Where(ih => ih.ItemzHierarchyId == HierarchyId.Parse("/"));

            if (rootItemz.Count() != 1)
            {
                throw new ApplicationException("Either no Root Repository Hierarchy record " +
                    "found OR multiple Root Repository Hierarchy records found in the system. " +
                    "Please contact your System Administrator.");
            }

            // EXPLANATION : We are using SQL Server HierarchyID field type. Now we can use EF Core special
            // methods to query for all Decendents as per below. We are actually finding all Decendents by saying
            // First find the ItemzHierarchy record where ID matches RootItemz ID. This is expected to be the
            // repository ID itself which is the root. then we find all desendents of Repository which is nothing but Project(s). 

            var projectHierarchyItemz = await _context.ItemzHierarchy!
                    .AsNoTracking()
                    .Where(ih => ih.ItemzHierarchyId!.GetAncestor(1) == rootItemz.FirstOrDefault()!.ItemzHierarchyId!)
                    .OrderByDescending(ih => ih.ItemzHierarchyId!)
                    .ToListAsync();

            // return projectHierarchyItemz.Count > 0 ? projectHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId.ToString() : null;
            return projectHierarchyItemz.Count > 0 ? projectHierarchyItemz.FirstOrDefault()!.ItemzHierarchyId!.ToString() : null;
        }
    }
}
