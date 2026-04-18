using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class KhuyenMaiRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT 
    KhuyenMaiID,
    TenKhuyenMai,
    PhanTramGiam,
    TuNgay,
    DenNgay,
    TrangThai
FROM KHUYENMAI
ORDER BY KhuyenMaiID DESC;";
            return Db.Query(sql);
        }

        public static int Create(string ten, int phanTram, DateTime tuNgay, DateTime denNgay, string trangThai)
        {
            var sql = @"
INSERT INTO KHUYENMAI (TenKhuyenMai, PhanTramGiam, TuNgay, DenNgay, TrangThai)
VALUES (@Ten, @PhanTram, @TuNgay, @DenNgay, @TrangThai);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@Ten", string.IsNullOrWhiteSpace(ten) ? (object)DBNull.Value : ten.Trim()),
                new SqlParameter("@PhanTram", phanTram),
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date),
                new SqlParameter("@TrangThai", string.IsNullOrWhiteSpace(trangThai) ? (object)DBNull.Value : trangThai.Trim())
            );
        }

        public static int Update(int id, string ten, int phanTram, DateTime tuNgay, DateTime denNgay, string trangThai)
        {
            var sql = @"
UPDATE KHUYENMAI
SET
    TenKhuyenMai = @Ten,
    PhanTramGiam = @PhanTram,
    TuNgay = @TuNgay,
    DenNgay = @DenNgay,
    TrangThai = @TrangThai
WHERE KhuyenMaiID = @ID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@Ten", string.IsNullOrWhiteSpace(ten) ? (object)DBNull.Value : ten.Trim()),
                new SqlParameter("@PhanTram", phanTram),
                new SqlParameter("@TuNgay", tuNgay.Date),
                new SqlParameter("@DenNgay", denNgay.Date),
                new SqlParameter("@TrangThai", string.IsNullOrWhiteSpace(trangThai) ? (object)DBNull.Value : trangThai.Trim()),
                new SqlParameter("@ID", id)
            );
        }

        public static int Delete(int id)
        {
            var sql = @"DELETE FROM KHUYENMAI WHERE KhuyenMaiID = @ID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@ID", id));
        }

        // Nếu bạn muốn ràng buộc: khuyến mãi đang dùng ở bảng khác thì không xóa
        // (ví dụ TOUR có KhuyenMaiID) => bật hàm này lên và check trong Service.
        public static bool IsUsed(int id)
        {
            // Sửa tên bảng/cột đúng hệ bạn nếu có:
            var sql = @"SELECT COUNT(1) FROM TOUR WHERE KhuyenMaiID = @ID;";
            var obj = Db.ExecuteScalar(sql, new SqlParameter("@ID", id));

            int n = 0;
            if (obj != null && obj != DBNull.Value) int.TryParse(obj.ToString(), out n);
            return n > 0;
        }
    }
}
