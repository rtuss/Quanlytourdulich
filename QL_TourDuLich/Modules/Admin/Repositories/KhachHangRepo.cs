using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class KhachHangRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT 
    KhachHangID,
    HoTen,
    DienThoai,
    Email,
    DiaChi,
    LoaiKhach
FROM KHACHHANG
ORDER BY KhachHangID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetLoaiKhach()
        {
            // Nếu bạn có bảng LOAI_KHACH riêng thì sửa query theo bảng đó.
            // Hiện tại dùng danh sách cố định cho đúng dữ liệu bạn đang có.
            var dt = new DataTable();
            dt.Columns.Add("Value", typeof(string));
            dt.Columns.Add("Text", typeof(string));

            dt.Rows.Add("THUONG", "THUONG");
            dt.Rows.Add("VIP", "VIP");
            dt.Rows.Add("HAY_HUY", "HAY_HUY");
            return dt;
        }

        public static int Create(string hoTen, string dienThoai, string email, string diaChi, string loaiKhach)
        {
            var sql = @"
INSERT INTO KHACHHANG (HoTen, DienThoai, Email, DiaChi, LoaiKhach)
VALUES (@HoTen, @DienThoai, @Email, @DiaChi, @LoaiKhach);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@HoTen", string.IsNullOrWhiteSpace(hoTen) ? (object)DBNull.Value : hoTen),
                new SqlParameter("@DienThoai", string.IsNullOrWhiteSpace(dienThoai) ? (object)DBNull.Value : dienThoai),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email),
                new SqlParameter("@DiaChi", string.IsNullOrWhiteSpace(diaChi) ? (object)DBNull.Value : diaChi),
                new SqlParameter("@LoaiKhach", string.IsNullOrWhiteSpace(loaiKhach) ? (object)DBNull.Value : loaiKhach)
            );
        }

        public static int Update(int khachHangId, string hoTen, string dienThoai, string email, string diaChi, string loaiKhach)
        {
            var sql = @"
UPDATE KHACHHANG
SET
    HoTen = @HoTen,
    DienThoai = @DienThoai,
    Email = @Email,
    DiaChi = @DiaChi,
    LoaiKhach = @LoaiKhach
WHERE KhachHangID = @KhachHangID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@HoTen", string.IsNullOrWhiteSpace(hoTen) ? (object)DBNull.Value : hoTen),
                new SqlParameter("@DienThoai", string.IsNullOrWhiteSpace(dienThoai) ? (object)DBNull.Value : dienThoai),
                new SqlParameter("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email),
                new SqlParameter("@DiaChi", string.IsNullOrWhiteSpace(diaChi) ? (object)DBNull.Value : diaChi),
                new SqlParameter("@LoaiKhach", string.IsNullOrWhiteSpace(loaiKhach) ? (object)DBNull.Value : loaiKhach),
                new SqlParameter("@KhachHangID", khachHangId)
            );
        }

        public static bool HasDangKy(int khachHangId)
        {
            // Dựa theo hệ thống tour của bạn: DANGKY_TOUR
            var sql = @"SELECT COUNT(1) FROM DANGKY_TOUR WHERE KhachHangID = @KhachHangID;";
            var obj = Db.ExecuteScalar(sql, new SqlParameter("@KhachHangID", khachHangId));
            var n = 0;
            if (obj != null && obj != DBNull.Value) int.TryParse(obj.ToString(), out n);
            return n > 0;
        }

        public static int Delete(int khachHangId)
        {
            var sql = @"DELETE FROM KHACHHANG WHERE KhachHangID = @KhachHangID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@KhachHangID", khachHangId));
        }
    }
}
