using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.KhachHang
{
    public class FrmKhachHang : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        private TextBox txtSearch = new TextBox();

        private TextBox txtHoTen = new TextBox();
        private TextBox txtDienThoai = new TextBox();
        private TextBox txtEmail = new TextBox();
        private TextBox txtDiaChi = new TextBox();
        private ComboBox cbLoaiKhach = new ComboBox();

        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear;
        private Label lbHint = new Label();

        public FrmKhachHang()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(wrap);

            var cardList = BuildCardList();
            cardList.Dock = DockStyle.Top;
            cardList.Height = 290;
            cardList.MinimumSize = new Size(0, 240);

            var cardForm = BuildCardForm();
            cardForm.Dock = DockStyle.Fill;

            wrap.Controls.Add(cardForm);
            wrap.Controls.Add(cardList);

            Load += (_, __) => Reload();
            grid.SelectionChanged += Grid_SelectionChanged;
            txtSearch.TextChanged += (_, __) => ApplySearch();

            btnReload.Click += (_, __) => Reload();
            btnClear.Click += (_, __) => ClearForm();

            btnAdd.Click += (_, __) => DoCreate();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnDelete.Click += (_, __) => DoDelete();
        }

        // ======================= CARD LIST =======================
        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách khách hàng",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 280,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 280,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 6, 0, 0)
            };

            btnReload = MakeBtn("Tải lại", IconChar.ArrowsRotate, outline: true, w: 120);
            btnClear = MakeBtn("Làm mới", IconChar.Broom, outline: true, w: 130);
            btnClear.Margin = new Padding(10, 0, 0, 0);
            btnReload.Margin = new Padding(10, 0, 0, 0);

            actions.Controls.Add(btnClear);
            actions.Controls.Add(btnReload);

            header.Controls.Add(actions);
            header.Controls.Add(title);

            var searchRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.White,
                Padding = new Padding(0, 6, 0, 6)
            };

            var searchWrap = new Panel { Dock = DockStyle.Left, Width = 520, Height = 32, BackColor = Color.White };
            UiFX.Round(searchWrap, 14);
            searchWrap.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(230, 230, 230));
                e.Graphics.DrawRectangle(pen, 0, 0, searchWrap.Width - 1, searchWrap.Height - 1);
            };

            var ico = new IconPictureBox
            {
                IconChar = IconChar.MagnifyingGlass,
                IconColor = Color.Gray,
                IconSize = 16,
                BackColor = Color.White,
                Size = new Size(16, 16),
                Location = new Point(10, 8)
            };

            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.ForeColor = Theme.TextDark;
            txtSearch.Location = new Point(34, 7);
            txtSearch.Width = 480;
            TrySetCue(txtSearch, "Tìm theo họ tên / SĐT / email / địa chỉ / loại khách...");

            searchWrap.Controls.Add(ico);
            searchWrap.Controls.Add(txtSearch);
            searchRow.Controls.Add(searchWrap);

            var gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };
            grid.Dock = DockStyle.Fill;
            SetupGridStyle(grid);
            gridPanel.Controls.Add(grid);

            card.Controls.Add(gridPanel);
            card.Controls.Add(searchRow);
            card.Controls.Add(header);

            return card;
        }

        // ======================= CARD FORM =======================
        private Panel BuildCardForm()
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(14, 14, 14, 18);

            var head = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            var lbTitle = new Label
            {
                Text = "Thông tin khách hàng",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật / xóa khách hàng.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 28)
            };

            head.Controls.Add(lbTitle);
            head.Controls.Add(lbSub);

            var body = new SplitContainer
            {
                Dock = DockStyle.Top,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10,
                BackColor = Color.White,
                Height = 340
            };

            bool _applyingSplit = false;
            void ApplySplitSafe()
            {
                if (_applyingSplit) return;
                if (body.IsDisposed) return;
                if (!body.IsHandleCreated) return;
                if (body.Width <= 0) return;

                try
                {
                    _applyingSplit = true;

                    int width = body.Width;
                    int sw = body.SplitterWidth;

                    int p2Min = Math.Min(420, Math.Max(280, width / 3));
                    int p1Min = Math.Max(300, width - p2Min - sw);

                    if (p1Min + p2Min + sw > width)
                    {
                        p1Min = Math.Max(240, width - p2Min - sw);
                        p2Min = Math.Max(240, width - p1Min - sw);
                    }

                    int desired = (int)(width * 0.68);
                    int minNew = p1Min;
                    int maxNew = width - p2Min - sw;
                    if (maxNew < minNew) maxNew = minNew;

                    body.SplitterDistance = Math.Max(minNew, Math.Min(desired, maxNew));
                    body.Panel1MinSize = p1Min;
                    body.Panel2MinSize = p2Min;
                }
                finally { _applyingSplit = false; }
            }
            body.HandleCreated += (_, __) => body.BeginInvoke((Action)ApplySplitSafe);
            body.SizeChanged += (_, __) => ApplySplitSafe();

            // LEFT
            var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            body.Panel1.Controls.Add(left);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                BackColor = Color.White
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            StyleText(txtHoTen);
            StyleText(txtDienThoai);
            StyleText(txtEmail);

            txtDiaChi.Multiline = true;
            txtDiaChi.Height = 64;
            StyleText(txtDiaChi);

            cbLoaiKhach.DropDownStyle = ComboBoxStyle.DropDownList;
            StyleCombo(cbLoaiKhach);

            AddRow(tlp, 0, "Họ tên", txtHoTen, "Loại khách", cbLoaiKhach);
            AddRow(tlp, 1, "Điện thoại", txtDienThoai, "Email", txtEmail);

            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            var lbDiaChi = MakeLabel("Địa chỉ");
            txtDiaChi.Dock = DockStyle.Fill;
            tlp.Controls.Add(lbDiaChi, 0, 2);
            tlp.Controls.Add(txtDiaChi, 1, 2);
            tlp.SetColumnSpan(txtDiaChi, 3);

            var crudRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0),
                Margin = new Padding(0)
            };

            btnAdd = MakeBtn("Tạo KH", IconChar.Plus, outline: false, w: 130);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 130);
            btnDelete = MakeBtn("Xóa KH", IconChar.Trash, outline: true, w: 130);

            btnAdd.Margin = new Padding(0, 0, 10, 0);
            btnUpdate.Margin = new Padding(0, 0, 10, 0);

            crudRow.Controls.Add(btnAdd);
            crudRow.Controls.Add(btnUpdate);
            crudRow.Controls.Add(btnDelete);

            left.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White });
            left.Controls.Add(crudRow);
            left.Controls.Add(tlp);

            // RIGHT
            var right = NewSoftCard();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(12);

            var prTitle = new Label
            {
                Text = "Chú ý:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 26
            };

            var note = new Label
            {
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Text =
                    "• Không xóa khách hàng đã có đăng ký tour.\n" +
                    "• Họ tên là bắt buộc.\n" +
                    "• Email có thể trống.",
                Padding = new Padding(0, 10, 0, 0)
            };

            right.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent });
            right.Controls.Add(note);
            right.Controls.Add(prTitle);

            body.Panel2.Controls.Add(right);

            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chọn 1 khách hàng trong danh sách để cập nhật / xóa.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = false,
                Height = 26,
                Padding = new Padding(0, 6, 0, 0)
            };

            card.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White });
            card.Controls.Add(lbHint);
            card.Controls.Add(body);
            card.Controls.Add(head);

            return card;
        }

        // ======================= DATA =======================
        private void Reload()
        {
            cbLoaiKhach.DisplayMember = "Text";
            cbLoaiKhach.ValueMember = "Value";
            cbLoaiKhach.DataSource = KhachHangService.GetLoaiKhach();

            _dtAll = KhachHangService.GetAll();
            grid.DataSource = _dtAll;

            _selectedId = -1;
            ClearForm();
        }

        private void ApplySearch()
        {
            if (_dtAll == null) return;

            var q = (txtSearch.Text ?? "").Trim();
            if (q.Length == 0)
            {
                grid.DataSource = _dtAll;
                return;
            }

            var dv = _dtAll.DefaultView;
            var safe = q.Replace("'", "''");
            dv.RowFilter =
                $"HoTen LIKE '%{safe}%' OR DienThoai LIKE '%{safe}%' OR Email LIKE '%{safe}%' OR DiaChi LIKE '%{safe}%' OR LoaiKhach LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void DoCreate()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên.");
                return;
            }

            KhachHangService.Create(
                txtHoTen.Text,
                txtDienThoai.Text,
                txtEmail.Text,
                txtDiaChi.Text,
                cbLoaiKhach.SelectedValue?.ToString() ?? cbLoaiKhach.Text
            );

            Reload();
            SetHint("Tạo khách hàng thành công.", ok: true);
        }

        private void DoUpdate()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 khách hàng để cập nhật."); return; }

            KhachHangService.Update(
                _selectedId,
                txtHoTen.Text,
                txtDienThoai.Text,
                txtEmail.Text,
                txtDiaChi.Text,
                cbLoaiKhach.SelectedValue?.ToString() ?? cbLoaiKhach.Text
            );

            Reload();
            SetHint("Cập nhật khách hàng thành công.", ok: true);
        }

        private void DoDelete()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 khách hàng để xóa."); return; }

            if (KhachHangService.HasDangKy(_selectedId))
            {
                MessageBox.Show("Không thể xóa khách hàng đã có đăng ký tour.");
                return;
            }

            if (MessageBox.Show("Xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            KhachHangService.Delete(_selectedId);
            Reload();
            SetHint("Xóa khách hàng thành công.", ok: true);
        }

        private void ClearForm()
        {
            txtHoTen.Text = "";
            txtDienThoai.Text = "";
            txtEmail.Text = "";
            txtDiaChi.Text = "";
            cbLoaiKhach.SelectedIndex = 0;

            SetHint("Chọn 1 khách hàng trong danh sách để cập nhật / xóa.", ok: true);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow;
            if (row.Cells["KhachHangID"]?.Value == null) return;

            _selectedId = Convert.ToInt32(row.Cells["KhachHangID"].Value);

            txtHoTen.Text = row.Cells["HoTen"].Value?.ToString() ?? "";
            txtDienThoai.Text = row.Cells["DienThoai"].Value?.ToString() ?? "";
            txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";
            txtDiaChi.Text = row.Cells["DiaChi"].Value?.ToString() ?? "";

            var loai = row.Cells["LoaiKhach"].Value?.ToString() ?? "THUONG";
            cbLoaiKhach.SelectedValue = loai;

            SetHint($"Đang chọn KhachHangID = {_selectedId}.", ok: true);
        }

        // ======================= UI HELPERS (y chang TOUR) =======================
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
            g.ColumnHeadersHeight = 40;

            g.RowTemplate.Height = 34;
        }

        private void AddRow(TableLayoutPanel tlp, int r, string l1, Control c1, string l2, Control c2)
        {
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            var lb1 = MakeLabel(l1);
            var lb2 = MakeLabel(l2);

            c1.Dock = DockStyle.Fill;
            c2.Dock = DockStyle.Fill;

            tlp.Controls.Add(lb1, 0, r);
            tlp.Controls.Add(c1, 1, r);
            tlp.Controls.Add(lb2, 2, r);
            tlp.Controls.Add(c2, 3, r);
        }

        private Label MakeLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Padding = new Padding(0, 8, 0, 0)
            };
        }

        private void StyleText(TextBox t)
        {
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 10);
            t.BackColor = Color.White;
        }

        private void StyleCombo(ComboBox c)
        {
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Font = new Font("Segoe UI", 10);
            c.BackColor = Color.White;
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
                Height = 40,
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

        private const int EM_SETCUEBANNER = 0x1501;
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private static void TrySetCue(TextBox tb, string cue)
        {
            try
            {
                if (tb.IsHandleCreated)
                    SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)1, cue);
                else
                    tb.HandleCreated += (_, __) => SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)1, cue);
            }
            catch { }
        }
    }
}
