using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class DangKyRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT
    dk.DangKyID,
    dk.TourID,
    t.TenTour,
    dk.KhachHangID,
    kh.HoTen,
    dk.SoLuongNguoi,
    dk.NgayDangKy,
    dk.TrangThai,
    dk.NhanVienID
FROM DANGKY_TOUR dk
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN KHACHHANG kh ON kh.KhachHangID = dk.KhachHangID
ORDER BY dk.DangKyID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetTourLookup()
        {
            var sql = @"
SELECT TourID, TenTour
FROM TOUR
ORDER BY TourID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetKhachHangLookup()
        {
            var sql = @"
SELECT KhachHangID, HoTen
FROM KHACHHANG
ORDER BY KhachHangID DESC;";
            return Db.Query(sql);
        }

        public static int Create(int tourId, int khachHangId, int soLuongNguoi, DateTime ngayDangKy, string trangThai, int? nhanVienId)
        {
            var sql = @"
INSERT INTO DANGKY_TOUR (TourID, KhachHangID, SoLuongNguoi, NgayDangKy, TrangThai, NhanVienID)
VALUES (@TourID, @KhachHangID, @SoLuongNguoi, @NgayDangKy, @TrangThai, @NhanVienID);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TourID", tourId),
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@SoLuongNguoi", soLuongNguoi),
                new SqlParameter("@NgayDangKy", ngayDangKy),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "DANG_KY")),
                new SqlParameter("@NhanVienID", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value)
            );
        }

        public static int Update(int dangKyId, int tourId, int khachHangId, int soLuongNguoi, DateTime ngayDangKy, string trangThai, int? nhanVienId)
        {
            var sql = @"
UPDATE DANGKY_TOUR
SET
    TourID = @TourID,
    KhachHangID = @KhachHangID,
    SoLuongNguoi = @SoLuongNguoi,
    NgayDangKy = @NgayDangKy,
    TrangThai = @TrangThai,
    NhanVienID = @NhanVienID
WHERE DangKyID = @DangKyID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TourID", tourId),
                new SqlParameter("@KhachHangID", khachHangId),
                new SqlParameter("@SoLuongNguoi", soLuongNguoi),
                new SqlParameter("@NgayDangKy", ngayDangKy),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "DANG_KY")),
                new SqlParameter("@NhanVienID", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value),
                new SqlParameter("@DangKyID", dangKyId)
            );
        }

        public static int Delete(int dangKyId)
        {
            var sql = @"DELETE FROM DANGKY_TOUR WHERE DangKyID = @DangKyID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@DangKyID", dangKyId));
        }

        public static int GetSoLuongToiDaCuaTour(int tourId)
        {
            var sql = @"
SELECT ISNULL(SoChoToiDa, 0)
FROM TOUR
WHERE TourID = @TourID;";

            object result = Db.ExecuteScalar(sql, new SqlParameter("@TourID", tourId));
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public static int GetTongSoNguoiDaDangKy(int tourId, int? excludeDangKyId = null)
        {
            string sql = @"
SELECT ISNULL(SUM(ISNULL(SoLuongNguoi, 0)), 0)
FROM DANGKY_TOUR
WHERE TourID = @TourID
  AND ISNULL(TrangThai, N'') <> N'HUY'";

            if (excludeDangKyId.HasValue)
            {
                sql += " AND DangKyID <> @DangKyID";
            }

            if (excludeDangKyId.HasValue)
            {
                object result = Db.ExecuteScalar(sql,
                    new SqlParameter("@TourID", tourId),
                    new SqlParameter("@DangKyID", excludeDangKyId.Value));

                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            else
            {
                object result = Db.ExecuteScalar(sql,
                    new SqlParameter("@TourID", tourId));

                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public static DataTable GetInvoicePreview(int dangKyId)
        {
            var sql = @"
SELECT
    dk.DangKyID,
    t.TenTour,
    kh.HoTen,
    dk.SoLuongNguoi,
    ISNULL(t.Gia, 0) AS DonGia,
    CAST(dk.SoLuongNguoi * ISNULL(t.Gia, 0) AS DECIMAL(18,2)) AS ThanhTien,
    dk.NgayDangKy,
    ISNULL(tt.TrangThai, dk.TrangThai) AS TrangThai,
    ISNULL(tt.SoTien, 0) AS SoTienDaTra,
    tt.NgayThanhToan
FROM DANGKY_TOUR dk
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN KHACHHANG kh ON kh.KhachHangID = dk.KhachHangID
OUTER APPLY (
    SELECT TOP 1
        x.ThanhToanID,
        x.SoTien,
        x.NgayThanhToan,
        x.TrangThai
    FROM THANHTOAN x
    WHERE x.DangKyID = dk.DangKyID
    ORDER BY x.NgayThanhToan DESC, x.ThanhToanID DESC
) tt
WHERE dk.DangKyID = @DangKyID;";

            return Db.Query(sql, new SqlParameter("@DangKyID", dangKyId));
        }
    }
}