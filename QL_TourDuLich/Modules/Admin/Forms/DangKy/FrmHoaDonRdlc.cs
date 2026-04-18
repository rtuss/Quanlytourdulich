using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace QL_TourDuLich.Modules.Admin.Forms.DangKy
{
    public class FrmHoaDonRdlc : Form
    {
        private readonly DataTable _dt;
        private readonly ReportViewer rv = new ReportViewer();

        // ⚠️ Dataset name trong RDLC (DataSet/ReportDataSource name)
        // Nếu trong RDLC bạn đặt khác, đổi ở đây cho khớp.
        private const string DATASET_NAME = "InvoiceDS";

        // ⚠️ Tên file report
        private const string REPORT_FILE = "rptHoaDon.rdlc";

        public FrmHoaDonRdlc(DataTable dt)
        {
            _dt = dt ?? throw new ArgumentNullException(nameof(dt));

            Text = "Preview Hóa đơn";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            rv.Dock = DockStyle.Fill;
            Controls.Add(rv);

            Load += FrmHoaDonRdlc_Load;
        }

        private void FrmHoaDonRdlc_Load(object sender, EventArgs e)
        {
            try
            {
                // 1) Tìm đường dẫn report
                var reportPath = FindReportPath();
                if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
                {
                    MessageBox.Show(
                        "Không tìm thấy file RDLC: " + REPORT_FILE + "\n\n" +
                        "Bạn kiểm tra:\n" +
                        "1) File có đúng tên '" + REPORT_FILE + "'\n" +
                        "2) Properties của RDLC: Copy to Output Directory = Copy if newer\n" +
                        "3) Nếu đặt trong thư mục Reports thì đường dẫn sẽ là: Reports\\" + REPORT_FILE,
                        "Thiếu file report",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    Close();
                    return;
                }

                // 2) Setup report
                rv.Reset();
                rv.ProcessingMode = ProcessingMode.Local;

                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.ReportPath = reportPath;

                // 3) Add datasource
                var rds = new ReportDataSource(DATASET_NAME, _dt);
                rv.LocalReport.DataSources.Add(rds);

                // (Tuỳ chọn) parameter
                // rv.LocalReport.SetParameters(new ReportParameter("CompanyName", "QL Tour Du Lich"));

                rv.RefreshReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi load report:\n" + ex.Message +
                    "\n\nGợi ý: kiểm tra DATASET_NAME trong code phải trùng tên DataSet trong RDLC.",
                    "RDLC Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Close();
            }
        }

        private string FindReportPath()
        {
            // Ưu tiên: chạy ra bin\Debug / bin\Release
            // 1) bin\...\rptHoaDon.rdlc
            // 2) bin\...\Reports\rptHoaDon.rdlc
            // 3) root project\rptHoaDon.rdlc
            // 4) root project\Reports\rptHoaDon.rdlc

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var p1 = Path.Combine(baseDir, REPORT_FILE);
            if (File.Exists(p1)) return p1;

            var p2 = Path.Combine(baseDir, "Reports", REPORT_FILE);
            if (File.Exists(p2)) return p2;

            // tìm ngược lên để ra thư mục project
            // baseDir thường là: ...\QL_TourDuLich\bin\Debug\
            // lên 2 cấp => ...\QL_TourDuLich\
            DirectoryInfo di = new DirectoryInfo(baseDir);
            for (int i = 0; i < 6 && di != null; i++)
            {
                var root = di.FullName;

                var p3 = Path.Combine(root, REPORT_FILE);
                if (File.Exists(p3)) return p3;

                var p4 = Path.Combine(root, "Reports", REPORT_FILE);
                if (File.Exists(p4)) return p4;

                di = di.Parent;
            }

            // fallback: trả về path mong đợi ở bin
            return p1;
        }
    }
}
