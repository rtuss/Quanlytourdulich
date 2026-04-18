using System;
using System.Data;
using System.Data.SqlClient;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Repositories
{
    public static class LoaiTourRepo
    {
        public static DataTable GetAll()
        {
            var sql = @"
SELECT LoaiTourID, TenLoai
FROM LOAITOUR
ORDER BY LoaiTourID DESC;";
            return Db.Query(sql);
        }

        public static int Create(string tenLoai)
        {
            var sql = @"
INSERT INTO LOAITOUR(TenLoai)
VALUES (@TenLoai);";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TenLoai", string.IsNullOrWhiteSpace(tenLoai) ? (object)DBNull.Value : tenLoai.Trim())
            );
        }

        public static int Update(int loaiTourId, string tenLoai)
        {
            var sql = @"
UPDATE LOAITOUR
SET TenLoai = @TenLoai
WHERE LoaiTourID = @LoaiTourID;";

            return Db.ExecuteNonQuery(sql,
                new SqlParameter("@TenLoai", string.IsNullOrWhiteSpace(tenLoai) ? (object)DBNull.Value : tenLoai.Trim()),
                new SqlParameter("@LoaiTourID", loaiTourId)
            );
        }

        public static bool HasTourUsing(int loaiTourId)
        {
            // nếu bảng tour của bạn tên khác (TOUR / TOUR_DULICH) thì sửa tại đây
            var sql = @"SELECT COUNT(1) FROM TOUR WHERE LoaiTourID = @LoaiTourID;";
            var obj = Db.ExecuteScalar(sql, new SqlParameter("@LoaiTourID", loaiTourId));

            int n = 0;
            if (obj != null && obj != DBNull.Value) int.TryParse(obj.ToString(), out n);
            return n > 0;
        }

        public static int Delete(int loaiTourId)
        {
            var sql = @"DELETE FROM LOAITOUR WHERE LoaiTourID = @LoaiTourID;";
            return Db.ExecuteNonQuery(sql, new SqlParameter("@LoaiTourID", loaiTourId));
        }
    }
}
