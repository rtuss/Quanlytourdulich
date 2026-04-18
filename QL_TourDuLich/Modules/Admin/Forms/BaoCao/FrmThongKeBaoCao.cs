using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.BaoCao
{
    public class FrmThongKeBaoCao : Form
    {
        // Filters
        private readonly ComboBox cbKy = new ComboBox();
        private readonly DateTimePicker dtFrom = new DateTimePicker();
        private readonly DateTimePicker dtTo = new DateTimePicker();
        private IconButton btnApply = new IconButton();
        private IconButton btnThisMonth = new IconButton();
        private Label lbHint = new Label();

        // KPI
        private readonly Label lbKpiDangKy = new Label();
        private readonly Label lbKpiHuy = new Label();
        private readonly Label lbKpiDoanhThu = new Label();
        private readonly Label lbKpiCongNo = new Label();

        // Charts
        private readonly Chart chDangKy = new Chart();
        private readonly Chart chDoanhThu = new Chart();
        private readonly Chart chTrangThai = new Chart();
        private readonly Chart chTopTour = new Chart();

        // One-shot guard
        private bool _chartInitialized = false;

        public FrmThongKeBaoCao()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14) };
            Controls.Add(wrap);

            var head = BuildHeader();
            var kpi = BuildKpiRow();
            var charts = BuildCharts();

            wrap.Controls.Add(charts);
            wrap.Controls.Add(kpi);
            wrap.Controls.Add(head);

            Load += (_, __) => InitUIOnly();
            Shown += (_, __) => DeferredInitAndLoad(); // ✅ fix Chart Height=0

            cbKy.SelectedIndexChanged += (_, __) => UpdateHint();
        }

        // ======================= INIT =======================
        private void InitUIOnly()
        {
            cbKy.DropDownStyle = ComboBoxStyle.DropDownList;
            cbKy.Items.Clear();
            cbKy.Items.AddRange(new object[] { "Tuần (WEEK)", "Tháng (MONTH)", "Năm (YEAR)" });
            cbKy.SelectedIndex = 1;

            dtFrom.Format = DateTimePickerFormat.Custom;
            dtFrom.CustomFormat = "dd/MM/yyyy";
            dtTo.Format = DateTimePickerFormat.Custom;
            dtTo.CustomFormat = "dd/MM/yyyy";

            SetThisMonth();
            UpdateHint();

            // hook events after created
            btnApply.Click += (_, __) => ReloadAllSafe();
            btnThisMonth.Click += (_, __) => { SetThisMonth(); ReloadAllSafe(); };

            // Prepare charts to avoid designer/measure crash
            PrepareChartSafe(chDangKy);
            PrepareChartSafe(chDoanhThu);
            PrepareChartSafe(chTrangThai);
            PrepareChartSafe(chTopTour);
        }

        private void DeferredInitAndLoad()
        {
            if (_chartInitialized) return;
            _chartInitialized = true;

            // ✅ delay 1 tick để form layout xong (Height/Width > 0)
            BeginInvoke((Action)(() =>
            {
                try
                {
                    SetupCharts();
                    ReloadAllSafe();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khởi tạo báo cáo: " + ex.Message);
                }
            }));
        }

        private void ReloadAllSafe()
        {
            try { ReloadAll(); }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê: " + ex.Message);
            }
        }

        // ======================= UI =======================
        private Panel BuildHeader()
        {
            var card = NewCard();
            card.Dock = DockStyle.Top;
            card.Height = 120;
            card.MinimumSize = new Size(0, 120);
            card.Padding = new Padding(14);

            var title = new Label
            {
                Text = "Thống kê & Báo cáo",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 30
            };

            // ====== 1 hàng filter + 1 hàng hint (ổn định, thẳng hàng) ======
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 9,
                RowCount = 2,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Cols: [lbKy][cbKy][sp][lbFrom][dtFrom][lbTo][dtTo][hint][buttons]
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));   // Kỳ
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));  // cbKy
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));   // spacer
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));   // Từ
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));  // dtFrom
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));   // Đến
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));  // dtTo
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // hint
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270));  // buttons

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); // row controls
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // row hint space

            // ====== controls style (không bị cắt chữ) ======
            StyleCombo(cbKy);
            StyleDate(dtFrom);
            StyleDate(dtTo);

            cbKy.Dock = DockStyle.Fill;
            dtFrom.Dock = DockStyle.Fill;
            dtTo.Dock = DockStyle.Fill;

            // Margin để không dính nhau
            cbKy.Margin = new Padding(0, 6, 0, 0);
            dtFrom.Margin = new Padding(0, 6, 10, 0);
            dtTo.Margin = new Padding(0, 6, 0, 0);

            // labels cùng hàng
            var lbKy = MakeLabelInline("Kỳ");
            var lbFrom = MakeLabelInline("Từ");
            var lbTo = MakeLabelInline("Đến");

            // Buttons
            btnApply = MakeBtn("Áp dụng", IconChar.Filter, outline: false, w: 120);
            btnThisMonth = MakeBtn("Tháng này", IconChar.CalendarDays, outline: true, w: 120);
            btnApply.Margin = new Padding(0, 4, 10, 0);

            var btnWrap = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 2, 0, 0),
                Margin = new Padding(0)
            };
            btnWrap.Controls.Add(btnThisMonth);
            btnWrap.Controls.Add(btnApply);

            // Hint
            lbHint = new Label
            {
                Text = "Chọn kỳ và khoảng thời gian để xem tổng hợp.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(0, 0, 0, 0)
            };

            // ====== add row 0 ======
            grid.Controls.Add(lbKy, 0, 0);
            grid.Controls.Add(cbKy, 1, 0);

            grid.Controls.Add(lbFrom, 3, 0);
            grid.Controls.Add(dtFrom, 4, 0);

            grid.Controls.Add(lbTo, 5, 0);
            grid.Controls.Add(dtTo, 6, 0);

            grid.Controls.Add(lbHint, 7, 0);
            grid.Controls.Add(btnWrap, 8, 0);

            card.Controls.Add(grid);
            card.Controls.Add(title);
            return card;
        }

        private Label MakeLabelInline(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Padding = new Padding(0, 6, 0, 0),
                AutoSize = false
            };
        }

        private void StyleDate(DateTimePicker d)
        {
            d.Format = DateTimePickerFormat.Custom;
            d.CustomFormat = "dd/MM/yyyy";
            d.Font = new Font("Segoe UI", 10);
            d.Height = 34;
        }


        private Label MakeLabelTop(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Padding = new Padding(0, 0, 0, 2),
                AutoSize = false
            };
        }

        private Control BuildKpiRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 96,
                MinimumSize = new Size(0, 96),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.White,
                Margin = new Padding(0, 12, 0, 12)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            row.Controls.Add(NewKpiCard("Tổng đăng ký", lbKpiDangKy), 0, 0);
            row.Controls.Add(NewKpiCard("Đăng ký hủy", lbKpiHuy), 1, 0);
            row.Controls.Add(NewKpiCard("Doanh thu", lbKpiDoanhThu), 2, 0);
            row.Controls.Add(NewKpiCard("Công nợ", lbKpiCongNo), 3, 0);

            var wrap = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Color.White };
            wrap.Controls.Add(row);
            return wrap;
        }

        private Control BuildCharts()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.White
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            // ✅ add to grid
            grid.Controls.Add(NewChartCard("Đăng ký theo thời gian", chDangKy, rightCol: false, topRow: true), 0, 0);
            grid.Controls.Add(NewChartCard("Tỷ lệ trạng thái đăng ký", chTrangThai, rightCol: true, topRow: true), 1, 0);
            grid.Controls.Add(NewChartCard("Doanh thu theo thời gian", chDoanhThu, rightCol: false, topRow: false), 0, 1);
            grid.Controls.Add(NewChartCard("Top tour theo doanh thu", chTopTour, rightCol: true, topRow: false), 1, 1);

            grid.MinimumSize = new Size(0, 420);
            return grid;
        }

        private Panel NewChartCard(string title, Chart chart, bool rightCol, bool topRow)
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(12);

            // ✅ margin: phải/cột phải khác nhau, hàng trên/hàng dưới khác nhau
            int mr = rightCol ? 0 : 12;
            int mb = topRow ? 12 : 0;
            card.Margin = new Padding(0, 0, mr, mb);

            card.MinimumSize = new Size(320, 240);

            var lb = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Theme.TextDark
            };

            chart.Dock = DockStyle.Fill;
            chart.MinimumSize = new Size(240, 160);

            card.Controls.Add(chart);
            card.Controls.Add(lb);
            return card;
        }

        private Panel NewKpiCard(string title, Label valueLabel)
        {
            var card = NewSoftCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(12);
            card.Margin = new Padding(0, 0, 12, 0);
            card.MinimumSize = new Size(220, 86);

            var lbT = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 18,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            valueLabel.Text = "0";
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            valueLabel.ForeColor = Theme.PrimaryBlue;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(valueLabel);
            card.Controls.Add(lbT);
            return card;
        }

        // ======================= CHART SAFE =======================
        private void PrepareChartSafe(Chart c)
        {
            c.MinimumSize = new Size(220, 160);
            c.Margin = new Padding(0);

            // Trick: Chart đôi khi crash nếu Dock Fill từ đầu
            c.Dock = DockStyle.None;
            c.Dock = DockStyle.Fill;

            if (c.ChartAreas.Count == 0)
                c.ChartAreas.Add(new ChartArea("A"));

            c.BackColor = Color.White;
        }

        private void SetupCharts()
        {
            SetupTimeChart(chDangKy);
            SetupTimeChart(chDoanhThu);
            SetupPieChart(chTrangThai);
            SetupBarChart(chTopTour);
        }

        private void SetupTimeChart(Chart c)
        {
            c.Series.Clear();
            c.ChartAreas.Clear();
            c.Legends.Clear();

            var area = new ChartArea("A");
            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
            area.AxisX.Interval = 1;
            c.ChartAreas.Add(area);

            c.Legends.Add(new Legend { Docking = Docking.Top, Font = new Font("Segoe UI", 8) });
        }

        private void SetupPieChart(Chart c)
        {
            c.Series.Clear();
            c.ChartAreas.Clear();
            c.Legends.Clear();

            c.ChartAreas.Add(new ChartArea("A") { BackColor = Color.White });
            c.Legends.Add(new Legend { Docking = Docking.Right, Font = new Font("Segoe UI", 8) });
        }

        private void SetupBarChart(Chart c)
        {
            c.Series.Clear();
            c.ChartAreas.Clear();
            c.Legends.Clear();

            var area = new ChartArea("A");
            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8);
            area.AxisX.Interval = 1;
            c.ChartAreas.Add(area);

            c.Legends.Add(new Legend { Docking = Docking.Top, Font = new Font("Segoe UI", 8) });
        }

        // ======================= DATA LOAD =======================
        private void ReloadAll()
        {
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date.AddDays(1);

            if (to <= from) throw new Exception("Khoảng ngày không hợp lệ.");

            string gran = GetGranularity();

            // KPI
            DataTable kpi = ThongKeService.GetKpi(from, to);
            if (kpi != null && kpi.Rows.Count > 0)
            {
                var r = kpi.Rows[0];
                lbKpiDangKy.Text = SafeInt(r["TongDangKy"]).ToString("N0");
                lbKpiHuy.Text = SafeInt(r["TongHuy"]).ToString("N0");
                lbKpiDoanhThu.Text = SafeDec(r["DoanhThu"]).ToString("N0") + " VNĐ";
                lbKpiCongNo.Text = SafeDec(r["CongNo"]).ToString("N0") + " VNĐ";
            }
            else
            {
                lbKpiDangKy.Text = "0";
                lbKpiHuy.Text = "0";
                lbKpiDoanhThu.Text = "0 VNĐ";
                lbKpiCongNo.Text = "0 VNĐ";
            }

            // Series
            BindSeries(chDangKy, "Đăng ký", ThongKeService.GetDangKySeries(gran, from, to), ChartTypeForTime());
            BindSeries(chDoanhThu, "Doanh thu", ThongKeService.GetDoanhThuSeries(gran, from, to), ChartTypeForTime());
            BindPie(chTrangThai, ThongKeService.GetTrangThaiDangKy(from, to));
            BindTopTour(chTopTour, ThongKeService.GetTopTourDoanhThu(from, to, 5));

            UpdateHint();
        }

        private SeriesChartType ChartTypeForTime()
        {
            var g = GetGranularity();
            return (g == "WEEK") ? SeriesChartType.Column : SeriesChartType.Line;
        }

        private void BindSeries(Chart chart, string name, DataTable dt, SeriesChartType type)
        {
            chart.Series.Clear();

            var s = new Series(name)
            {
                ChartType = type,
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 5
            };

            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    var label = (r["Label"]?.ToString() ?? "").Trim();
                    var val = SafeDec(r["Value"]);
                    s.Points.AddXY(label, val);
                }
            }

            if (s.Points.Count == 0) s.Points.AddXY("N/A", 0);

            chart.Series.Add(s);
        }

        private void BindPie(Chart chart, DataTable dt)
        {
            chart.Series.Clear();

            var s = new Series("Trạng thái")
            {
                ChartType = SeriesChartType.Pie
            };
            s["PieLabelStyle"] = "Outside";
            s.Label = "#PERCENT{P0}";
            s.LegendText = "#VALX";

            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    var st = (r["TrangThai"]?.ToString() ?? "").Trim();
                    var n = SafeInt(r["SoLuong"]);
                    s.Points.AddXY(string.IsNullOrWhiteSpace(st) ? "N/A" : st, n);
                }
            }

            if (s.Points.Count == 0) s.Points.AddXY("N/A", 1);

            chart.Series.Add(s);
        }

        private void BindTopTour(Chart chart, DataTable dt)
        {
            chart.Series.Clear();

            var s = new Series("Doanh thu")
            {
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true,
                LabelFormat = "N0",
                Font = new Font("Segoe UI", 8)
            };

            if (dt != null)
            {
                foreach (DataRow r in dt.Rows)
                {
                    var ten = (r["TenTour"]?.ToString() ?? "").Trim();
                    var val = SafeDec(r["DoanhThu"]);
                    s.Points.AddXY(string.IsNullOrWhiteSpace(ten) ? "N/A" : ten, val);
                }
            }

            if (s.Points.Count == 0) s.Points.AddXY("N/A", 0);

            chart.Series.Add(s);
        }

        // ======================= FILTERS =======================
        private string GetGranularity()
        {
            if (cbKy.SelectedIndex == 0) return "WEEK";
            if (cbKy.SelectedIndex == 2) return "YEAR";
            return "MONTH";
        }

        private void SetThisMonth()
        {
            var now = DateTime.Today;
            var first = new DateTime(now.Year, now.Month, 1);
            dtFrom.Value = first;
            dtTo.Value = first.AddMonths(1).AddDays(-1);
        }

        private void UpdateHint()
        {
            var g = GetGranularity();
            lbHint.Text = (g == "WEEK") ? "Đang xem theo Tuần." :
                         (g == "MONTH") ? "Đang xem theo Tháng." :
                         "Đang xem theo Năm.";
        }

        // ======================= SAFE PARSE =======================
        private static int SafeInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            int.TryParse(o.ToString(), out var n);
            return n;
        }

        private static decimal SafeDec(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            decimal.TryParse(o.ToString(), out var n);
            return n;
        }

        // ======================= UI HELPERS =======================
        private Panel NewCard()
        {
            var card = new Panel { BackColor = Color.White };
            UiFX.Round(card, 18);
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(235, 235, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private Panel NewSoftCard()
        {
            var card = new Panel { BackColor = Color.FromArgb(250, 250, 250) };
            UiFX.Round(card, 14);
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(235, 235, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private void StyleCombo(ComboBox c)
        {
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Font = new Font("Segoe UI", 10);
            c.BackColor = Color.White;
            c.Dock = DockStyle.Fill;
        }
        private Control CenterWrap(Control c)
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            c.Anchor = AnchorStyles.Left;      // giữ bên trái
            c.Location = new Point(0, 0);

            host.Resize += (_, __) =>
            {
                // căn giữa theo chiều dọc
                c.Left = 0;
                c.Top = Math.Max(0, (host.Height - c.Height) / 2);
                // nếu muốn control chiếm hết chiều ngang cell
                if (c is DateTimePicker || c is ComboBox)
                    c.Width = host.Width;  // fill theo cell
            };

            host.Controls.Add(c);
            return host;
        }


        private IconButton MakeBtn(string text, IconChar icon, bool outline, int w)
        {
            var b = new IconButton
            {
                Text = text,
                IconChar = icon,
                IconSize = 18,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Height = 36,
                Width = w,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = false
            };

            b.FlatAppearance.BorderSize = outline ? 1 : 0;

            if (outline)
            {
                b.BackColor = Color.White;
                b.ForeColor = Theme.PrimaryBlue;
                b.IconColor = Theme.PrimaryBlue;
                b.FlatAppearance.BorderColor = Theme.PrimaryBlue;
            }
            else
            {
                b.BackColor = Theme.PrimaryBlue;
                b.ForeColor = Color.White;
                b.IconColor = Color.White;
            }

            b.MouseEnter += (_, __) =>
            {
                if (outline) b.BackColor = Color.FromArgb(245, 249, 255);
                else b.BackColor = Darken(Theme.PrimaryBlue, 0.10);
            };
            b.MouseLeave += (_, __) =>
            {
                if (outline) b.BackColor = Color.White;
                else b.BackColor = Theme.PrimaryBlue;
            };

            return b;
        }

        private static Color Darken(Color c, double amount01)
        {
            int r = Math.Max(0, (int)(c.R * (1 - amount01)));
            int g = Math.Max(0, (int)(c.G * (1 - amount01)));
            int b = Math.Max(0, (int)(c.B * (1 - amount01)));
            return Color.FromArgb(c.A, r, g, b);
        }
    }
}
