using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class ThanhToanRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT
    tt.ThanhToanID,
    tt.DangKyID,
    dk.TourID,
    t.TenTour,
    dk.KhachHangID,
    kh.HoTen,
    tt.SoTien,
    tt.NgayThanhToan,
    tt.TrangThai,
    tt.GhiChu,
    ISNULL(tt.DaHuy, 0) AS DaHuy
FROM THANHTOAN tt
LEFT JOIN DANGKY_TOUR dk ON dk.DangKyID = tt.DangKyID
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN KHACHHANG kh ON kh.KhachHangID = dk.KhachHangID
WHERE ISNULL(tt.DaHuy, 0) = 0
ORDER BY tt.ThanhToanID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetDangKyLookup()
        {
            var sql = @"
SELECT
    dk.DangKyID,
    (CAST(dk.DangKyID AS NVARCHAR(20)) + N' - ' + ISNULL(t.TenTour,N'') + N' - ' + ISNULL(kh.HoTen,N'')) AS TenHienThi
FROM DANGKY_TOUR dk
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN KHACHHANG kh ON kh.KhachHangID = dk.KhachHangID
WHERE dk.TrangThai = 'DANG_KY'
ORDER BY dk.DangKyID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetCongNoTongHop()
        {
            var sql = @"
SELECT
    dk.DangKyID,
    t.TenTour,
    kh.HoTen,
    ISNULL(dk.SoLuongNguoi, 1) AS SoLuongNguoi,
    ISNULL(t.Gia, 0) AS DonGia,
    ISNULL(dk.SoLuongNguoi, 1) * ISNULL(t.Gia, 0) AS TongTienPhaiTra,
    ISNULL(SUM(CASE WHEN ISNULL(tt.DaHuy, 0) = 0 THEN tt.SoTien ELSE 0 END), 0) AS TongDaThanhToan,
    (ISNULL(dk.SoLuongNguoi, 1) * ISNULL(t.Gia, 0))
      - ISNULL(SUM(CASE WHEN ISNULL(tt.DaHuy, 0) = 0 THEN tt.SoTien ELSE 0 END), 0) AS ConNo,
    CASE
        WHEN ISNULL(SUM(CASE WHEN ISNULL(tt.DaHuy, 0) = 0 THEN tt.SoTien ELSE 0 END), 0) = 0
            THEN N'CHUA_THANH_TOAN'
        WHEN ISNULL(SUM(CASE WHEN ISNULL(tt.DaHuy, 0) = 0 THEN tt.SoTien ELSE 0 END), 0) < (ISNULL(dk.SoLuongNguoi, 1) * ISNULL(t.Gia, 0))
            THEN N'DA_COC'
        ELSE N'DA_THANH_TOAN'
    END AS TrangThaiCongNo
FROM DANGKY_TOUR dk
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN KHACHHANG kh ON kh.KhachHangID = dk.KhachHangID
LEFT JOIN THANHTOAN tt ON tt.DangKyID = dk.DangKyID
WHERE dk.TrangThai = 'DANG_KY'
GROUP BY
    dk.DangKyID,
    t.TenTour,
    kh.HoTen,
    dk.SoLuongNguoi,
    t.Gia
ORDER BY dk.DangKyID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetCongNoByDangKyId(int dangKyId)
        {
            var sql = @"
SELECT
    dk.DangKyID,
    ISNULL(dk.SoLuongNguoi, 1) * ISNULL(t.Gia, 0) AS TongTienPhaiTra,
    ISNULL(SUM(CASE WHEN ISNULL(tt.DaHuy, 0) = 0 THEN tt.SoTien ELSE 0 END), 0) AS TongDaThanhToan
FROM DANGKY_TOUR dk
LEFT JOIN TOUR t ON t.TourID = dk.TourID
LEFT JOIN THANHTOAN tt ON tt.DangKyID = dk.DangKyID
WHERE dk.DangKyID = @DangKyID
GROUP BY dk.DangKyID, dk.SoLuongNguoi, t.Gia;";
            return Db.Query(sql, new SqlParameter("@DangKyID", dangKyId));
        }

        public static DataTable GetById(int thanhToanId)
        {
            var sql = @"
SELECT
    ThanhToanID,
    DangKyID,
    SoTien,
    NgayThanhToan,
    TrangThai,
    GhiChu,
    ISNULL(DaHuy, 0) AS DaHuy
FROM THANHTOAN
WHERE ThanhToanID = @ThanhToanID;";
            return Db.Query(sql, new SqlParameter("@ThanhToanID", thanhToanId));
        }

        public static int Create(int dangKyId, decimal soTien, DateTime ngayThanhToan, string trangThai, string ghiChu)
        {
            var sql = @"
INSERT INTO THANHTOAN (DangKyID, SoTien, NgayThanhToan, TrangThai, GhiChu, DaHuy)
VALUES (@DangKyID, @SoTien, @NgayThanhToan, @TrangThai, @GhiChu, 0);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@DangKyID", dangKyId),
                new SqlParameter("@SoTien", soTien),
                new SqlParameter("@NgayThanhToan", ngayThanhToan),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "CHUA_DU")),
                new SqlParameter("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? (object)DBNull.Value : ghiChu)
            );
        }

        public static int Update(int thanhToanId, int dangKyId, decimal soTien, DateTime ngayThanhToan, string trangThai, string ghiChu)
        {
            var sql = @"
UPDATE THANHTOAN
SET
    DangKyID = @DangKyID,
    SoTien = @SoTien,
    NgayThanhToan = @NgayThanhToan,
    TrangThai = @TrangThai,
    GhiChu = @GhiChu
WHERE ThanhToanID = @ThanhToanID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@DangKyID", dangKyId),
                new SqlParameter("@SoTien", soTien),
                new SqlParameter("@NgayThanhToan", ngayThanhToan),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "CHUA_DU")),
                new SqlParameter("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? (object)DBNull.Value : ghiChu),
                new SqlParameter("@ThanhToanID", thanhToanId)
            );
        }

        public static int UpdateStatus(int thanhToanId, string trangThai)
        {
            var sql = @"
UPDATE THANHTOAN
SET TrangThai = @TrangThai
WHERE ThanhToanID = @ThanhToanID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TrangThai", trangThai),
                new SqlParameter("@ThanhToanID", thanhToanId)
            );
        }

        public static int UpdateStatusByDangKyId(int dangKyId, string trangThai)
        {
            var sql = @"
UPDATE THANHTOAN
SET TrangThai = @TrangThai
WHERE DangKyID = @DangKyID
  AND ISNULL(DaHuy, 0) = 0;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TrangThai", trangThai),
                new SqlParameter("@DangKyID", dangKyId)
            );
        }

        public static int SoftDelete(int thanhToanId)
        {
            var sql = @"UPDATE THANHTOAN SET DaHuy = 1 WHERE ThanhToanID = @ThanhToanID;";
            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@ThanhToanID", thanhToanId));
        }
    }
}