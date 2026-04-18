using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class KhachHangService
    {
        public static DataTable GetAll() => KhachHangRepo.GetAll();
        public static DataTable GetLoaiKhach() => KhachHangRepo.GetLoaiKhach();

        public static int Create(string hoTen, string dienThoai, string email, string diaChi, string loaiKhach)
        {
            return KhachHangRepo.Create(
                (hoTen ?? "").Trim(),
                (dienThoai ?? "").Trim(),
                (email ?? "").Trim(),
                (diaChi ?? "").Trim(),
                (loaiKhach ?? "").Trim()
            );
        }

        public static int Update(int id, string hoTen, string dienThoai, string email, string diaChi, string loaiKhach)
        {
            return KhachHangRepo.Update(
                id,
                (hoTen ?? "").Trim(),
                (dienThoai ?? "").Trim(),
                (email ?? "").Trim(),
                (diaChi ?? "").Trim(),
                (loaiKhach ?? "").Trim()
            );
        }

        public static bool HasDangKy(int id) => KhachHangRepo.HasDangKy(id);
        public static int Delete(int id) => KhachHangRepo.Delete(id);
    }
}
