using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class ThongKeRepo
    {
        public static DataTable GetKpi(DateTime from, DateTime to)
        {
            var sql = @"
SELECT
    TongDangKy = (SELECT COUNT(1) FROM DANGKY_TOUR WHERE NgayDangKy >= @From AND NgayDangKy < @To),
    TongHuy    = (SELECT COUNT(1) FROM DANGKY_TOUR WHERE NgayDangKy >= @From AND NgayDangKy < @To AND TrangThai = 'HUY'),
    DoanhThu   = (SELECT ISNULL(SUM(SoTien),0) FROM THANHTOAN WHERE NgayThanhToan >= @From AND NgayThanhToan < @To),
    CongNo     = (SELECT ISNULL(SUM(TongNo),0) FROM CONGNO_KHACHHANG WHERE TrangThai = 'CON_NO');";

            return Db.Query(sql,
                new SqlParameter("@From", from),
                new SqlParameter("@To", to)
            );
        }

        public static DataTable GetDangKySeries(string granularity, DateTime from, DateTime to)
        {
            // granularity: "WEEK" / "MONTH" / "YEAR"
            // Dùng ISO week để đỡ phụ thuộc DATEFIRST
            if (string.Equals(granularity, "YEAR", StringComparison.OrdinalIgnoreCase))
            {
                var sql = @"
SELECT
    [Label] = CAST(DATEPART(YEAR, dk.NgayDangKy) AS NVARCHAR(10)),
    [Value] = COUNT(1)
FROM DANGKY_TOUR dk
WHERE dk.NgayDangKy >= @From AND dk.NgayDangKy < @To
GROUP BY DATEPART(YEAR, dk.NgayDangKy)
ORDER BY DATEPART(YEAR, dk.NgayDangKy);";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }

            if (string.Equals(granularity, "MONTH", StringComparison.OrdinalIgnoreCase))
            {
                var sql = @"
SELECT
    [Label] = FORMAT(CONVERT(date, dk.NgayDangKy), 'MM/yyyy'),
    [Value] = COUNT(1),
    SortKey = (DATEPART(YEAR, dk.NgayDangKy) * 100 + DATEPART(MONTH, dk.NgayDangKy))
FROM DANGKY_TOUR dk
WHERE dk.NgayDangKy >= @From AND dk.NgayDangKy < @To
GROUP BY DATEPART(YEAR, dk.NgayDangKy), DATEPART(MONTH, dk.NgayDangKy), FORMAT(CONVERT(date, dk.NgayDangKy), 'MM/yyyy')
ORDER BY SortKey;";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }

            // WEEK (ISO week)
            {
                var sql = @"
SELECT
    [Label] = CONCAT('W', RIGHT('0' + CAST(DATEPART(ISO_WEEK, dk.NgayDangKy) AS NVARCHAR(2)), 2), '/', DATEPART(YEAR, dk.NgayDangKy)),
    [Value] = COUNT(1),
    SortKey = (DATEPART(YEAR, dk.NgayDangKy) * 100 + DATEPART(ISO_WEEK, dk.NgayDangKy))
FROM DANGKY_TOUR dk
WHERE dk.NgayDangKy >= @From AND dk.NgayDangKy < @To
GROUP BY DATEPART(YEAR, dk.NgayDangKy), DATEPART(ISO_WEEK, dk.NgayDangKy)
ORDER BY SortKey;";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }
        }

        public static DataTable GetDoanhThuSeries(string granularity, DateTime from, DateTime to)
        {
            if (string.Equals(granularity, "YEAR", StringComparison.OrdinalIgnoreCase))
            {
                var sql = @"
SELECT
    [Label] = CAST(DATEPART(YEAR, tt.NgayThanhToan) AS NVARCHAR(10)),
    [Value] = ISNULL(SUM(tt.SoTien),0)
FROM THANHTOAN tt
WHERE tt.NgayThanhToan >= @From AND tt.NgayThanhToan < @To
GROUP BY DATEPART(YEAR, tt.NgayThanhToan)
ORDER BY DATEPART(YEAR, tt.NgayThanhToan);";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }

            if (string.Equals(granularity, "MONTH", StringComparison.OrdinalIgnoreCase))
            {
                var sql = @"
SELECT
    [Label] = FORMAT(CONVERT(date, tt.NgayThanhToan), 'MM/yyyy'),
    [Value] = ISNULL(SUM(tt.SoTien),0),
    SortKey = (DATEPART(YEAR, tt.NgayThanhToan) * 100 + DATEPART(MONTH, tt.NgayThanhToan))
FROM THANHTOAN tt
WHERE tt.NgayThanhToan >= @From AND tt.NgayThanhToan < @To
GROUP BY DATEPART(YEAR, tt.NgayThanhToan), DATEPART(MONTH, tt.NgayThanhToan), FORMAT(CONVERT(date, tt.NgayThanhToan), 'MM/yyyy')
ORDER BY SortKey;";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }

            // WEEK (ISO)
            {
                var sql = @"
SELECT
    [Label] = CONCAT('W', RIGHT('0' + CAST(DATEPART(ISO_WEEK, tt.NgayThanhToan) AS NVARCHAR(2)), 2), '/', DATEPART(YEAR, tt.NgayThanhToan)),
    [Value] = ISNULL(SUM(tt.SoTien),0),
    SortKey = (DATEPART(YEAR, tt.NgayThanhToan) * 100 + DATEPART(ISO_WEEK, tt.NgayThanhToan))
FROM THANHTOAN tt
WHERE tt.NgayThanhToan >= @From AND tt.NgayThanhToan < @To
GROUP BY DATEPART(YEAR, tt.NgayThanhToan), DATEPART(ISO_WEEK, tt.NgayThanhToan)
ORDER BY SortKey;";

                return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
            }
        }

        public static DataTable GetTrangThaiDangKy(DateTime from, DateTime to)
        {
            var sql = @"
SELECT
    TrangThai = ISNULL(dk.TrangThai,'(NULL)'),
    SoLuong   = COUNT(1)
FROM DANGKY_TOUR dk
WHERE dk.NgayDangKy >= @From AND dk.NgayDangKy < @To
GROUP BY dk.TrangThai
ORDER BY SoLuong DESC;";
            return Db.Query(sql, new SqlParameter("@From", from), new SqlParameter("@To", to));
        }

        public static DataTable GetTopTourDoanhThu(DateTime from, DateTime to, int topN)
        {
            var sql = @"
SELECT TOP (@TopN)
    t.TourID,
    t.TenTour,
    DoanhThu = ISNULL(SUM(tt.SoTien),0)
FROM THANHTOAN tt
JOIN DANGKY_TOUR dk ON dk.DangKyID = tt.DangKyID
JOIN TOUR t ON t.TourID = dk.TourID
WHERE tt.NgayThanhToan >= @From AND tt.NgayThanhToan < @To
GROUP BY t.TourID, t.TenTour
ORDER BY DoanhThu DESC;";
            return Db.Query(sql,
                new SqlParameter("@TopN", topN),
                new SqlParameter("@From", from),
                new SqlParameter("@To", to)
            );
        }
    }
}
