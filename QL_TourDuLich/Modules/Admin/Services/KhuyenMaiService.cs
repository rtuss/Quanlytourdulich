using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class KhuyenMaiService
    {
        public static DataTable GetAll() => KhuyenMaiRepo.GetAll();

        public static void Create(string ten, int phanTram, DateTime tuNgay, DateTime denNgay, string trangThai)
        {
            Validate(ten, phanTram, tuNgay, denNgay, trangThai);
            KhuyenMaiRepo.Create(ten.Trim(), phanTram, tuNgay, denNgay, trangThai.Trim());
        }

        public static void Update(int id, string ten, int phanTram, DateTime tuNgay, DateTime denNgay, string trangThai)
        {
            if (id <= 0) throw new Exception("Chưa chọn khuyến mãi.");
            Validate(ten, phanTram, tuNgay, denNgay, trangThai);
            KhuyenMaiRepo.Update(id, ten.Trim(), phanTram, tuNgay, denNgay, trangThai.Trim());
        }

        public static void Delete(int id)
        {
            if (id <= 0) throw new Exception("Chưa chọn khuyến mãi.");

            // Nếu bạn có ràng buộc đang dùng ở TOUR / DANGKY... thì bật lên:
            // if (KhuyenMaiRepo.IsUsed(id))
            //     throw new Exception("Không thể xóa: khuyến mãi đang được sử dụng.");

            KhuyenMaiRepo.Delete(id);
        }

        private static void Validate(string ten, int phanTram, DateTime tuNgay, DateTime denNgay, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(ten))
                throw new Exception("Vui lòng nhập Tên khuyến mãi.");

            if (phanTram < 0 || phanTram > 100)
                throw new Exception("Phần trăm giảm phải từ 0 đến 100.");

            if (denNgay.Date < tuNgay.Date)
                throw new Exception("Đến ngày phải lớn hơn hoặc bằng Từ ngày.");

            if (string.IsNullOrWhiteSpace(trangThai))
                throw new Exception("Vui lòng chọn Trạng thái.");
        }
    }
}
