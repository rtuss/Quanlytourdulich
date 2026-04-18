using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class ThanhToanService
    {
        public static DataTable GetAll() => ThanhToanRepo.GetAll();
        public static DataTable GetDangKyLookup() => ThanhToanRepo.GetDangKyLookup();
        public static DataTable GetCongNoTongHop() => ThanhToanRepo.GetCongNoTongHop();

        public static int Create(int dangKyId, decimal soTien, DateTime ngayThanhToan, string trangThai, string ghiChu)
        {
            ValidateSoTien(soTien);

            string finalStatus = TinhTrangThaiSauKhiThemMoi(dangKyId, soTien);

            int result = ThanhToanRepo.Create(
                dangKyId,
                soTien,
                ngayThanhToan,
                finalStatus,
                (ghiChu ?? "").Trim()
            );

            // đồng bộ lại toàn bộ các giao dịch cùng đăng ký
            ThanhToanRepo.UpdateStatusByDangKyId(dangKyId, finalStatus);

            return result;
        }

        public static int Update(int thanhToanId, int dangKyId, decimal soTien, DateTime ngayThanhToan, string trangThai, string ghiChu)
        {
            ValidateSoTien(soTien);

            var oldDt = ThanhToanRepo.GetById(thanhToanId);
            if (oldDt == null || oldDt.Rows.Count == 0)
                throw new Exception("Không tìm thấy giao dịch thanh toán cần cập nhật.");

            int dangKyIdCu = Convert.ToInt32(oldDt.Rows[0]["DangKyID"]);
            decimal soTienCu = Convert.ToDecimal(oldDt.Rows[0]["SoTien"]);
            bool daHuy = Convert.ToInt32(oldDt.Rows[0]["DaHuy"]) == 1;

            if (daHuy)
                throw new Exception("Giao dịch này đã bị hủy, không thể cập nhật.");

            string finalStatusMoi = TinhTrangThaiSauKhiCapNhat(thanhToanId, dangKyId, soTien, dangKyIdCu, soTienCu);

            int result = ThanhToanRepo.Update(
                thanhToanId,
                dangKyId,
                soTien,
                ngayThanhToan,
                finalStatusMoi,
                (ghiChu ?? "").Trim()
            );

            // Nếu đổi sang đăng ký khác, phải tính lại cả 2 bên
            string statusDangKyMoi = TinhTrangThaiHienTai(dangKyId);
            ThanhToanRepo.UpdateStatusByDangKyId(dangKyId, statusDangKyMoi);

            if (dangKyIdCu != dangKyId)
            {
                string statusDangKyCu = TinhTrangThaiHienTai(dangKyIdCu);
                ThanhToanRepo.UpdateStatusByDangKyId(dangKyIdCu, statusDangKyCu);
            }

            return result;
        }

        public static int SoftDelete(int thanhToanId)
        {
            var oldDt = ThanhToanRepo.GetById(thanhToanId);
            if (oldDt == null || oldDt.Rows.Count == 0)
                throw new Exception("Không tìm thấy giao dịch thanh toán.");

            int dangKyId = Convert.ToInt32(oldDt.Rows[0]["DangKyID"]);
            bool daHuy = Convert.ToInt32(oldDt.Rows[0]["DaHuy"]) == 1;

            if (daHuy)
                throw new Exception("Giao dịch này đã bị hủy trước đó.");

            int result = ThanhToanRepo.SoftDelete(thanhToanId);

            string statusSauHuy = TinhTrangThaiHienTai(dangKyId);
            ThanhToanRepo.UpdateStatusByDangKyId(dangKyId, statusSauHuy);

            return result;
        }

        private static void ValidateSoTien(decimal soTien)
        {
            if (soTien <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0.");
        }

        private static string TinhTrangThaiSauKhiThemMoi(int dangKyId, decimal soTienMoi)
        {
            var dt = ThanhToanRepo.GetCongNoByDangKyId(dangKyId);

            decimal tongPhaiTra = 0;
            decimal tongDaThanhToan = 0;

            if (dt != null && dt.Rows.Count > 0)
            {
                tongPhaiTra = Convert.ToDecimal(dt.Rows[0]["TongTienPhaiTra"]);
                tongDaThanhToan = Convert.ToDecimal(dt.Rows[0]["TongDaThanhToan"]);
            }

            decimal tongSauThem = tongDaThanhToan + soTienMoi;
            return tongSauThem >= tongPhaiTra ? "DA_THANH_TOAN" : "CHUA_DU";
        }

        private static string TinhTrangThaiSauKhiCapNhat(int thanhToanId, int dangKyIdMoi, decimal soTienMoi, int dangKyIdCu, decimal soTienCu)
        {
            // Nếu vẫn cùng 1 đăng ký:
            // tổng mới = tổng hiện tại - tiền cũ + tiền mới
            if (dangKyIdMoi == dangKyIdCu)
            {
                var dt = ThanhToanRepo.GetCongNoByDangKyId(dangKyIdMoi);

                decimal tongPhaiTra = 0;
                decimal tongDaThanhToan = 0;

                if (dt != null && dt.Rows.Count > 0)
                {
                    tongPhaiTra = Convert.ToDecimal(dt.Rows[0]["TongTienPhaiTra"]);
                    tongDaThanhToan = Convert.ToDecimal(dt.Rows[0]["TongDaThanhToan"]);
                }

                decimal tongSauCapNhat = tongDaThanhToan - soTienCu + soTienMoi;
                return tongSauCapNhat >= tongPhaiTra ? "DA_THANH_TOAN" : "CHUA_DU";
            }

            // Nếu đổi sang đăng ký khác:
            // với đăng ký mới, tổng mới = tổng hiện tại của đăng ký mới + tiền mới
            var dtMoi = ThanhToanRepo.GetCongNoByDangKyId(dangKyIdMoi);

            decimal tongPhaiTraMoi = 0;
            decimal tongDaThanhToanMoi = 0;

            if (dtMoi != null && dtMoi.Rows.Count > 0)
            {
                tongPhaiTraMoi = Convert.ToDecimal(dtMoi.Rows[0]["TongTienPhaiTra"]);
                tongDaThanhToanMoi = Convert.ToDecimal(dtMoi.Rows[0]["TongDaThanhToan"]);
            }

            decimal tongSauCapNhatMoi = tongDaThanhToanMoi + soTienMoi;
            return tongSauCapNhatMoi >= tongPhaiTraMoi ? "DA_THANH_TOAN" : "CHUA_DU";
        }

        private static string TinhTrangThaiHienTai(int dangKyId)
        {
            var dt = ThanhToanRepo.GetCongNoByDangKyId(dangKyId);

            decimal tongPhaiTra = 0;
            decimal tongDaThanhToan = 0;

            if (dt != null && dt.Rows.Count > 0)
            {
                tongPhaiTra = Convert.ToDecimal(dt.Rows[0]["TongTienPhaiTra"]);
                tongDaThanhToan = Convert.ToDecimal(dt.Rows[0]["TongDaThanhToan"]);
            }

            return tongDaThanhToan >= tongPhaiTra ? "DA_THANH_TOAN" : "CHUA_DU";
        }
    }
}