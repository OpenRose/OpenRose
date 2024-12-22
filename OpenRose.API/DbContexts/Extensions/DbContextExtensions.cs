// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace ItemzApp.API.DbContexts.Extensions
{
    // EXPLANATION : I learned about this extension method as per blog from Dmitry Sikorsky found at ...
    // https://medium.com/@dmitrysikorsky/entity-framework-core-count-by-sql-query-6ac12557dfaa

    public static class DbContextExtensions
    {
        public async static Task<int> CountByRawSqlAsync(this DbContext dbContext, string sql, params KeyValuePair<string, object>[] parameters)
        {
            int result = -1;
            SqlConnection? connection = dbContext.Database.GetDbConnection() as SqlConnection;

            if (connection is null)
            {
                return result;
            }
            try
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    foreach (KeyValuePair<string, object> parameter in parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }

                    using (DbDataReader dataReader = await command.ExecuteReaderAsync())
                        if (dataReader.HasRows)
                            while (dataReader.Read())
                                result = dataReader.GetInt32(0);
                }
            }

            // TODO : We should have better error handling here
            catch (System.Exception e) 
            {
                throw;
            }

            finally { connection.Close(); }

            return result;
        }
    }
}
