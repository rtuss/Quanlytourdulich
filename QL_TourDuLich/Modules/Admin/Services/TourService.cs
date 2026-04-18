using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class TourService
    {
        // ===================== TOUR =====================
        public static DataTable GetAll() => TourRepo.GetAll();
        public static DataTable GetLoaiTour() => TourRepo.GetLoaiTour();

        public static void Create(string tenTour, int loaiTourId, string diaDiem, decimal gia, DateTime ngayKhoiHanh,
            int soChoToiDa, string trangThai, string lyDoHuy)
        {
            if (string.IsNullOrWhiteSpace(tenTour)) throw new ArgumentException("Tên tour không được rỗng.");
            if (soChoToiDa <= 0) throw new ArgumentException("Số chỗ tối đa phải > 0.");
            if (string.Equals(trangThai, "HUY", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(lyDoHuy))
                throw new ArgumentException("Hủy tour phải có lý do.");

            TourRepo.Create(tenTour, loaiTourId, diaDiem, gia, ngayKhoiHanh, soChoToiDa, trangThai, lyDoHuy);
        }

        public static void Update(int tourId, string tenTour, int loaiTourId, string diaDiem, decimal gia,
            DateTime ngayKhoiHanh, int soChoToiDa, string trangThai, string lyDoHuy)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (string.IsNullOrWhiteSpace(tenTour)) throw new ArgumentException("Tên tour không được rỗng.");
            if (soChoToiDa <= 0) throw new ArgumentException("Số chỗ tối đa phải > 0.");
            if (string.Equals(trangThai, "HUY", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(lyDoHuy))
                throw new ArgumentException("Hủy tour phải có lý do.");

            TourRepo.Update(tourId, tenTour, loaiTourId, diaDiem, gia, ngayKhoiHanh, soChoToiDa, trangThai, lyDoHuy);
        }

        public static void SetTrangThai(int tourId, string trangThai, string lyDoHuy)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (string.Equals(trangThai, "HUY", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(lyDoHuy))
                throw new ArgumentException("Hủy tour phải có lý do.");

            TourRepo.SetTrangThai(tourId, trangThai, lyDoHuy);
        }

        public static bool HasDangKy(int tourId) => tourId > 0 && TourRepo.HasDangKy(tourId);

        public static void Delete(int tourId)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (TourRepo.HasDangKy(tourId)) throw new InvalidOperationException("Không thể xóa tour đã có khách đăng ký.");
            TourRepo.Delete(tourId);
        }

        // ===================== LỊCH TRÌNH TOUR =====================
        public static DataTable GetLichTrinhByTourId(int tourId)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            return TourRepo.GetLichTrinhByTourId(tourId);
        }

        // ===================== HÌNH ẢNH TOUR =====================
        public static DataTable GetHinhAnhTour(int tourId)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            return TourRepo.GetHinhAnhByTourId(tourId);
        }

        public static void AddHinhAnhTour(int tourId, string duongDan, string moTa, bool laAnhChinh)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (string.IsNullOrWhiteSpace(duongDan)) throw new ArgumentException("Đường dẫn ảnh không hợp lệ.");
            TourRepo.AddHinhAnhTour(tourId, duongDan, moTa, laAnhChinh);
        }

        public static void SetAnhChinh(int tourId, int hinhAnhId)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (hinhAnhId <= 0) throw new ArgumentException("HinhAnhID không hợp lệ.");
            TourRepo.SetAnhChinh(tourId, hinhAnhId);
        }

        public static void DeleteHinhAnh(int tourId, int hinhAnhId)
        {
            if (tourId <= 0) throw new ArgumentException("TourID không hợp lệ.");
            if (hinhAnhId <= 0) throw new ArgumentException("HinhAnhID không hợp lệ.");
            TourRepo.DeleteHinhAnh(tourId, hinhAnhId);
        }
    }
}
