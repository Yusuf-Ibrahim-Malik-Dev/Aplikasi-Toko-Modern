using System;
using System.Collections.Generic;
using CommonClass;

namespace Helper
{
    /// <summary>
    /// Session Manager untuk mengelola sesi user yang sedang login
    /// dan memeriksa hak akses berbasis role serta permission
    /// </summary>
    public static class SessionManager
    {
        private static ClassUser _currentUser;
        private static readonly HashSet<string> _permissionCache = new HashSet<string>();

        /// <summary>
        /// Mendapatkan user yang sedang login
        /// </summary>
        public static ClassUser CurrentUser
        {
            get { return _currentUser; }
            set 
            { 
                _currentUser = value; 
                if (value != null)
                {
                    RefreshPermissionCache();
                }
                else
                {
                    _permissionCache.Clear();
                }
            }
        }

        /// <summary>
        /// Cek apakah ada user yang login
        /// </summary>
        public static bool IsLoggedIn => _currentUser != null;

        /// <summary>
        /// Logout user dan bersihkan sesi
        /// </summary>
        public static void Logout()
        {
            _currentUser = null;
            _permissionCache.Clear();
        }

        /// <summary>
        /// Refresh cache permission - permission list must be set externally
        /// </summary>
        private static void RefreshPermissionCache()
        {
            _permissionCache.Clear();
            // Permission cache is populated by SetPermissions external call
        }

        /// <summary>
        /// Set permission codes (called by permission manager after checking DB)
        /// </summary>
        public static void SetPermissions(IEnumerable<string> permissions)
        {
            _permissionCache.Clear();
            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    _permissionCache.Add(p);
                }
            }
        }

        // =========================================================
        // ROLE CHECK METHODS
        // =========================================================

        /// <summary>
        /// Cek apakah user memiliki role tertentu
        /// </summary>
        public static bool HasRole(string roleName)
        {
            if (!IsLoggedIn) return false;
            return !string.IsNullOrWhiteSpace(roleName) &&
                   !string.IsNullOrWhiteSpace(_currentUser.role_name) &&
                   _currentUser.role_name.Equals(roleName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Cek apakah user adalah Owner
        /// </summary>
        public static bool IsOwner() => HasRole("Owner");

        /// <summary>
        /// Cek apakah user adalah Admin
        /// </summary>
        public static bool IsAdmin() => HasRole("Admin") || IsOwner();

        /// <summary>
        /// Cek apakah user adalah Manager
        /// </summary>
        public static bool IsManager() => HasRole("Manager") || IsAdmin();

        /// <summary>
        /// Cek apakah user adalah Kasir
        /// </summary>
        public static bool IsKasir() => HasRole("Kasir") || IsManager();

        /// <summary>
        /// Cek apakah user adalah Gudang
        /// </summary>
        public static bool IsGudang() => HasRole("Gudang") || IsManager();

        // =========================================================
        // PERMISSION CHECK METHODS
        // =========================================================

        /// <summary>
        /// Cek apakah user memiliki permission tertentu
        /// </summary>
        public static bool HasPermission(string permissionCode)
        {
            if (!IsLoggedIn) return false;
            if (string.IsNullOrWhiteSpace(permissionCode)) return false;
            return _permissionCache.Contains(permissionCode);
        }

        /// <summary>
        /// Cek apakah user memiliki salah satu permission dari daftar
        /// </summary>
        public static bool HasAnyPermission(params string[] permissionCodes)
        {
            if (!IsLoggedIn) return false;
            foreach (var code in permissionCodes)
            {
                if (HasPermission(code)) return true;
            }
            return false;
        }

        /// <summary>
        /// Cek apakah user memiliki SEMUA permission dari daftar
        /// </summary>
        public static bool HasAllPermissions(string[] permissionCodes)
        {
            if (!IsLoggedIn) return false;
            foreach (var code in permissionCodes)
            {
                if (!HasPermission(code)) return false;
            }
            return true;
        }

        // =========================================================
        // MENU VISIBLE CHECK METHODS
        // =========================================================

        /// <summary>
        /// Cek apakah boleh menampilkan menu tertentu
        /// </summary>
        public static bool CanView(string permissionCode) => HasPermission(permissionCode);

        /// <summary>
        /// Cek apakah boleh membuat data
        /// </summary>
        public static bool CanCreate(string category) => HasPermission($"{category}_CREATE");

        /// <summary>
        /// Cek apakah boleh edit data
        /// </summary>
        public static bool CanEdit(string category) => HasPermission($"{category}_EDIT");

        /// <summary>
        /// Cek apakah boleh hapus data
        /// </summary>
        public static bool CanDelete(string category) => HasPermission($"{category}_DELETE");

        /// <summary>
        /// Cek apakah boleh lihat data
        /// </summary>
        public static bool CanViewData(string category, string action) => HasPermission($"{category}_{action.ToUpper()}");

        // =========================================================
        // LEGACY METHODS (DIPERBAIKI)
        // =========================================================

        /// <summary>
        /// Cek role_id user saat ini
        /// </summary>
        public static int CurrentRoleId => IsLoggedIn ? _currentUser.role_id : 0;

        /// <summary>
        /// Cek nama role user saat ini
        /// </summary>
        public static string CurrentRoleName => IsLoggedIn ? _currentUser.role_name : string.Empty;

        /// <summary>
        /// Get permission list (cached)
        /// </summary>
        public static IEnumerable<string> GetPermissions()
        {
            if (!IsLoggedIn) return new List<string>();
            return new List<string>(_permissionCache);
        }

        /// <summary>
        /// Cek apakah ada permission tertentu dalam cache
        /// </summary>
        public static bool HasPermissionCached(string permissionCode) =>
            IsLoggedIn && !string.IsNullOrWhiteSpace(permissionCode) && _permissionCache.Contains(permissionCode);
    }
}