using System;
using System.Data;

namespace QL_TourDuLich.Modules.Admin.Services
{
    public static class PhanCongHDVService
    {
        public static DataTable GetToursDangMo()
            => PhanCongHDVRepo.GetToursDangMo();

        // ✅ đúng tên hàm để Frm gọi
        public static DataTable GetByHdv(int hdvId)
            => PhanCongHDVRepo.GetByHdv(hdvId);

        public static int Create(int tourId, int hdvId, DateTime tuNgay, DateTime denNgay)
        {
            Validate(tourId, hdvId, tuNgay, denNgay);

            if (PhanCongHDVRepo.IsOverlapped(hdvId, tuNgay, denNgay))
                throw new Exception("HDV đã có lịch phân công trùng thời gian.");

            return PhanCongHDVRepo.Create(tourId, hdvId, tuNgay, denNgay);
        }

        public static void Update(int phanCongId, int tourId, int hdvId, DateTime tuNgay, DateTime denNgay)
        {
            if (phanCongId <= 0) throw new Exception("Phân công không hợp lệ.");
            Validate(tourId, hdvId, tuNgay, denNgay);

            if (PhanCongHDVRepo.IsOverlapped(hdvId, tuNgay, denNgay, excludePhanCongId: phanCongId))
                throw new Exception("HDV đã có lịch phân công trùng thời gian.");

            PhanCongHDVRepo.Update(phanCongId, tourId, hdvId, tuNgay, denNgay);
        }


        public static void Delete(int phanCongId)
        {
            if (phanCongId <= 0) throw new Exception("Chọn phân công để xóa.");
            PhanCongHDVRepo.Delete(phanCongId);

        }
        public static DataTable GetByHDV(int hdvId)
        {
            if (hdvId <= 0)
                return new DataTable();

            return PhanCongHDVRepo.GetByHDV(hdvId);
        }

        private static void Validate(int tourId, int hdvId, DateTime tuNgay, DateTime denNgay)
        {
            if (tourId <= 0) throw new Exception("Tour không hợp lệ.");
            if (hdvId <= 0) throw new Exception("HDV không hợp lệ.");
            if (denNgay.Date < tuNgay.Date) throw new Exception("Đến ngày phải >= Từ ngày.");
        }
    }
}
