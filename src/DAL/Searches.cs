namespace SQLServerSearcher.DAL
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    using Model;

    public class Searches : ISearches
    {
        private readonly ApplicationState _appState;

        public Searches(ApplicationState appState)
        {
            _appState = appState;
        }

        public List<Database> GetDatabases()
        {
            var databases = new List<Database>();
            var reader = ExecuteSql("SELECT d.name, d.create_date, d.collation_name, d.state_desc FROM sys.databases d");
            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    var database = new Database
                    {
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("create_date")),
                        CollationName = reader.GetString(reader.GetOrdinal("collation_name")),
                        OnlineState = reader.GetString(reader.GetOrdinal("state_desc"))
                    };
                    databases.Add(database);
                }
            }
            return databases;
        }

        public List<Table> FindTables(string database, string query = null)
        {
            var tables = new List<Table>();
            string sql = string.Format(@" SELECT DISTINCT s.name AS schemaName, t.name, t.create_date, t.modify_date, ISNULL(iu.lastSeek, '') AS lastSeek, ISNULL(iu.lastScan, '') AS lastScan, ISNULL(iu.lastLookup, '') AS lastLookup, ISNULL(iu.lastUpdate, '') AS lastUpdate
                                            FROM {0}.sys.tables t
                                           INNER JOIN {0}.sys.schemas s ON s.schema_id = t.schema_id
                                           OUTER APPLY (SELECT MAX(last_user_seek) AS lastSeek, MAX(last_user_scan) AS lastScan, MAX(last_user_lookup) AS lastLookup, MAX(last_user_update) AS lastUpdate FROM {0}.sys.dm_db_index_usage_stats ius WHERE ius.object_id = t.object_id) iu", database);
            if (!string.IsNullOrEmpty(query))
            {
                sql = sql + string.Format(@" LEFT OUTER JOIN {0}.sys.columns c ON t.object_id = c.object_id
                                            WHERE t.name LIKE '%{1}%' OR c.name LIKE '%{1}%'", database, query);
            }
            using (var reader = ExecuteSql(sql))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var table = new Table
                        {
                            SchemaName = reader.GetString(reader.GetOrdinal("schemaName")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("create_date")),
                            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modify_date")),
                            LastSeek = reader.GetDateTime(reader.GetOrdinal("lastSeek")),
                            LastScan = reader.GetDateTime(reader.GetOrdinal("lastScan")),
                            LastLookup = reader.GetDateTime(reader.GetOrdinal("lastLookup")),
                            LastUpdate = reader.GetDateTime(reader.GetOrdinal("lastUpdate")),
                        };
                        tables.Add(table);
                    }
                }
            }
            return tables;
        }

        public List<View> FindViews(string database, string query = null)
        {
            var views = new List<View>();
            string sql = string.Format(@" SELECT DISTINCT s.name AS schemaName, v.name, v.create_date, v.modify_date, ISNULL(iu.lastSeek, '') AS lastSeek, ISNULL(iu.lastScan, '') AS lastScan, ISNULL(iu.lastLookup, '') AS lastLookup, ISNULL(iu.lastUpdate, '') AS lastUpdate
                                            FROM {0}.sys.views v
                                           INNER JOIN {0}.sys.schemas s ON s.schema_id = v.schema_id
                                           OUTER APPLY (SELECT MAX(last_user_seek) AS lastSeek, MAX(last_user_scan) AS lastScan, MAX(last_user_lookup) AS lastLookup, MAX(last_user_update) AS lastUpdate FROM {0}.sys.dm_db_index_usage_stats ius WHERE ius.object_id = v.object_id) iu", database);
            if (!string.IsNullOrEmpty(query))
            {
                sql = sql + string.Format(@" LEFT OUTER JOIN {0}.sys.columns c ON v.object_id = c.object_id
                                            WHERE v.name LIKE '%{1}%' OR c.name LIKE '%{1}%'", database, query);
            }
            using (var reader = ExecuteSql(sql))
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var view = new View
                        {
                            SchemaName = reader.GetString(reader.GetOrdinal("schemaName")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("create_date")),
                            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modify_date")),
                            LastSeek = reader.GetDateTime(reader.GetOrdinal("lastSeek")),
                            LastScan = reader.GetDateTime(reader.GetOrdinal("lastScan")),
                            LastLookup = reader.GetDateTime(reader.GetOrdinal("lastLookup")),
                            LastUpdate = reader.GetDateTime(reader.GetOrdinal("lastUpdate")),
                        };
                        views.Add(view);
                    }
                }
            }
            return views;
        }

        public List<string> FindProcedures(string database, string query = null)
        {
            return new List<string>();
        }

        public List<string> FindFunctions(string database, string query = null)
        {
            return new List<string>();
        }

        public List<string> GetSchemas()
        {
            var schemaCollection = new List<string>();
            DataTable dt = _appState.CurrentConnection.GetSchema();
            foreach (DataRow schema in dt.Rows)
            {
                var schemaName = schema.Field<string>("CollectionName");
                schemaCollection.Add(schemaName);
            }
            return schemaCollection;
        }

        private SqlDataReader ExecuteSql(string sql)
        {
            var cmd = new SqlCommand
            {
                CommandText = sql, 
                CommandType = CommandType.Text, 
                Connection = _appState.CurrentConnection
            };
            return cmd.ExecuteReader();            
        }
    }
}
