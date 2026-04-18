using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.TaiKhoan
{
    public class FrmTaiKhoan : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        // Search
        private TextBox txtSearch = new TextBox();

        // Form inputs
        private TextBox txtUser = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtPhone = new TextBox();
        private TextBox txtEmail = new TextBox();
        private ComboBox cbRole = new ComboBox();
        private ComboBox cbStatus = new ComboBox();

        private TextBox txtResetPass = new TextBox();
        private CheckBox chkShowPass = new CheckBox();

        // Buttons
        private IconButton btnAdd = new IconButton();
        private IconButton btnUpdate = new IconButton();
        private IconButton btnReset = new IconButton();
        private IconButton btnReload = new IconButton();
        private IconButton btnClear = new IconButton();

        private Label lbHint = new Label();

        public FrmTaiKhoan()
        {
            // ✅ FIX DPI/Scale: để Font scale đúng, không bị cắt chữ
            AutoScaleMode = AutoScaleMode.Font;

            Theme.ApplyForm(this);

            // ===== Root =====
            var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(root);

            // Top bar
            var top = Theme.CreateTopBar("Quản lý tài khoản");
            top.Dock = DockStyle.Top;

            // Content
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            // ✅ Add content trước, top sau (top luôn nằm trên)
            root.Controls.Add(content);
            root.Controls.Add(top);

            // Build UI blocks
            var cardList = BuildCardList();   // Dock Top
            var cardForm = BuildCardForm();   // Dock Fill

            content.Controls.Add(cardForm);
            content.Controls.Add(cardList);

            // Events
            Load += (_, __) => Reload();
            grid.SelectionChanged += Grid_SelectionChanged;

            btnReload.Click += (_, __) => Reload();
            btnClear.Click += (_, __) => ClearForm();
            btnAdd.Click += (_, __) => DoCreate();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnReset.Click += (_, __) => DoReset();

            txtSearch.TextChanged += (_, __) => ApplySearch();
        }

        // ======================= CARD LIST (GRID) =======================
        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Dock = DockStyle.Top;
            card.Height = 290;
            card.MinimumSize = new Size(0, 240);
            card.Padding = new Padding(14);

            // Header row
            var headerRow = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };
            var title = new Label
            {
                Text = "Danh sách tài khoản",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 10)
            };

            // Right buttons
            var rightBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 280,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 4, 0, 0)
            };

            btnReload = MakeBtn("Tải lại", IconChar.ArrowsRotate, outline: true, w: 120);
            btnClear = MakeBtn("Làm mới", IconChar.Broom, outline: true, w: 130);

            rightBtns.Controls.Add(btnClear);
            rightBtns.Controls.Add(btnReload);

            headerRow.Controls.Add(title);
            headerRow.Controls.Add(rightBtns);

            // Search row
            var searchRow = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White, Padding = new Padding(0, 6, 0, 6) };
            var searchWrap = new Panel { Dock = DockStyle.Left, Width = 420, Height = 32, BackColor = Color.White };
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
            txtSearch.Location = new Point(34, 6);
            txtSearch.Width = 370;
            TrySetCue(txtSearch, "Tìm theo tài khoản / họ tên / SĐT / email...");

            searchWrap.Controls.Add(ico);
            searchWrap.Controls.Add(txtSearch);
            searchRow.Controls.Add(searchWrap);

            // Grid panel
            var gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            gridPanel.Padding = new Padding(0, 6, 0, 0);

            grid.Dock = DockStyle.Fill;
            SetupGridStyle(grid);

            gridPanel.Controls.Add(grid);

            card.Controls.Add(gridPanel);
            card.Controls.Add(searchRow);
            card.Controls.Add(headerRow);

            return card;
        }

        // ======================= CARD FORM =======================
        private Panel BuildCardForm()
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(14, 14, 14, 18);

            var wrap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.White,
                Padding = new Padding(6, 0, 0, 0) // ✅ thêm chút lề trái, tránh dính/cắt chữ
            };

            wrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // header
            wrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // form
            wrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // buttons
            wrap.RowStyles.Add(new RowStyle(SizeType.AutoSize));      // hint
            wrap.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // spacer

            // Header
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };

            var title = new Label
            {
                Text = "Thông tin tài khoản",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var sub = new Label
            {
                Text = "Tạo nhân viên, phân quyền, cập nhật trạng thái và reset mật khẩu.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 26)
            };

            header.Controls.Add(title);
            header.Controls.Add(sub);

            // Form table
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 4,
                BackColor = Color.White
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            cbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cbRole.Items.Clear();
            cbRole.Items.AddRange(new object[] { "ADMIN", "STAFF" });

            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.Items.Clear();
            cbStatus.Items.AddRange(new object[] { "HOATDONG", "KHOA" });

            StyleText(txtUser);
            StyleText(txtName);
            StyleText(txtPhone);
            StyleText(txtEmail);
            StyleCombo(cbRole);
            StyleCombo(cbStatus);

            txtResetPass.PasswordChar = '●';
            StyleText(txtResetPass);

            AddRow(tlp, 0, "Tài khoản", txtUser, "Họ tên", txtName);
            AddRow(tlp, 1, "Điện thoại", txtPhone, "Email", txtEmail);
            AddRow(tlp, 2, "Vai trò", cbRole, "Trạng thái", cbStatus);

            // Reset password row (span)
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            var lbPass = MakeLabel("Mật khẩu reset");
            var passWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };

            chkShowPass.Text = "Hiện";
            chkShowPass.AutoSize = true;
            chkShowPass.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            chkShowPass.ForeColor = Theme.PrimaryBlue;
            chkShowPass.Dock = DockStyle.Right;
            chkShowPass.Width = 70;
            chkShowPass.CheckedChanged += (_, __) =>
            {
                txtResetPass.PasswordChar = chkShowPass.Checked ? '\0' : '●';
            };

            txtResetPass.Dock = DockStyle.Fill;
            passWrap.Controls.Add(txtResetPass);
            passWrap.Controls.Add(chkShowPass);

            tlp.Controls.Add(lbPass, 0, 3);
            tlp.Controls.Add(passWrap, 1, 3);
            tlp.SetColumnSpan(passWrap, 3);

            // Buttons
            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.White,
                Padding = new Padding(2, 8, 0, 0), // ✅ lề trái nhỏ để không “cắt chữ”
                Margin = new Padding(0, 6, 0, 0)
            };

            btnAdd = MakeBtn("Tạo nhân viên", IconChar.UserPlus, outline: false, w: 160);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 120);
            btnReset = MakeBtn("Reset mật khẩu", IconChar.Key, outline: true, w: 160);

            btnRow.Controls.Add(btnAdd);
            btnRow.Controls.Add(btnUpdate);
            btnRow.Controls.Add(btnReset);

            // Hint
            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chọn 1 tài khoản trong danh sách để cập nhật / reset.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Padding = new Padding(2, 6, 0, 0), // ✅ thêm 2px trái
                AutoEllipsis = true
            };

            wrap.Controls.Add(header, 0, 0);
            wrap.Controls.Add(tlp, 0, 1);
            wrap.Controls.Add(btnRow, 0, 2);
            wrap.Controls.Add(lbHint, 0, 3);
            wrap.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White }, 0, 4);

            card.Controls.Add(wrap);
            return card;
        }

        // ======================= DATA OPS =======================
        private void Reload()
        {
            _dtAll = TaiKhoanService.GetAll();
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
            dv.RowFilter = $"TenDangNhap LIKE '%{safe}%' OR HoTen LIKE '%{safe}%' OR DienThoai LIKE '%{safe}%' OR Email LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void DoCreate()
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Vui lòng nhập Tài khoản và Họ tên.");
                return;
            }

            var role = string.IsNullOrWhiteSpace(cbRole.Text) ? "STAFF" : cbRole.Text;
            var status = string.IsNullOrWhiteSpace(cbStatus.Text) ? "HOATDONG" : cbStatus.Text;

            var pass = string.IsNullOrWhiteSpace(txtResetPass.Text) ? "14122003" : txtResetPass.Text;

            TaiKhoanService.CreateStaff(
                txtUser.Text.Trim(),
                pass,
                txtName.Text.Trim(),
                role,
                status,
                txtPhone.Text.Trim(),
                txtEmail.Text.Trim()
            );

            Reload();
            SetHint("Tạo tài khoản thành công.", ok: true);
        }

        private void DoUpdate()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tài khoản để cập nhật."); return; }

            TaiKhoanService.Update(
                _selectedId,
                txtName.Text.Trim(),
                cbRole.Text,
                cbStatus.Text,
                txtPhone.Text.Trim(),
                txtEmail.Text.Trim()
            );

            Reload();
            SetHint("Cập nhật thành công.", ok: true);
        }

        private void DoReset()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tài khoản để reset."); return; }

            var pass = string.IsNullOrWhiteSpace(txtResetPass.Text) ? "14122003" : txtResetPass.Text;
            TaiKhoanService.ResetPassword(_selectedId, pass);

            SetHint("Reset mật khẩu xong.", ok: true);
            MessageBox.Show("Reset mật khẩu xong.");
        }

        private void ClearForm()
        {
            txtUser.Text = "";
            txtName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            cbRole.SelectedIndex = -1;
            cbStatus.SelectedIndex = -1;
            txtResetPass.Text = "";
            chkShowPass.Checked = false;

            txtUser.ReadOnly = false;
            txtUser.BackColor = Color.White;

            SetHint("Chọn 1 tài khoản trong danh sách để cập nhật / reset.", ok: true);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow;
            if (row.Cells["TaiKhoanID"]?.Value == null) return;

            _selectedId = Convert.ToInt32(row.Cells["TaiKhoanID"].Value);

            txtUser.Text = row.Cells["TenDangNhap"].Value?.ToString();
            txtName.Text = row.Cells["HoTen"].Value?.ToString();
            cbRole.Text = row.Cells["VaiTro"].Value?.ToString();
            cbStatus.Text = row.Cells["TrangThai"].Value?.ToString();
            txtPhone.Text = row.Cells["DienThoai"].Value?.ToString();
            txtEmail.Text = row.Cells["Email"].Value?.ToString();

            txtUser.ReadOnly = true;
            txtUser.BackColor = Color.FromArgb(248, 248, 248);

            SetHint($"Đang chọn ID = {_selectedId}. Có thể cập nhật hoặc reset mật khẩu.", ok: true);
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

            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

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

        // Cue banner (optional)
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
