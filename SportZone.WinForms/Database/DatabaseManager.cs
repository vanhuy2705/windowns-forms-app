using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SportZone.WinForms.Database
{
    public class DbConnectionResult
    {
        public bool Success { get; set; }
        public string DatabaseName { get; set; } = "QuanLySanTheThaoDB";
        public string ServerName { get; set; } = @".\SQLEXPRESS";
        public long LatencyMs { get; set; }
        public int TableCount { get; set; }
        public string DataFilePath { get; set; } = @"C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\QuanLySanTheThaoDB.mdf";
        public string ErrorMessage { get; set; }
    }

    public static class DatabaseManager
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    try
                    {
                        var csConfig = ConfigurationManager.ConnectionStrings["SportZoneDb"]?.ConnectionString;
                        if (!string.IsNullOrEmpty(csConfig))
                        {
                            _connectionString = csConfig;
                        }
                    }
                    catch
                    {
                        // Fallback to default local SQLEXPRESS
                    }

                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = @"Data Source=DESKTOP-B9A50IM\SQLEXPRESS;Initial Catalog=QuanLySanTheThaoDB;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5;";
                    }
                }
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DbConnectionResult TestConnection()
        {
            var result = new DbConnectionResult();
            var sw = Stopwatch.StartNew();

            try
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    sw.Stop();
                    result.LatencyMs = sw.ElapsedMilliseconds;
                    result.Success = true;

                    const string sql = @"
                        SELECT 
                            ISNULL(@@SERVERNAME, '.\SQLEXPRESS') AS ServerName,
                            ISNULL(DB_NAME(), 'QuanLySanTheThaoDB') AS DbName,
                            (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE') AS TableCount,
                            ISNULL((SELECT TOP 1 physical_name FROM sys.database_files WHERE type_desc = 'ROWS'), 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\QuanLySanTheThaoDB.mdf') AS DataFilePath;";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result.ServerName = reader["ServerName"]?.ToString() ?? @".\SQLEXPRESS";
                            result.DatabaseName = reader["DbName"]?.ToString() ?? "QuanLySanTheThaoDB";
                            result.TableCount = Convert.ToInt32(reader["TableCount"] ?? 0);
                            result.DataFilePath = reader["DataFilePath"]?.ToString() ?? @"C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\QuanLySanTheThaoDB.mdf";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                result.Success = false;
                result.LatencyMs = sw.ElapsedMilliseconds;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public static Task<DbConnectionResult> TestConnectionAsync()
        {
            return Task.Run(() => TestConnection());
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static DataSet ExecuteDataSet(string sql, params SqlParameter[] parameters)
        {
            var ds = new DataSet();
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }
            return ds;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
