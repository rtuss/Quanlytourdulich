using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Configuration;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class PhanCongHDVRepo
    {
        // ✅ lấy connection string kể cả khi Db.ConnectionString là private/protected
        private static string GetConnString()
        {
            // 1) thử lấy từ Db (public/private) bằng reflection
            try
            {
                var t = typeof(Db);

                // property names hay gặp
                foreach (var name in new[] { "ConnectionString", "CONNECTION_STRING", "ConnStr", "CONN_STR", "CONNSTRING" })
                {
                    var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (p != null)
                    {
                        var v = p.GetValue(null, null)?.ToString();
                        if (!string.IsNullOrWhiteSpace(v)) return v;
                    }

                    var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (f != null)
                    {
                        var v = f.GetValue(null)?.ToString();
                        if (!string.IsNullOrWhiteSpace(v)) return v;
                    }
                }
            }
            catch { }

            // 2) fallback: lấy từ app.config (nếu có)
            try
            {
                // ưu tiên cái đầu tiên
                if (ConfigurationManager.ConnectionStrings != null && ConfigurationManager.ConnectionStrings.Count > 0)
                    return ConfigurationManager.ConnectionStrings[0].ConnectionString;
            }
            catch { }

            throw new Exception("Không lấy được ConnectionString. Hãy kiểm tra Db/Config.");
        }

        private static SqlConnection NewConn() => new SqlConnection(GetConnString());

        // ================== TOURS (để đổ combobox) ==================
        public static DataTable GetToursDangMo()
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                SELECT TourID, TenTour
                FROM TOUR
                ORDER BY TourID DESC;", conn))
            {
                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }

        // ================== LIST ASSIGNMENTS ==================
        public static DataTable GetByHdv(int hdvId)
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                SELECT pc.PhanCongID, pc.TourID, pc.HDVID, t.TenTour, pc.TuNgay, pc.DenNgay
                FROM PHANCONG_HDV pc
                LEFT JOIN TOUR t ON t.TourID = pc.TourID
                WHERE pc.HDVID = @HDVID
                ORDER BY pc.TuNgay DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@HDVID", hdvId);

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd)) da.Fill(dt);
                return dt;
            }
        }

        public static int Create(int tourId, int hdvId, DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                INSERT INTO PHANCONG_HDV (TourID, HDVID, TuNgay, DenNgay)
                VALUES (@TourID, @HDVID, @TuNgay, @DenNgay);
                SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@TourID", tourId);
                cmd.Parameters.AddWithValue("@HDVID", hdvId);
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void Update(int phanCongId, int tourId, int hdvId, DateTime tuNgay, DateTime denNgay)
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                UPDATE PHANCONG_HDV
                SET TourID=@TourID, HDVID=@HDVID, TuNgay=@TuNgay, DenNgay=@DenNgay
                WHERE PhanCongID=@PhanCongID;", conn))
            {
                cmd.Parameters.AddWithValue("@PhanCongID", phanCongId);
                cmd.Parameters.AddWithValue("@TourID", tourId);
                cmd.Parameters.AddWithValue("@HDVID", hdvId);
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int phanCongId)
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                DELETE FROM PHANCONG_HDV
                WHERE PhanCongID=@PhanCongID;", conn))
            {
                cmd.Parameters.AddWithValue("@PhanCongID", phanCongId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static DataTable GetByHDV(int hdvId)
        {
            using (var cn = Db.Open())
            using (var cmd = new SqlCommand(@"
                SELECT pc.PhanCongID,
                       pc.TourID,
                       pc.HDVID,
                       t.TenTour,
                       pc.TuNgay,
                       pc.DenNgay
                FROM PHANCONG_HDV pc
                JOIN TOUR t ON pc.TourID = t.TourID
                WHERE pc.HDVID = @hdvId
                ORDER BY pc.TuNgay DESC
            ", cn))
            {
                cmd.Parameters.AddWithValue("@hdvId", hdvId);

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                return dt;
            }
        }

        // ✅ check trùng lịch
        public static bool IsOverlapped(int hdvId, DateTime tuNgay, DateTime denNgay, int excludePhanCongId = 0)
        {
            using (var conn = NewConn())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM PHANCONG_HDV
                WHERE HDVID=@HDVID
                  AND (@Exclude=0 OR PhanCongID<>@Exclude)
                  AND (TuNgay <= @DenNgay AND DenNgay >= @TuNgay);", conn))
            {
                cmd.Parameters.AddWithValue("@HDVID", hdvId);
                cmd.Parameters.AddWithValue("@Exclude", excludePhanCongId);
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
