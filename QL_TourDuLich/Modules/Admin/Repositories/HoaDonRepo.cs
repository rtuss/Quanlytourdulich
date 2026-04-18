using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class HoaDonRepo
    {
        // 1) Tạo hóa đơn + chi tiết, trả về HoaDonID
        public static int CreateFromDangKy(int dangKyId, int? nhanVienId)
        {
            var sql = @"
DECLARE @HoaDonID INT;

INSERT INTO HOADON (DangKyID, TrangThai, NhanVienID, NgayLap, TongTien)
VALUES (@DangKyID, N'DA_XUAT', @NhanVienID, GETDATE(), 0);

SET @HoaDonID = SCOPE_IDENTITY();

INSERT INTO HOADON_CHITIET (HoaDonID, TenTour, SoLuongNguoi, DonGia, ThanhTien)
SELECT
    @HoaDonID,
    t.TenTour,
    dk.SoLuongNguoi,
    t.Gia,
    dk.SoLuongNguoi * t.Gia
FROM DANGKY_TOUR dk
JOIN TOUR t ON dk.TourID = t.TourID
WHERE dk.DangKyID = @DangKyID;

UPDATE HOADON
SET TongTien = (SELECT SUM(ThanhTien) FROM HOADON_CHITIET WHERE HoaDonID = @HoaDonID)
WHERE HoaDonID = @HoaDonID;

SELECT @HoaDonID;
";
            object result = Db.Scalar(sql,
                new SqlParameter("@DangKyID", dangKyId),
                new SqlParameter("@NhanVienID", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value)
            );
            return Convert.ToInt32(result);
        }

        // 2) Lấy dữ liệu in (RDLC) theo HoaDonID
        public static DataTable GetInvoicePrint(int hoaDonId)
        {
            var sql = @"
SELECT *
FROM VW_HOA_DON_IN
WHERE HoaDonID = @HoaDonID;
";
            return Db.Query(sql, new SqlParameter("@HoaDonID", hoaDonId));
        }
    }
}
