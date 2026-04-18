using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public class AuthResult
    {
        public int TaiKhoanID { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string VaiTro { get; set; }
        public string TrangThai { get; set; }
    }

    public static class AuthService
    {
        public static AuthResult Login(string username, string password)
        {
            var dt = Db.Query(
                "SELECT TOP 1 TaiKhoanID, TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai " +
                "FROM TAIKHOAN WHERE TenDangNhap = @u",
                new SqlParameter("@u", username)
            );

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];

            var hashDb = (row["MatKhauHash"]?.ToString() ?? "").Trim().ToLower();
            var hashInput = HashHelper.Sha256Hex(password).Trim().ToLower();

            // So sánh hash mật khẩu theo TÀI KHOẢN trong SQL
            if (hashDb != hashInput)
                return null;

            return new AuthResult
            {
                TaiKhoanID = (int)row["TaiKhoanID"],
                TenDangNhap = row["TenDangNhap"].ToString(),
                HoTen = row["HoTen"].ToString(),
                VaiTro = row["VaiTro"].ToString(),
                TrangThai = row["TrangThai"].ToString()
            };
        }
    }
}
