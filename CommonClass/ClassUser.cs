using System;

namespace CommonClass
{
    public class ClassUser
    {
        public string nik { get; set; }
        public string nama { get; set; }
        public string Password { get; set; }
        public string expireddate { get; set; }
        public int role_id { get; set; }
        public string role_name { get; set; }
        public bool is_active { get; set; } = true;
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
    }
}
