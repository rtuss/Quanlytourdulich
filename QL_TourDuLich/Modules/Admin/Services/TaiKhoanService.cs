using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class TaiKhoanService
    {
        public static DataTable GetAll() => TaiKhoanRepo.GetAll();

        public static void CreateStaff(string username, string plainPass, string hoTen, string vaiTro, string trangThai, string phone, string email)
        {
            var hash = HashHelper.Sha256Hex(plainPass);
            TaiKhoanRepo.Create(username, hash, hoTen, vaiTro, trangThai, phone, email);
        }

        public static void Update(int id, string hoTen, string vaiTro, string trangThai, string phone, string email)
        {
            TaiKhoanRepo.Update(id, hoTen, vaiTro, trangThai, phone, email);
        }

        public static void ResetPassword(int id, string plainPass)
        {
            var hash = HashHelper.Sha256Hex(plainPass);
            TaiKhoanRepo.ResetPassword(id, hash);
        }
    }
}
