using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class TourRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT 
    t.TourID,
    t.TenTour,
    t.LoaiTourID,
    lt.TenLoai,
    t.DiaDiem,
    t.Gia,
    t.NgayKhoiHanh,
    t.SoChoToiDa,
    t.TrangThai,
    t.LyDoHuy,
    t.NgayTao
FROM TOUR t
LEFT JOIN LOAITOUR lt ON lt.LoaiTourID = t.LoaiTourID
ORDER BY t.TourID DESC;";
            return Db.Query(sql);
        }

        public static DataTable GetLoaiTour()
        {
            var sql = @"SELECT LoaiTourID, TenLoai FROM LOAITOUR ORDER BY TenLoai;";
            return Db.Query(sql);
        }

        public static int Create(string tenTour, int loaiTourId, string diaDiem, decimal gia, DateTime ngayKhoiHanh,
            int soChoToiDa, string trangThai, string lyDoHuy)
        {
            var sql = @"
INSERT INTO TOUR
(TenTour, LoaiTourID, DiaDiem, Gia, NgayKhoiHanh, SoChoToiDa, TrangThai, LyDoHuy, NgayTao)
VALUES
(@TenTour, @LoaiTourID, @DiaDiem, @Gia, @NgayKhoiHanh, @SoChoToiDa, @TrangThai, @LyDoHuy, GETDATE());";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TenTour", tenTour ?? (object)DBNull.Value),
                new SqlParameter("@LoaiTourID", loaiTourId),
                new SqlParameter("@DiaDiem", (object)(diaDiem ?? "")),
                new SqlParameter("@Gia", gia),
                new SqlParameter("@NgayKhoiHanh", ngayKhoiHanh),
                new SqlParameter("@SoChoToiDa", soChoToiDa),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "DANG_MO")),
                new SqlParameter("@LyDoHuy", string.IsNullOrWhiteSpace(lyDoHuy) ? (object)DBNull.Value : lyDoHuy)
            );
        }

        public static int Update(int tourId, string tenTour, int loaiTourId, string diaDiem, decimal gia,
            DateTime ngayKhoiHanh, int soChoToiDa, string trangThai, string lyDoHuy)
        {
            var sql = @"
UPDATE TOUR
SET
    TenTour = @TenTour,
    LoaiTourID = @LoaiTourID,
    DiaDiem = @DiaDiem,
    Gia = @Gia,
    NgayKhoiHanh = @NgayKhoiHanh,
    SoChoToiDa = @SoChoToiDa,
    TrangThai = @TrangThai,
    LyDoHuy = @LyDoHuy
WHERE TourID = @TourID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TenTour", tenTour ?? (object)DBNull.Value),
                new SqlParameter("@LoaiTourID", loaiTourId),
                new SqlParameter("@DiaDiem", (object)(diaDiem ?? "")),
                new SqlParameter("@Gia", gia),
                new SqlParameter("@NgayKhoiHanh", ngayKhoiHanh),
                new SqlParameter("@SoChoToiDa", soChoToiDa),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "DANG_MO")),
                new SqlParameter("@LyDoHuy", string.IsNullOrWhiteSpace(lyDoHuy) ? (object)DBNull.Value : lyDoHuy),
                new SqlParameter("@TourID", tourId)
            );
        }

        public static int SetTrangThai(int tourId, string trangThai, string lyDoHuy)
        {
            var sql = @"
UPDATE TOUR
SET TrangThai = @TrangThai,
    LyDoHuy  = @LyDoHuy
WHERE TourID = @TourID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TrangThai", (object)(trangThai ?? "DANG_MO")),
                new SqlParameter("@LyDoHuy", string.IsNullOrWhiteSpace(lyDoHuy) ? (object)DBNull.Value : lyDoHuy),
                new SqlParameter("@TourID", tourId)
            );
        }

        public static bool HasDangKy(int tourId)
        {
            var sql = @"SELECT COUNT(1) FROM DANGKY_TOUR WHERE TourID = @TourID;";
            var obj = Db.ExecuteScalar(sql, new SqlParameter("@TourID", tourId));
            var n = 0;
            if (obj != null && obj != DBNull.Value) int.TryParse(obj.ToString(), out n);
            return n > 0;
        }

        public static int Delete(int tourId)
        {
            var sql = @"DELETE FROM TOUR WHERE TourID = @TourID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@TourID", tourId));
        }

        // ===================== HÌNH ẢNH TOUR =====================
        public static DataTable GetHinhAnhByTourId(int tourId)
        {
            var sql = @"
SELECT 
    HinhAnhID,
    DuongDan,
    MoTa,
    LaAnhChinh
FROM HINH_ANH_TOUR
WHERE TourID = @TourID
ORDER BY LaAnhChinh DESC, HinhAnhID;";
            return Db.Query(sql, new SqlParameter("@TourID", tourId));
        }

        public static int AddHinhAnhTour(int tourId, string duongDan, string moTa, bool laAnhChinh)
        {
            // Nếu là ảnh chính -> tắt ảnh chính cũ trước
            if (laAnhChinh)
            {
                var sqlOff = @"UPDATE HINH_ANH_TOUR SET LaAnhChinh = 0 WHERE TourID = @TourID;";
                Db.ExecuteNonQuery(sqlOff, new SqlParameter("@TourID", tourId));
            }

            var sql = @"
INSERT INTO HINH_ANH_TOUR (TourID, DuongDan, MoTa, LaAnhChinh)
VALUES (@TourID, @DuongDan, @MoTa, @LaAnhChinh);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TourID", tourId),
                new SqlParameter("@DuongDan", (object)(duongDan ?? "")),
                new SqlParameter("@MoTa", string.IsNullOrWhiteSpace(moTa) ? (object)DBNull.Value : moTa),
                new SqlParameter("@LaAnhChinh", laAnhChinh ? 1 : 0)
            );
        }

        public static int SetAnhChinh(int tourId, int hinhAnhId)
        {
            var sqlOff = @"UPDATE HINH_ANH_TOUR SET LaAnhChinh = 0 WHERE TourID = @TourID;";
            Db.ExecuteNonQuery(sqlOff, new SqlParameter("@TourID", tourId));

            var sqlOn = @"
UPDATE HINH_ANH_TOUR
SET LaAnhChinh = 1
WHERE HinhAnhID = @HinhAnhID AND TourID = @TourID;";
            return Db.ExecuteNonQuery(sqlOn,
                new SqlParameter("@HinhAnhID", hinhAnhId),
                new SqlParameter("@TourID", tourId)
            );
        }

        public static int DeleteHinhAnh(int tourId, int hinhAnhId)
        {
            var sql = @"DELETE FROM HINH_ANH_TOUR WHERE HinhAnhID = @HinhAnhID AND TourID = @TourID;";
            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@HinhAnhID", hinhAnhId),
                new SqlParameter("@TourID", tourId)
            );
        }

        // ===================== LỊCH TRÌNH TOUR =====================
        public static DataTable GetLichTrinhByTourId(int tourId)
        {
            var sql = @"
SELECT
    TourID,
    NgayThu,
    Buoi,
    DiaDiem,
    NoiDung
FROM LICH_TRINH_TOUR
WHERE TourID = @TourID
ORDER BY NgayThu ASC,
         CASE
            WHEN Buoi = N'Sáng' THEN 1
            WHEN Buoi = N'Chiều' THEN 2
            WHEN Buoi = N'Tối' THEN 3
            ELSE 9
         END;";
            return Db.Query(sql, new SqlParameter("@TourID", tourId));
        }
    }
}
