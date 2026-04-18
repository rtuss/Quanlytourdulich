using System;
using System.Data;
using QL_TourDuLich.Modules.Admin.Repositories;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class ThongKeService
    {
        public static DataTable GetKpi(DateTime from, DateTime to) => ThongKeRepo.GetKpi(from, to);

        public static DataTable GetDangKySeries(string granularity, DateTime from, DateTime to)
            => ThongKeRepo.GetDangKySeries(granularity, from, to);

        public static DataTable GetDoanhThuSeries(string granularity, DateTime from, DateTime to)
            => ThongKeRepo.GetDoanhThuSeries(granularity, from, to);

        public static DataTable GetTrangThaiDangKy(DateTime from, DateTime to)
            => ThongKeRepo.GetTrangThaiDangKy(from, to);

        public static DataTable GetTopTourDoanhThu(DateTime from, DateTime to, int topN = 5)
            => ThongKeRepo.GetTopTourDoanhThu(from, to, topN);
    }
}
