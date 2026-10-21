using System;
using System.Collections.Generic;
using CommonClass;
using Helper;
using System.Data;

namespace DAAccess
{
    public class DAUser : IDisposable
    {
        private readonly DbConnection conn;
        private bool disposed = false;

        public DAUser()
        {
            conn = new DbConnection(dbConfiguration.ConnectionString);
        }

        // =========================================================
        // LOGIN USER
        // Returns: "1" = success, "0" = failed, "-1" = inactive
        // =========================================================
        public string CheckLogin(ClassUser data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.nama) ||
                string.IsNullOrEmpty(data.Password))
            {
                return "0";
            }

            string password = null;
            bool isActive = false;
            bool isUserFound = false;
            ClassUser loggedInUser = null;

            object[] par =
            {
                data.nama.Trim()
            };

            conn.ExecReader("EXEC dbo.proc_login @0", par);

            while (conn.HasRow())
            {
                isUserFound = true;
                password = conn.GetFieldValue("password");
                isActive = conn.GetFieldValue<bool>("is_active");

                loggedInUser = new ClassUser
                {
                    nik = conn.GetFieldValue("nik"),
                    nama = conn.GetFieldValue("nama"),
                    Password = conn.GetFieldValue("password"),
                    role_name = conn.GetFieldValue("role_name"),
                    role_id = conn.GetFieldValue<int>("role_id"),
                    is_active = isActive
                };
            }

            if (!isUserFound) return "0";
            if (!isActive) return "-1";
            if (string.IsNullOrWhiteSpace(password)) return "0";

            bool passwordBenar = PasswordHelper.VerifyPassword(data.Password, password);

            if (passwordBenar && loggedInUser != null)
            {
                SessionManager.CurrentUser = loggedInUser;
                using (var permissionAccess = new DAPermission())
                {
                    SessionManager.SetPermissions(permissionAccess.GetUserPermissions(loggedInUser.role_id));
                }
                return "1";
            }

            return "0";
        }

        // =========================================================
        // GET CURRENT USER SESSION
        // =========================================================
        public ClassUser GetCurrentUser()
        {
            return SessionManager.CurrentUser;
        }

        // =========================================================
        // CHECK USER HAS PERMISSION
        // =========================================================
        public bool HasPermission(string permissionCode)
        {
            if (!SessionManager.IsLoggedIn) return false;
            return CheckUserPermission(SessionManager.CurrentUser.role_id, permissionCode);
        }

        // =========================================================
        // CHECK USER HAS ROLE
        // =========================================================
        public bool HasRole(int roleId, string roleName)
        {
            if (!SessionManager.IsLoggedIn) return false;
            return SessionManager.CurrentUser.role_id == roleId &&
                   !string.IsNullOrWhiteSpace(SessionManager.CurrentUser.role_name) &&
                   SessionManager.CurrentUser.role_name.Equals(roleName, StringComparison.OrdinalIgnoreCase);
        }

        private bool CheckUserPermission(int roleId, string permissionCode)
        {
            using (var rpConn = new DAPermission())
            {
                return rpConn.CheckPermission(roleId, permissionCode);
            }
        }

        // =========================================================
        // AMBIL SEMUA DATA USER
        // =========================================================
        public List<ClassUser> BindList()
        {
            List<ClassUser> list = new List<ClassUser>();

            conn.ExecReader("proc_getAllUser");

            while (conn.HasRow())
            {
                ClassUser data = new ClassUser();
                data.nik = conn.GetFieldValue("nik");
                data.nama = conn.GetFieldValue("nama");
                data.Password = string.Empty;
                data.expireddate = conn.GetFieldValue("expireddate");
                data.is_active = conn.GetFieldValue<bool>("is_active");
                data.role_id = conn.GetFieldValue<int>("role_id");
                data.role_name = conn.GetFieldValue("role_name");
                list.Add(data);
            }

            return list;
        }

        // =========================================================
        // TAMBAH / UPDATE USER
        // =========================================================
        public string SubmitUser(ClassUser data, string action = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (string.IsNullOrWhiteSpace(data.nik))
                throw new ArgumentException("NIK wajib diisi.", nameof(data));

            string msg = string.Empty;
            string passwordHash = null;

            // Cek apakah NIK sudah ada di database.
            ClassUser existingUser = GetDataByUserId(data.nik);

            // Tentukan action otomatis berdasarkan keberadaan user.
            action = (existingUser != null &&
                      !string.IsNullOrEmpty(existingUser.nik))
                ? "UPDATE"
                : "INSERT";

            // Hash password hanya jika password baru diberikan.
            if (!string.IsNullOrWhiteSpace(data.Password))
            {
                passwordHash = data.Password;
            }
            else if (existingUser != null)
            {
                // Pertahankan password lama jika tidak diganti.
                passwordHash = existingUser.Password;
            }

            object[] par =
            {
                    data.nik,
                    data.nama,
                    passwordHash,
                    data.expireddate,
                    data.role_id,
                    data.is_active ? 1 : 0,
                    action
                };

            conn.ExecReader(
                "EXEC dbo.proc_userinsertupdate @0, @1, @2, @3, @4, @5, @6",
                par);

            while (conn.HasRow())
            {
                msg = conn.GetFieldValue("pesan");
            }

            return msg;
        }

        // =========================================================
        // AMBIL DATA USER BERDASARKAN NIK
        // =========================================================
        public ClassUser GetDataByUserId(string nik)
        {
            ClassUser data = new ClassUser();

            if (string.IsNullOrWhiteSpace(nik)) return data;

            object[] par = { nik.Trim() };

            conn.ExecReader("EXEC dbo.proc_getdatauserbykode @0", par);

            while (conn.HasRow())
            {
                data.nik = conn.GetFieldValue("nik");
                data.nama = conn.GetFieldValue("nama");
                data.Password = string.Empty;
                data.expireddate = conn.GetFieldValue("expireddate");
                data.is_active = conn.GetFieldValue<bool>("is_active");
                data.role_id = conn.GetFieldValue<int>("role_id");
                data.role_name = conn.GetFieldValue("role_name");
            }

            return data;
        }

        // =========================================================
        // HAPUS USER
        // =========================================================
        public ClassUser DeleteUser(string nik)
        {
            if (string.IsNullOrWhiteSpace(nik))
                throw new ArgumentException("NIK tidak boleh kosong.", nameof(nik));

            object[] par = { nik.Trim() };

            conn.ExecReader("EXEC dbo.proc_deleteuser @0", par);
            while (conn.HasRow()) { /* consume optional procedure result */ }
            return new ClassUser { nik = nik.Trim() };
        }

        // =========================================================
        // SET USER ACTIVE/INACTIVE
        // =========================================================
        public bool SetUserActive(string nik, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(nik)) return false;

            ClassUser data = new ClassUser { nik = nik, is_active = isActive };
            string result = SubmitUser(data, "UPDATE");
            return result.Contains("berhasil");
        }

        // =========================================================
        // DISPOSE
        // =========================================================
        public void Dispose()
        {
            if (!disposed)
            {
                if (conn != null) conn.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    // =========================================================
    // KELAS PENDUKUNG: DAPermission
    // =========================================================
    public class DAPermission : IDisposable
    {
        private readonly DbConnection conn;
        private bool disposed = false;

        public DAPermission()
        {
            conn = new DbConnection(dbConfiguration.ConnectionString);
        }

        public List<string> GetUserPermissions(int roleId)
        {
            List<string> permissions = new List<string>();
            conn.ExecReader(@"
                SELECT p.permission_code 
                FROM tbRolePermission rp
                INNER JOIN tbPermission p ON rp.permission_id = p.permission_id
                WHERE rp.role_id = @0 AND p.is_active = 1", new object[] { roleId });

            while (conn.HasRow())
            {
                permissions.Add(conn.GetFieldValue("permission_code"));
            }
            return permissions;
        }

        public bool CheckPermission(int roleId, string permissionCode)
        {
            conn.ExecReader(@"
                SELECT COUNT(*) AS total
                FROM tbRolePermission rp
                INNER JOIN tbPermission p ON rp.permission_id = p.permission_id
                WHERE rp.role_id = @0 AND p.permission_code = @1 AND p.is_active = 1",
                new object[] { roleId, permissionCode });

            while (conn.HasRow())
                return conn.GetFieldValue<int>("total") > 0;
            return false;
        }

        public List<string> GetAllPermissions()
        {
            List<string> permissions = new List<string>();
            conn.ExecReader("SELECT permission_code FROM tbPermission WHERE is_active = 1");
            while (conn.HasRow())
            {
                permissions.Add(conn.GetFieldValue("permission_code"));
            }
            return permissions;
        }

        public bool AssignPermissionToRole(int roleId, int permissionId)
        {
            try
            {
                conn.ExecNonQuery(
                    "INSERT INTO tbRolePermission (role_id, permission_id, granted_at) VALUES (@0, @1, GETDATE())",
                    new object[] { roleId, permissionId });
                return true;
            }
            catch { return false; }
        }

        public bool RevokePermissionFromRole(int roleId, int permissionId)
        {
            try
            {
                conn.ExecNonQuery(
                    "DELETE FROM tbRolePermission WHERE role_id = @0 AND permission_id = @1",
                    new object[] { roleId, permissionId });
                return true;
            }
            catch { return false; }
        }

        public List<ClassRole> GetRoles()
        {
            List<ClassRole> roles = new List<ClassRole>();
            conn.ExecReader("SELECT role_id, role_name, role_description, is_active FROM tbRole WHERE is_active = 1");
            while (conn.HasRow())
            {
                roles.Add(new ClassRole
                {
                    role_id = conn.GetFieldValue<int>("role_id"),
                    role_name = conn.GetFieldValue("role_name"),
                    role_description = conn.GetFieldValue("role_description"),
                    is_active = conn.GetFieldValue<bool>("is_active")
                });
            }
            return roles;
        }

        public DataTable GetDataRole()
        {
            DataTable dt = new DataTable();

            conn.ExecReader(@"
                        SELECT
                            role_id,
                            role_name
                        FROM dbo.tbRole
                        WHERE is_active = 1
                        ORDER BY role_name
                    ");

            dt.Columns.Add("role_id", typeof(int));
            dt.Columns.Add("role_name", typeof(string));

            while (conn.HasRow())
            {
                DataRow row = dt.NewRow();

                row["role_id"] = conn.GetFieldValue<int>("role_id");
                row["role_name"] = conn.GetFieldValue("role_name");

                dt.Rows.Add(row);
            }

            return dt;
        }

        public string GenerateNik()
        {
            string tanggal = DateTime.Now.ToString("yyyyMMdd");

            conn.ExecReader(@"
                    SELECT
                        @0 + RIGHT(
                            '0000' + CAST(
                                ISNULL(
                                    MAX(TRY_CONVERT(INT, RIGHT(nik, 4))),
                                    0
                                ) + 1 AS VARCHAR(10)
                            ),
                            4
                        ) AS nik
                    FROM dbo.tbUser
                    WHERE nik LIKE @0 + '%'
                ", new object[] { tanggal });

            while (conn.HasRow())
            {
                return conn.GetFieldValue("nik");
            }

            throw new Exception("Gagal generate NIK.");
        }



        public DataTable GetAllUser()
        {
            var par = new object[] { };

            return conn.ExecQuery("EXEC dbo.proc_getAllUser");
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (conn != null) conn.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    // =========================================================
    // CLASS ROLE MODEL
    // =========================================================
    public class ClassRole
    {
        public int role_id { get; set; }
        public string role_name { get; set; }
        public string role_description { get; set; }
        public bool is_active { get; set; }
    }
}