using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class HoaDonService
    {
        public static int CreateFromDangKy(int dangKyId, int? nhanVienId)
            => HoaDonRepo.CreateFromDangKy(dangKyId, nhanVienId);

        public static DataTable GetInvoicePrint(int hoaDonId)
            => HoaDonRepo.GetInvoicePrint(hoaDonId);
    }
}
