
using System;
using System.Data;
using System.Data.SqlClient;

namespace DAAccess
{
    public class DbConnection : IDisposable
    {
        private readonly SqlConnection _con;
        private SqlDataReader _dr;
        private readonly int _defaultTimeout;
        private bool _disposed;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public DbConnection(
            string connString,
            int defaultTimeout = 30)
        {
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new ArgumentException(
                    "Connection string tidak boleh kosong.",
                    nameof(connString));
            }

            _defaultTimeout = defaultTimeout;
            _con = new SqlConnection(connString);
            _con.Open();
        }

        // =========================================================
        // EXEC READER
        // CommandType: StoredProcedure
        //
        // Contoh:
        // conn.ExecReader("proc_login, @2",
        //     new object[] { u, p }, 30);
        // =========================================================
        public void ExecReader(
           string sql,
           object[] par = null)
        {
            CloseReader();

            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                // Jalankan sebagai perintah SQL biasa.
                cmd.CommandType = CommandType.Text;

                cmd.CommandTimeout = 30;

                AddParams(cmd, par);

                _dr = cmd.ExecuteReader();
            }
        }


        // =========================================================
        // EXEC TABLE
        // CommandType: Text
        //
        // Untuk SELECT dari TABLE atau query SQL biasa.
        //
        // Contoh:
        // conn.ExecTable(
        //     "SELECT * FROM APP_BPJS WHERE AP_REGNO = @0",
        //     new object[] { apRegno }, 30);
        // =========================================================
        public DataTable ExecTable(
            string sql,
            object[] par = null,
            int? timeout = null)
        {
            CloseReader();

            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        // =========================================================
        // EXEC VIEW
        // Mengambil seluruh data dari VIEW.
        //
        // Contoh:
        // conn.ExecView("VW_HISTORY_STATUS_BPJSTK", 30);
        // =========================================================
        public DataTable ExecView(
            string viewName,
            int? timeout = null)
        {
            string sql =
                "SELECT * FROM " + QuoteObjectName(viewName);

            return ExecTable(sql, null, timeout);
        }

        // =========================================================
        // EXEC VIEW WITH WHERE
        //
        // Contoh:
        // conn.ExecView(
        //     "VW_HISTORY_STATUS_BPJSTK",
        //     "AP_REGNO = @0",
        //     new object[] { apRegno }, 30);
        // =========================================================
        public DataTable ExecView(
            string viewName,
            string whereClause,
            object[] par,
            int? timeout = null)
        {
            string sql =
                "SELECT * FROM " + QuoteObjectName(viewName);

            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                sql += " WHERE " + whereClause;
            }

            return ExecTable(sql, par, timeout);
        }

        // =========================================================
        // EXEC STORED PROCEDURE
        // Return: DataTable
        //
        // Contoh:
        // conn.ExecStoreProcedure(
        //     "USP_GET_API_BPJSTK",
        //     new object[] { "", "submit-realisasi", "" },
        //     30);
        // =========================================================
        public DataTable ExecStoreProcedure(
            string procedureName,
            object[] par = null,
            int? timeout = null)
        {
            CloseReader();

            using (SqlCommand cmd =
                new SqlCommand(procedureName, _con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    return dt;
                }
            }
        }

        // =========================================================
        // EXEC SCALAR
        // CommandType: StoredProcedure
        // Return: satu nilai
        //
        // Contoh:
        // object result = conn.ExecScalar(
        //     "USP_COUNT_BANKGARANSI", null, 30);
        // =========================================================
        public object ExecScalar(
            string sql,
            object[] par = null,
            int? timeout = null)
        {
            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                return cmd.ExecuteScalar();
            }
        }

        // =========================================================
        // EXEC NON QUERY
        // CommandType: StoredProcedure
        // Return: jumlah baris yang terpengaruh
        //
        // Contoh:
        // int affected = conn.ExecNonQuery(
        //     "USP_UPDATE_STATUS", new object[] { id }, 30);
        // =========================================================
        public int ExecNonQuery(
            string sql,
            object[] par = null,
            int? timeout = null)
        {
            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================================================
        // EXEC SQL NON QUERY
        // CommandType: Text
        // Untuk INSERT, UPDATE, DELETE menggunakan SQL.
        //
        // Contoh:
        // conn.ExecSqlNonQuery(
        //     "UPDATE APP_BPJS SET STATUS = @0 WHERE ID = @1",
        //     new object[] { status, id }, 30);
        // =========================================================
        public int ExecSqlNonQuery(
            string sql,
            object[] par = null,
            int? timeout = null)
        {
            CloseReader();

            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                return cmd.ExecuteNonQuery();
            }
        }

        // =========================================================
        // EXEC SQL SCALAR
        // CommandType: Text
        //
        // Contoh:
        // object total = conn.ExecSqlScalar(
        //     "SELECT COUNT(*) FROM APP_BPJS WHERE STATUS = @0",
        //     new object[] { status }, 30);
        // =========================================================
        public object ExecSqlScalar(
            string sql,
            object[] par = null,
            int? timeout = null)
        {
            CloseReader();

            using (SqlCommand cmd = new SqlCommand(sql, _con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout ?? _defaultTimeout;

                AddParams(cmd, par);

                return cmd.ExecuteScalar();
            }
        }

        // =========================================================
        // READER HELPERS
        // =========================================================

        public bool HasRow()
        {
            return _dr != null &&
                   !_dr.IsClosed &&
                   _dr.Read();
        }

        public string GetFieldValue(string field)
        {
            if (_dr == null || _dr.IsClosed)
                return string.Empty;

            int index = _dr.GetOrdinal(field);

            return _dr.IsDBNull(index)
                ? string.Empty
                : Convert.ToString(_dr.GetValue(index));
        }

        public object GetNativeFieldValue(string field)
        {
            if (_dr == null || _dr.IsClosed)
                return null;

            int index = _dr.GetOrdinal(field);

            return _dr.IsDBNull(index)
                ? null
                : _dr.GetValue(index);
        }

        public T GetFieldValue<T>(
            string field,
            T defaultValue = default(T))
        {
            object value = GetNativeFieldValue(field);

            if (value == null || value == DBNull.Value)
                return defaultValue;

            Type targetType =
                Nullable.GetUnderlyingType(typeof(T))
                ?? typeof(T);

            if (targetType.IsEnum)
            {
                return (T)Enum.ToObject(targetType, value);
            }

            return (T)Convert.ChangeType(value, targetType);
        }

        // =========================================================
        // PRIVATE: ADD PARAMETERS
        // Parameter: @0, @1, @2, ...
        // =========================================================
        private void AddParams(
            SqlCommand cmd,
            object[] par)
        {
            if (par == null)
                return;

            for (int i = 0; i < par.Length; i++)
            {
                cmd.Parameters.AddWithValue(
                    "@" + i,
                    par[i] ?? DBNull.Value);
            }
        }

        // =========================================================
        // PRIVATE: VALIDATE TABLE / VIEW NAME
        // Mendukung nama seperti:
        // VW_HISTORY_STATUS_BPJSTK
        // dbo.VW_HISTORY_STATUS_BPJSTK
        // [dbo].[VW_HISTORY_STATUS_BPJSTK] tidak diperlukan
        // =========================================================
        private static string QuoteObjectName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Nama TABLE atau VIEW wajib diisi.",
                    nameof(name));
            }

            string[] parts = name.Split('.');

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    throw new ArgumentException(
                        "Nama TABLE atau VIEW tidak valid.",
                        nameof(name));
                }

                foreach (char c in part)
                {
                    if (!char.IsLetterOrDigit(c) &&
                        c != '_' &&
                        c != '#' &&
                        c != '$')
                    {
                        throw new ArgumentException(
                            "Nama TABLE atau VIEW tidak valid.",
                            nameof(name));
                    }
                }
            }

            return string.Join(
                ".",
                Array.ConvertAll(
                    parts,
                    delegate (string part)
                    {
                        return "[" + part + "]";
                    }));
        }

        public DataTable ExecQuery(
       string query,
       object[] parameters = null)
        {
            // Menyiapkan tempat untuk menampung hasil query.
            DataTable dt = new DataTable();

            // Menutup reader sebelumnya jika masih terbuka.
            CloseReader();

            // Menggunakan koneksi database yang sudah tersedia.
            using (SqlCommand cmd = new SqlCommand(query, _con))
            {
                // Menjalankan query SQL biasa.
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = _defaultTimeout;

                // Menambahkan parameter query.
                AddParams(cmd, parameters);

                // Mengambil hasil query ke DataTable.
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }

            // Mengembalikan seluruh data.
            return dt;
        }

        // =========================================================
        // PRIVATE: CLOSE READER
        // =========================================================
        private void CloseReader()
        {
            if (_dr != null)
            {
                if (!_dr.IsClosed)
                {
                    _dr.Close();
                }

                _dr.Dispose();
                _dr = null;
            }
        }

        // =========================================================
        // DISPOSE
        // =========================================================
        public void Dispose()
        {
            if (_disposed)
                return;

            CloseReader();

            if (_con != null)
            {
                if (_con.State != ConnectionState.Closed)
                {
                    _con.Close();
                }

                _con.Dispose();
            }

            _disposed = true;
        }
    }
}
