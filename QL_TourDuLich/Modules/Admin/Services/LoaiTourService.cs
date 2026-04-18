using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class LoaiTourService
    {
        public static DataTable GetAll() => LoaiTourRepo.GetAll();

        public static void Create(string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
                throw new Exception("Vui lòng nhập Tên loại tour.");

            LoaiTourRepo.Create(tenLoai.Trim());
        }

        public static void Update(int loaiTourId, string tenLoai)
        {
            if (loaiTourId <= 0) throw new Exception("Chưa chọn loại tour.");
            if (string.IsNullOrWhiteSpace(tenLoai))
                throw new Exception("Vui lòng nhập Tên loại tour.");

            LoaiTourRepo.Update(loaiTourId, tenLoai.Trim());
        }

        public static void Delete(int loaiTourId)
        {
            if (loaiTourId <= 0) throw new Exception("Chưa chọn loại tour.");

            if (LoaiTourRepo.HasTourUsing(loaiTourId))
                throw new Exception("Không thể xóa: đang có tour sử dụng loại tour này.");

            LoaiTourRepo.Delete(loaiTourId);
        }
    }
}
