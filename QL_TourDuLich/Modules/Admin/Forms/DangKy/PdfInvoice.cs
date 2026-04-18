using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace QL_TourDuLich.Modules.Admin.Forms.DangKy
{
    public static class PdfInvoice
    {
        public static void ExportAndOpen(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return;

            var r = dt.Rows[0];

            string tenTour = Get(r, "TenTour");
            string hoTen = Get(r, "HoTen");
            string trangThai = Get(r, "TrangThai");
            string dangKyId = Get(r, "DangKyID");
            string ngayDangKy = FormatDateTime(Get(r, "NgayDangKy"));
            string soLuongNguoi = Get(r, "SoLuongNguoi");

            decimal donGia = ToDec(Get(r, "DonGia"));
            decimal thanhTien = ToDec(Get(r, "ThanhTien"));

            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "QLTour_Invoices"
            );
            Directory.CreateDirectory(folder);

            var fileName = $"HoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var fullPath = Path.Combine(folder, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html lang='vi'><head><meta charset='utf-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
            sb.AppendLine("<title>Hóa đơn thanh toán</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
:root{
  --text:#0f172a; --muted:#64748b; --line:#e2e8f0; --card:#ffffff; --bg:#f1f5f9;
  --primary:#2563eb; --ok:#16a34a; --warn:#ea580c; --bad:#dc2626;
}
*{ box-sizing:border-box; }
body{ margin:0; font-family:Segoe UI, Arial, sans-serif; background:var(--bg); color:var(--text); }
.page{ max-width:920px; margin:32px auto; padding:0 16px; }
.card{ background:var(--card); border:1px solid var(--line); border-radius:18px; overflow:hidden; box-shadow:0 14px 30px rgba(0,0,0,.08); }
.top{
  padding:20px 24px; display:flex; align-items:flex-start; justify-content:space-between; gap:16px;
  border-bottom:1px solid var(--line);
}
.h1{ margin:0; font-size:22px; letter-spacing:.4px; }
.sub{ margin:6px 0 0; color:var(--muted); font-size:13px; }
.badge{ display:inline-flex; align-items:center; gap:8px; padding:7px 12px; border-radius:999px; font-weight:800; font-size:12px; border:1px solid; }
.badge .dot{ width:8px; height:8px; border-radius:50%; background:currentColor; }
.badge.ok{ color:var(--ok); background:#dcfce7; border-color:#86efac; }
.badge.warn{ color:var(--warn); background:#ffedd5; border-color:#fdba74; }
.badge.bad{ color:var(--bad); background:#fee2e2; border-color:#fca5a5; }
.actions{ display:flex; gap:10px; justify-content:flex-end; }
.btn{
  border:none; background:var(--primary); color:#fff; padding:10px 14px; border-radius:12px;
  font-weight:800; cursor:pointer;
}
.btn:hover{ filter:brightness(.96); }
.content{ padding:18px 24px 22px; }
.grid{ display:grid; grid-template-columns:1fr 1fr; gap:12px 16px; }
.kv{ border:1px solid var(--line); border-radius:14px; padding:12px 12px; background:#fafafa; }
.k{ font-size:12px; color:var(--muted); margin-bottom:6px; }
.v{ font-size:14px; font-weight:700; }
.tableWrap{ margin-top:16px; border:1px solid var(--line); border-radius:14px; overflow:hidden; }
table{ width:100%; border-collapse:collapse; }
th,td{ padding:12px 14px; border-bottom:1px solid var(--line); font-size:14px; }
th{ background:#f8fafc; font-size:13px; color:#0f172a; }
tr:last-child td{ border-bottom:none; }
.money{ text-align:right; white-space:nowrap; }
.total{
  margin-top:16px; border:1px dashed #93c5fd; background:#eff6ff; border-radius:14px;
  padding:14px 16px; display:flex; align-items:center; justify-content:space-between;
}
.total .label{ font-weight:900; color:#0f172a; }
.total .value{ font-size:22px; font-weight:900; color:#0f172a; }
.foot{
  padding:18px 24px 26px; border-top:1px solid var(--line);
  display:flex; justify-content:space-between; align-items:flex-end; gap:16px;
}
.note{ color:var(--muted); font-size:12px; }
.sign{ min-width:260px; text-align:right; }
.line{ margin-top:44px; border-bottom:2px dotted #94a3b8; height:1px; }
@media print{
  body{ background:#fff; }
  .page{ margin:0; max-width:none; padding:0; }
  .card{ border:none; box-shadow:none; border-radius:0; }
  .actions{ display:none; }
}
");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<div class='page'><div class='card'>");

            string badgeClass = BadgeClass(trangThai);

            sb.AppendLine("<div class='top'>");
            sb.AppendLine("<div>");
            sb.AppendLine("<h1 class='h1'>HÓA ĐƠN THANH TOÁN</h1>");
            sb.AppendLine($"<div class='sub'>Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div style='text-align:right'>");
            sb.AppendLine($"<div class='badge {badgeClass}'><span class='dot'></span>Trạng thái: {Esc(trangThai)}</div>");
            sb.AppendLine("<div class='actions' style='margin-top:10px'>");
            sb.AppendLine("<button class='btn' onclick='window.print()'>In / Lưu PDF</button>");
            sb.AppendLine("</div></div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='content'>");

            sb.AppendLine("<div class='grid'>");
            sb.AppendLine(KV("Mã đăng ký", dangKyId));
            sb.AppendLine(KV("Ngày đăng ký", ngayDangKy));
            sb.AppendLine(KV("Tour", tenTour));
            sb.AppendLine(KV("Khách hàng", hoTen));
            sb.AppendLine(KV("Số lượng người", soLuongNguoi));
            sb.AppendLine(KV("Đơn giá", Money(donGia)));
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='tableWrap'><table>");
            sb.AppendLine("<thead><tr><th>Nội dung</th><th class='money'>Giá trị</th></tr></thead>");
            sb.AppendLine("<tbody>");
            sb.AppendLine($"<tr><td>Đơn giá</td><td class='money'>{Money(donGia)}</td></tr>");
            sb.AppendLine($"<tr><td>Số lượng</td><td class='money'>{Esc(soLuongNguoi)}</td></tr>");
            sb.AppendLine($"<tr><td><b>Thành tiền</b></td><td class='money'><b>{Money(thanhTien)}</b></td></tr>");
            sb.AppendLine("</tbody></table></div>");

            sb.AppendLine("<div class='total'>");
            sb.AppendLine("<div class='label'>TỔNG THANH TOÁN</div>");
            sb.AppendLine($"<div class='value'>{Money(thanhTien)}</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>"); // content

            sb.AppendLine("<div class='foot'>");
            sb.AppendLine("<div class='note'>Ghi chú: Hóa đơn được tạo tự động từ hệ thống QL Tour.</div>");
            sb.AppendLine("<div class='sign'>Ký tên<div class='line'></div></div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div></div></body></html>");

            File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);

            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }

        private static string KV(string k, string v)
            => $"<div class='kv'><div class='k'>{Esc(k)}</div><div class='v'>{Esc(v)}</div></div>";

        private static string BadgeClass(string st)
        {
            st = (st ?? "").Trim().ToUpperInvariant();
            if (st == "DA_THANH_TOAN") return "ok";
            if (st == "DANG_KY") return "warn";
            if (st == "HUY") return "bad";
            return "warn";
        }

        private static string Esc(string s)
        {
            s ??= "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string Get(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col)) return "";
            return r[col]?.ToString() ?? "";
        }

        private static decimal ToDec(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;

            if (decimal.TryParse(s, out var v)) return v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v)) return v;

            return 0;
        }

        private static string Money(decimal v) => string.Format("{0:N0} VNĐ", v);

        private static string FormatDateTime(string s)
        {
            if (DateTime.TryParse(s, out var d))
                return d.ToString("dd/MM/yyyy HH:mm");
            return s ?? "";
        }
    }
}
