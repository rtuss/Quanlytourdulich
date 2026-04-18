using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class HuongDanVienService
    {
        public static DataTable GetAll() => HuongDanVienRepo.GetAll();

        public static DataTable Search(string keyword) => HuongDanVienRepo.Search(keyword);

        public static void Create(string hoTen, string dienThoai, string kinhNghiem, string ngonNgu, string trangThai)
        {
            if (string.IsNullOrWhiteSpace(hoTen))
                throw new Exception("Họ tên không được để trống.");

            HuongDanVienRepo.Create(hoTen.Trim(), (dienThoai ?? "").Trim(), kinhNghiem, ngonNgu, trangThai);
        }

        public static void Update(int hdvId, string hoTen, string dienThoai, string kinhNghiem, string ngonNgu, string trangThai)
        {
            if (hdvId <= 0) throw new Exception("Chưa chọn HDV.");
            if (string.IsNullOrWhiteSpace(hoTen))
                throw new Exception("Họ tên không được để trống.");

            HuongDanVienRepo.Update(hdvId, hoTen.Trim(), (dienThoai ?? "").Trim(), kinhNghiem, ngonNgu, trangThai);
        }

        public static void Delete(int hdvId)
        {
            if (hdvId <= 0) throw new Exception("Chưa chọn HDV.");

            if (HuongDanVienRepo.HasPhanCong(hdvId))
                throw new Exception("HDV đang có phân công. Vui lòng xóa phân công trước.");

            HuongDanVienRepo.Delete(hdvId);
        }
    }
}
