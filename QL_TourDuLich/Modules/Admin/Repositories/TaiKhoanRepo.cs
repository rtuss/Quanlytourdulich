using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class TaiKhoanRepo
    {
        public static DataTable GetAll()
        {
            return Db.Query("SELECT TaiKhoanID, TenDangNhap, HoTen, VaiTro, TrangThai, DienThoai, Email, NgayTao FROM TAIKHOAN ORDER BY TaiKhoanID DESC");
        }

        public static int Create(string username, string passHash, string hoTen, string vaiTro, string trangThai, string phone, string email)
        {
            return Db.ExecuteNonQuery(
                "INSERT INTO TAIKHOAN(TenDangNhap, MatKhauHash, HoTen, VaiTro, TrangThai, DienThoai, Email, NgayTao) " +
                "VALUES(@u,@p,@h,@r,@s,@ph,@e,GETDATE())",
                new SqlParameter("@u", username),
                new SqlParameter("@p", passHash),
                new SqlParameter("@h", hoTen),
                new SqlParameter("@r", vaiTro),
                new SqlParameter("@s", trangThai),
                new SqlParameter("@ph", phone),
                new SqlParameter("@e", email)
            );
        }

        public static int Update(int id, string hoTen, string vaiTro, string trangThai, string phone, string email)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TAIKHOAN SET HoTen=@h, VaiTro=@r, TrangThai=@s, DienThoai=@ph, Email=@e WHERE TaiKhoanID=@id",
                new SqlParameter("@h", hoTen),
                new SqlParameter("@r", vaiTro),
                new SqlParameter("@s", trangThai),
                new SqlParameter("@ph", phone),
                new SqlParameter("@e", email),
                new SqlParameter("@id", id)
            );
        }

        public static int ResetPassword(int id, string passHash)
        {
            return Db.ExecuteNonQuery(
                "UPDATE TAIKHOAN SET MatKhauHash=@p WHERE TaiKhoanID=@id",
                new SqlParameter("@p", passHash),
                new SqlParameter("@id", id)
            );
        }
    }
}
