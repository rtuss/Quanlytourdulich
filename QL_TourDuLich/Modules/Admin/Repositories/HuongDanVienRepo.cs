using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class HuongDanVienRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT 
    HDVID,
    HoTen,
    DienThoai,
    KinhNghiem,
    NgonNgu,
    TrangThai
FROM HUONGDANVIEN
ORDER BY HDVID DESC;";
            return Db.Query(sql);
        }

        public static DataTable Search(string keyword)
        {
            var sql = @"
SELECT 
    HDVID,
    HoTen,
    DienThoai,
    KinhNghiem,
    NgonNgu,
    TrangThai
FROM HUONGDANVIEN
WHERE
    (@kw IS NULL OR @kw = N'')
    OR HoTen LIKE @kwLike
    OR DienThoai LIKE @kwLike
    OR NgonNgu LIKE @kwLike
ORDER BY HDVID DESC;";

            var kw = (keyword ?? "").Trim();
            return Db.Query(sql,
                new SqlParameter("@kw", kw),
                new SqlParameter("@kwLike", "%" + kw + "%")
            );
        }

        public static int Create(string hoTen, string dienThoai, string kinhNghiem, string ngonNgu, string trangThai)
        {
            var sql = @"
INSERT INTO HUONGDANVIEN (HoTen, DienThoai, KinhNghiem, NgonNgu, TrangThai)
VALUES (@HoTen, @DienThoai, @KinhNghiem, @NgonNgu, @TrangThai);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@HoTen", (object)(hoTen ?? "")),
                new SqlParameter("@DienThoai", (object)(dienThoai ?? "")),
                new SqlParameter("@KinhNghiem", string.IsNullOrWhiteSpace(kinhNghiem) ? (object)DBNull.Value : kinhNghiem),
                new SqlParameter("@NgonNgu", string.IsNullOrWhiteSpace(ngonNgu) ? (object)DBNull.Value : ngonNgu),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "RANH"))
            );
        }

        public static int Update(int hdvId, string hoTen, string dienThoai, string kinhNghiem, string ngonNgu, string trangThai)
        {
            var sql = @"
UPDATE HUONGDANVIEN
SET
    HoTen = @HoTen,
    DienThoai = @DienThoai,
    KinhNghiem = @KinhNghiem,
    NgonNgu = @NgonNgu,
    TrangThai = @TrangThai
WHERE HDVID = @HDVID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@HoTen", (object)(hoTen ?? "")),
                new SqlParameter("@DienThoai", (object)(dienThoai ?? "")),
                new SqlParameter("@KinhNghiem", string.IsNullOrWhiteSpace(kinhNghiem) ? (object)DBNull.Value : kinhNghiem),
                new SqlParameter("@NgonNgu", string.IsNullOrWhiteSpace(ngonNgu) ? (object)DBNull.Value : ngonNgu),
                new SqlParameter("@TrangThai", (object)(trangThai ?? "RANH")),
                new SqlParameter("@HDVID", hdvId)
            );
        }

        public static int Delete(int hdvId)
        {
            var sql = @"DELETE FROM HUONGDANVIEN WHERE HDVID = @HDVID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@HDVID", hdvId));
        }

        public static bool HasPhanCong(int hdvId)
        {
            var sql = @"SELECT COUNT(1) FROM PHANCONG_HDV WHERE HDVID = @HDVID;";
            var obj = Db.ExecuteScalar(sql, new SqlParameter("@HDVID", hdvId));
            var n = 0;
            if (obj != null && obj != DBNull.Value) int.TryParse(obj.ToString(), out n);
            return n > 0;
        }
    }
}
