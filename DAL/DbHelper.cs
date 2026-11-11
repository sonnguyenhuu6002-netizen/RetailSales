using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace RetailSales.DAL
{
    /// <summary>Tiện ích truy cập CSDL. MỌI câu lệnh đều dùng SqlParameter (chống SQL Injection).</summary>
    public static class DbHelper
    {
        // Đổi Server cho phù hợp máy bạn: "." , "localhost", ".\\SQLEXPRESS", "(localdb)\\MSSQLLocalDB"...
        // Microsoft.Data.SqlClient mặc định Encrypt=True nên cần TrustServerCertificate=True khi dùng SQL Server cục bộ.
        public static readonly string ConnStr =
    @"Server=DESKTOP-5NDKGQD\SQLEXPRESS;Database=SalesDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public static SqlConnection Open()
        {
            var cn = new SqlConnection(ConnStr);
            cn.Open();
            return cn;
        }

        public static SqlParameter P(string name, object value) =>
            new SqlParameter(name, value ?? DBNull.Value);

        public static string Str(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);

        /// <summary>SELECT -> List dùng SqlDataReader.</summary>
        public static List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps)
        {
            var list = new List<T>();
            using (var cn = Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(ps);
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) list.Add(map(rd));
            }
            return list;
        }

        public static int Exec(string sql, params SqlParameter[] ps)
        {
            using (var cn = Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(ps);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (var cn = Open())
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(ps);
                return cmd.ExecuteScalar();
            }
        }
    }
}
