using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.HuongDanVien
{
    public class FrmPhanCongHDV : Form
    {
        private readonly int _hdvId;
        private readonly string _hdvName;

        private DataGridView grid = new DataGridView();
        private DataTable _dt;
        private int _selectedPhanCongId = -1;

        private ComboBox cbTour = new ComboBox();
        private DateTimePicker dtTu = new DateTimePicker();
        private DateTimePicker dtDen = new DateTimePicker();

        private IconButton btnAdd = new IconButton();
        private IconButton btnUpdate = new IconButton();
        private IconButton btnDelete = new IconButton();
        private IconButton btnClear = new IconButton();
        private IconButton btnClose = new IconButton();

        private Label lbHint = new Label();

        public FrmPhanCongHDV(int hdvId, string hdvName)
        {
            _hdvId = hdvId;
            _hdvName = hdvName ?? "";

            Text = "Phân công Hướng dẫn viên";
            StartPosition = FormStartPosition.CenterParent;

            Width = 1250;
            Height = 740;
            MinimumSize = new Size(1180, 660);

            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            BuildUI();

            Load += (_, __) =>
            {
                LoadTours();
                Reload();
                ClearForm();
            };

            grid.SelectionChanged += (_, __) => PickSelected();
        }

        // ======================= UI =======================
        private void BuildUI()
        {
            var wrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14)
            };
            Controls.Add(wrap);

            var head = BuildHeaderCard();
            head.Dock = DockStyle.Top;

            // 2 cột: trái list / phải form (ổn định hơn splitcontainer, không lỗi splitter)
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var left = BuildCardList();
            left.Dock = DockStyle.Fill;

            var spacer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            var right = BuildCardForm();
            right.Dock = DockStyle.Fill;

            main.Controls.Add(left, 0, 0);
            main.Controls.Add(spacer, 1, 0);
            main.Controls.Add(right, 2, 0);

            wrap.Controls.Add(main);
            wrap.Controls.Add(head);
        }

        private Panel BuildHeaderCard()
        {
            var card = NewCard();
            card.AutoSize = true;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            card.Padding = new Padding(16, 14, 16, 14);

            var title = new Label
            {
                Text = "PHÂN CÔNG HƯỚNG DẪN VIÊN",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 34,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Theme.TextDark
            };

            var sub = new Label
            {
                Text = $"HDV: {_hdvName} (ID: {_hdvId})",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 22,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };

            var hint = new Label
            {
                Text = "Chọn 1 dòng trong danh sách để sửa/xóa. Chọn Tour + khoảng ngày (Từ/Đến) để thêm mới. Không cho phép trùng lịch.",
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Padding = new Padding(0, 6, 0, 0)
            };

            card.Controls.Add(hint);
            card.Controls.Add(sub);
            card.Controls.Add(title);
            return card;
        }

        // ======================= CARD LIST =======================
        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách phân công",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 320,
                TextAlign = ContentAlignment.MiddleLeft
            };

            header.Controls.Add(title);

            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 0)
            };

            grid.Dock = DockStyle.Fill;
            SetupGridStyle(grid);
            gridPanel.Controls.Add(grid);

            card.Controls.Add(gridPanel);
            card.Controls.Add(header);
            return card;
        }

        // ======================= CARD FORM =======================
        private Panel BuildCardForm()
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(14, 14, 14, 18);

            // ---------- Head ----------
            var head = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.White };

            var lbTitle = new Label
            {
                Text = "Thiết lập phân công",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lbSub = new Label
            {
                Text = "Chọn Tour + khoảng ngày (Từ/Đến). Hệ thống sẽ báo lỗi nếu trùng lịch.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 32)
            };

            head.Controls.Add(lbTitle);
            head.Controls.Add(lbSub);

            // ---------- Input Row (Tour - Từ - Đến) ----------
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Color.White,
                Height = 52,
                Padding = new Padding(0, 10, 0, 0),
                Margin = new Padding(0)
            };

            // cột label cố định để KHÔNG wrap chữ
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56)); // Tour
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));  // cbTour
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34)); // Từ
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));  // dtTu
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44)); // Đến
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));  // dtDen

            StyleCombo(cbTour);
            StyleDate(dtTu);
            StyleDate(dtDen);

            cbTour.Dock = DockStyle.Fill;
            dtTu.Dock = DockStyle.Fill;
            dtDen.Dock = DockStyle.Fill;

            // chống cắt chữ
            cbTour.MinimumSize = new Size(0, 34);
            dtTu.MinimumSize = new Size(0, 34);
            dtDen.MinimumSize = new Size(0, 34);

            cbTour.Margin = new Padding(0, 0, 10, 0);
            dtTu.Margin = new Padding(0, 0, 10, 0);
            dtDen.Margin = new Padding(0);

            row.Controls.Add(MakeLabel("Tour", 56), 0, 0);
            row.Controls.Add(cbTour, 1, 0);
            row.Controls.Add(MakeLabel("Từ", 34), 2, 0);
            row.Controls.Add(dtTu, 3, 0);
            row.Controls.Add(MakeLabel("Đến", 44), 4, 0);
            row.Controls.Add(dtDen, 5, 0);

            // ---------- Hint ----------
            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chưa chọn phân công.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = false,
                Height = 30,
                Padding = new Padding(0, 10, 0, 0)
            };

            // ---------- Buttons (CRUD) ----------
            var crudRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true, // ✅ cho xuống dòng khi hẹp (không mất chữ)
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White,
                Padding = new Padding(0, 10, 0, 0),
                Margin = new Padding(0)
            };

            btnAdd = MakeBtn("Thêm", IconChar.Plus, outline: false, w: 150);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 170);
            btnDelete = MakeBtn("Xóa", IconChar.Trash, outline: true, w: 140);
            btnClear = MakeBtn("Làm mới", IconChar.Broom, outline: true, w: 160);

            btnAdd.Margin = new Padding(0, 0, 10, 10);
            btnUpdate.Margin = new Padding(0, 0, 10, 10);
            btnDelete.Margin = new Padding(0, 0, 10, 10);
            btnClear.Margin = new Padding(0, 0, 10, 10);

            btnAdd.Click += (_, __) => DoAdd();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnDelete.Click += (_, __) => DoDelete();
            btnClear.Click += (_, __) => ClearForm();

            crudRow.Controls.Add(btnAdd);
            crudRow.Controls.Add(btnUpdate);
            crudRow.Controls.Add(btnDelete);
            crudRow.Controls.Add(btnClear);

            // ---------- Bottom Close ----------
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.White };

            btnClose = MakeBtn("Đóng", IconChar.Xmark, outline: true, w: 150);
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Location = new Point(bottom.Width - btnClose.Width, 12);
            btnClose.Click += (_, __) => Close();
            bottom.Controls.Add(btnClose);

            bottom.Resize += (_, __) =>
            {
                btnClose.Location = new Point(bottom.ClientSize.Width - btnClose.Width, 12);
            };

            // ---------- Spacer ----------
            var spacer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            card.Controls.Add(spacer);
            card.Controls.Add(bottom);
            card.Controls.Add(crudRow);
            card.Controls.Add(lbHint);
            card.Controls.Add(row);
            card.Controls.Add(head);

            return card;
        }

        // ======================= DATA =======================
        private void LoadTours()
        {
            var dt = PhanCongHDVService.GetToursDangMo();
            cbTour.DataSource = dt;
            cbTour.DisplayMember = "TenTour";
            cbTour.ValueMember = "TourID";
            if (dt != null && dt.Rows.Count > 0) cbTour.SelectedIndex = 0;
        }

        private void Reload()
        {
            _dt = PhanCongHDVService.GetByHdv(_hdvId);
            grid.DataSource = _dt;

            if (grid.Columns["PhanCongID"] != null) grid.Columns["PhanCongID"].HeaderText = "PC ID";
            if (grid.Columns["TourID"] != null) grid.Columns["TourID"].Visible = false;
            if (grid.Columns["HDVID"] != null) grid.Columns["HDVID"].Visible = false;
            if (grid.Columns["TenTour"] != null) grid.Columns["TenTour"].HeaderText = "Tour";
            if (grid.Columns["TuNgay"] != null) grid.Columns["TuNgay"].HeaderText = "Từ ngày";
            if (grid.Columns["DenNgay"] != null) grid.Columns["DenNgay"].HeaderText = "Đến ngày";

            if (grid.Columns["TuNgay"] != null) grid.Columns["TuNgay"].DefaultCellStyle.Format = "dd/MM/yyyy";
            if (grid.Columns["DenNgay"] != null) grid.Columns["DenNgay"].DefaultCellStyle.Format = "dd/MM/yyyy";

            grid.ClearSelection();
            _selectedPhanCongId = -1;
            SetHint("Chưa chọn phân công.", ok: true);
        }

        private void PickSelected()
        {
            if (grid.CurrentRow == null) return;
            if (!(grid.CurrentRow.DataBoundItem is DataRowView rv)) return;

            _selectedPhanCongId = SafeInt(rv["PhanCongID"]);
            int tourId = SafeInt(rv["TourID"]);
            if (tourId > 0) cbTour.SelectedValue = tourId;

            if (DateTime.TryParse(rv["TuNgay"]?.ToString(), out var tu)) dtTu.Value = tu;
            if (DateTime.TryParse(rv["DenNgay"]?.ToString(), out var den)) dtDen.Value = den;

            SetHint($"Đang chọn PC ID = {_selectedPhanCongId}.", ok: true);
        }

        private void DoAdd()
        {
            try
            {
                int tourId = SafeInt(cbTour.SelectedValue);
                if (tourId <= 0) throw new Exception("Vui lòng chọn Tour.");
                PhanCongHDVService.Create(tourId, _hdvId, dtTu.Value, dtDen.Value);

                Reload();
                ClearForm();
                SetHint("Thêm phân công thành công.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoUpdate()
        {
            try
            {
                if (_selectedPhanCongId <= 0) throw new Exception("Chọn 1 phân công để cập nhật.");

                int tourId = SafeInt(cbTour.SelectedValue);
                if (tourId <= 0) throw new Exception("Vui lòng chọn Tour.");

                PhanCongHDVService.Update(_selectedPhanCongId, tourId, _hdvId, dtTu.Value, dtDen.Value);

                Reload();
                SetHint("Cập nhật phân công thành công.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoDelete()
        {
            try
            {
                if (_selectedPhanCongId <= 0) throw new Exception("Chọn 1 phân công để xóa.");

                if (MessageBox.Show("Xóa phân công này?", "Xác nhận",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                PhanCongHDVService.Delete(_selectedPhanCongId);

                Reload();
                ClearForm();
                SetHint("Xóa phân công thành công.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearForm()
        {
            _selectedPhanCongId = -1;
            grid.ClearSelection();
            dtTu.Value = DateTime.Today;
            dtDen.Value = DateTime.Today;
            if (cbTour.Items.Count > 0) cbTour.SelectedIndex = 0;
            SetHint("Chưa chọn phân công.", ok: true);
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

        private void SetupGridStyle(DataGridView g)
        {
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.RowHeadersVisible = false;

            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(240, 240, 240);

            g.BackgroundColor = Color.White;
            g.DefaultCellStyle.BackColor = Color.White;
            g.DefaultCellStyle.ForeColor = Theme.TextDark;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            g.DefaultCellStyle.SelectionForeColor = Theme.TextDark;

            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersDefaultCellStyle.BackColor = Theme.PrimaryBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            g.ColumnHeadersHeight = 46;

            g.RowTemplate.Height = 38;
        }

        private Label MakeLabel(string text, int width = 56)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = width,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private void StyleCombo(ComboBox c)
        {
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Font = new Font("Segoe UI", 10);
            c.BackColor = Color.White;
            c.Height = 34;
        }

        private void StyleDate(DateTimePicker d)
        {
            d.Format = DateTimePickerFormat.Custom;
            d.CustomFormat = "dd/MM/yyyy";
            d.Font = new Font("Segoe UI", 10);
            d.Height = 34;
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
                Height = 42,
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

        private void SetHint(string msg, bool ok)
        {
            lbHint.Text = msg;
            lbHint.ForeColor = ok ? Color.Gray : Color.IndianRed;
        }

        private static Color Darken(Color c, double amount01)
        {
            int r = Math.Max(0, (int)(c.R * (1 - amount01)));
            int g = Math.Max(0, (int)(c.G * (1 - amount01)));
            int b = Math.Max(0, (int)(c.B * (1 - amount01)));
            return Color.FromArgb(c.A, r, g, b);
        }

        private static int SafeInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            int.TryParse(o.ToString(), out var n);
            return n;
        }
    }
}
