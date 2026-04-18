using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class DangKyService
    {
        public static DataTable GetAll() => DangKyRepo.GetAll();
        public static DataTable GetTourLookup() => DangKyRepo.GetTourLookup();
        public static DataTable GetKhachHangLookup() => DangKyRepo.GetKhachHangLookup();

        public static int Create(int tourId, int khachHangId, int soLuongNguoi, DateTime ngayDangKy, string trangThai, int? nhanVienId)
        {
            trangThai = NormalizeTrangThai(trangThai);

            ValidateDangKy(tourId, soLuongNguoi, trangThai, null);

            return DangKyRepo.Create(
                tourId,
                khachHangId,
                soLuongNguoi,
                ngayDangKy,
                trangThai,
                nhanVienId
            );
        }

        public static int Update(int dangKyId, int tourId, int khachHangId, int soLuongNguoi, DateTime ngayDangKy, string trangThai, int? nhanVienId)
        {
            trangThai = NormalizeTrangThai(trangThai);

            ValidateDangKy(tourId, soLuongNguoi, trangThai, dangKyId);

            return DangKyRepo.Update(
                dangKyId,
                tourId,
                khachHangId,
                soLuongNguoi,
                ngayDangKy,
                trangThai,
                nhanVienId
            );
        }

        public static int Delete(int dangKyId) => DangKyRepo.Delete(dangKyId);
        public static DataTable GetInvoicePreview(int dangKyId) => DangKyRepo.GetInvoicePreview(dangKyId);

        private static string NormalizeTrangThai(string trangThai)
        {
            return string.IsNullOrWhiteSpace(trangThai)
                ? "DANG_KY"
                : trangThai.Trim().ToUpper();
        }

        private static void ValidateDangKy(int tourId, int soLuongNguoiMoi, string trangThaiMoi, int? excludeDangKyId)
        {
            if (tourId <= 0)
                throw new Exception("Tour không hợp lệ.");

            if (soLuongNguoiMoi <= 0)
                throw new Exception("Số lượng người phải lớn hơn 0.");

            if (trangThaiMoi == "HUY")
                return;

            int soLuongToiDa = DangKyRepo.GetSoLuongToiDaCuaTour(tourId);
            int tongDaDangKy = DangKyRepo.GetTongSoNguoiDaDangKy(tourId, excludeDangKyId);

            if (soLuongToiDa <= 0)
                throw new Exception("Tour chưa thiết lập số lượng tối đa.");

            int conLai = soLuongToiDa - tongDaDangKy;

            if (soLuongNguoiMoi > conLai)
            {
                if (conLai < 0) conLai = 0;

                throw new Exception(
                    $"Tour này chỉ còn {conLai} chỗ trống, không thể đăng ký {soLuongNguoiMoi} người."
                );
            }
        }
    }
}