using System;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Forms.Tour;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

using QL_TourDuLich.Modules.Admin.Forms.KhachHang;
using QL_TourDuLich.Modules.Admin.Forms.DangKy;
using QL_TourDuLich.Modules.Admin.Forms.ThanhToan;
using QL_TourDuLich.Modules.Admin.Forms.BaoCao;
using QL_TourDuLich.Modules.Admin.Forms.HuongDanVien;

namespace QL_TourDuLich.Modules.Admin.Forms
{
    public class FrmAdminDashboard : Form
    {
        private readonly AuthResult _user;

        // ✅ root layout: topbar (top) + body (fill)
        private Panel root = new Panel();
        private Panel body = new Panel();

        private Panel sidebar = new Panel();
        private Panel topbar = new Panel();
        private Panel content = new Panel();
        private Panel indicator = new Panel();

        private Label lbUser = new Label();
        private Label lbPageTitle = new Label();

        private IconButton _activeBtn = null;

        private readonly Color MenuHoverBg = Color.FromArgb(245, 250, 255);
        private readonly Color MenuActiveBg = Color.FromArgb(232, 244, 255);
        private readonly Color MenuActiveText = Color.FromArgb(20, 80, 170);
        private readonly Color MenuNormalText = Theme.TextDark;

        public FrmAdminDashboard(AuthResult user)
        {
            _user = user;

            Text = "Admin Dashboard - QL Tour";
            WindowState = FormWindowState.Maximized;
            AutoScaleMode = AutoScaleMode.Font;

            Theme.ApplyForm(this);
            BackColor = Color.White;

            BuildRootLayout();
            BuildTopbar();     // ✅ full width, không bao giờ bị sidebar che
            BuildSidebar();
            BuildContent();

            // mặc định
            OpenForm(new TaiKhoan.FrmTaiKhoan());
            SetActiveMenu(sidebar.Controls["btnTaiKhoan"] as IconButton, "Tài khoản");
        }

        // ================= ROOT =================
        private void BuildRootLayout()
        {
            Controls.Clear();

            root.Dock = DockStyle.Fill;
            root.BackColor = Color.White;
            Controls.Add(root);

            topbar.Dock = DockStyle.Top;
            topbar.Height = 64;

            body.Dock = DockStyle.Fill;
            body.BackColor = Color.White;

            root.Controls.Add(body);
            root.Controls.Add(topbar);

            // ✅ trong body mới có sidebar + content
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 260;

            content.Dock = DockStyle.Fill;

            body.Controls.Add(content);
            body.Controls.Add(sidebar);
        }

        // ================= TOPBAR (đúng như hình 2) =================
        private void BuildTopbar()
        {
            topbar.SuspendLayout();
            topbar.Controls.Clear();

            topbar.BackColor = Theme.PrimaryBlue;
            topbar.Padding = new Padding(14, 10, 14, 10);

            topbar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 0, 0, 0));
                e.Graphics.DrawLine(pen, 0, topbar.Height - 1, topbar.Width, topbar.Height - 1);
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Theme.PrimaryBlue,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420)); // ✅ dư chỗ chip + logout

            topbar.Controls.Add(layout);

            // LEFT: icon + title
            var left = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Theme.PrimaryBlue,
                Padding = new Padding(0, 2, 0, 0),
                Margin = new Padding(0)
            };

            var icon = new IconPictureBox
            {
                IconChar = IconChar.Route,
                IconColor = Color.White,
                IconSize = 24,
                BackColor = Theme.PrimaryBlue,
                Size = new Size(24, 24),
                Margin = new Padding(0, 6, 10, 0)
            };

            var title = new Label
            {
                Text = "QL Tour Du Lịch - Admin",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 0)
            };

            left.Controls.Add(icon);
            left.Controls.Add(title);

            // MID: page title (center)
            var mid = new Panel { Dock = DockStyle.Fill, BackColor = Theme.PrimaryBlue, Margin = new Padding(0) };
            lbPageTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Tài khoản",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            mid.Controls.Add(lbPageTitle);

            // RIGHT: chip + logout (RightToLeft, canh giữa chuẩn)
            var right = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Theme.PrimaryBlue,
                Padding = new Padding(0, 2, 0, 0),
                Margin = new Padding(0)
            };

            var btnLogout = new IconButton
            {
                Text = "Đăng xuất",
                IconChar = IconChar.RightFromBracket,
                IconSize = 18,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Height = 36,
                Width = 130,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Theme.PrimaryBlue,
                IconColor = Theme.PrimaryBlue,
                Margin = new Padding(10, 4, 0, 0)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.MouseEnter += (_, __) => btnLogout.BackColor = Color.FromArgb(240, 240, 240);
            btnLogout.MouseLeave += (_, __) => btnLogout.BackColor = Color.White;
            btnLogout.Click += (_, __) => MessageBox.Show("Logout sau nhé (bạn gắn về form login).");

            var userChip = new Panel
            {
                Height = 36,
                Width = 260,
                BackColor = Color.FromArgb(40, 255, 255, 255),
                Margin = new Padding(0, 4, 0, 0)
            };
            UiFX.Round(userChip, 18);

            var userIco = new IconPictureBox
            {
                IconChar = IconChar.UserCircle,
                IconColor = Color.White,
                IconSize = 18,
                BackColor = userChip.BackColor,
                Size = new Size(18, 18),
                Location = new Point(10, 9)
            };

            lbUser = new Label
            {
                Text = $"{_user.HoTen}  •  {_user.VaiTro}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Width = 220,
                Height = 36,
                Location = new Point(34, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            userChip.Controls.Add(userIco);
            userChip.Controls.Add(lbUser);

            right.Controls.Add(btnLogout);
            right.Controls.Add(userChip);

            layout.Controls.Add(left, 0, 0);
            layout.Controls.Add(mid, 1, 0);
            layout.Controls.Add(right, 2, 0);

            topbar.ResumeLayout(true);
        }

        // ================= SIDEBAR =================
        private void BuildSidebar()
        {
            sidebar.SuspendLayout();
            sidebar.Controls.Clear();

            sidebar.BackColor = Color.White;

            sidebar.Paint += (s, e) =>
            {
                using var pen = new Pen(Theme.Border);
                e.Graphics.DrawLine(pen, sidebar.Width - 1, 0, sidebar.Width - 1, sidebar.Height);
            };

            // header
            var sbHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = Color.White,
                Padding = new Padding(12, 12, 12, 10)
            };

            var sbTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "DANH MỤC",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.PrimaryBlue,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var sbSub = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "Quản lý chức năng hệ thống",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            sbHeader.Controls.Add(sbSub);
            sbHeader.Controls.Add(sbTitle);
            sidebar.Controls.Add(sbHeader);

            indicator = new Panel
            {
                Width = 6,
                Height = 44,
                Left = 0,
                Top = sbHeader.Bottom + 10,
                BackColor = Theme.PrimaryCyan
            };
            sidebar.Controls.Add(indicator);
            indicator.SendToBack();

            int top = sbHeader.Bottom + 10;

            sidebar.Controls.Add(MakeMenuButton("btnTaiKhoan", "Tài khoản", IconChar.UsersCog, top, () =>
            {
                OpenForm(new TaiKhoan.FrmTaiKhoan());
                SetActiveMenu(sidebar.Controls["btnTaiKhoan"] as IconButton, "Tài khoản");
            }));
            top += 50;

            sidebar.Controls.Add(MakeMenuButton("btnTour", "Tour", IconChar.MapMarkedAlt, top, () =>
            {
                OpenForm(new FrmTour());
                SetActiveMenu(sidebar.Controls["btnTour"] as IconButton, "Tour");
            }));
            top += 50;
            sidebar.Controls.Add(MakeMenuButton("btnLoaiTour", "Loại tour", IconChar.Tags, top, () =>
            {
                OpenForm(new QL_TourDuLich.Modules.Admin.Forms.Tour.FrmLoaiTour());
                MoveIndicatorToButton(sidebar.Controls["btnLoaiTour"]);
            }));
            top += 50;
            


            sidebar.Controls.Add(MakeMenuButton("btnKhach", "Khách hàng", IconChar.UserFriends, top, () =>
            {
                OpenForm(new FrmKhachHang());
                SetActiveMenu(sidebar.Controls["btnKhach"] as IconButton, "Khách hàng");
            }));
            top += 50;

            sidebar.Controls.Add(MakeMenuButton("btnKhuyenMai", "Khuyến mãi", IconChar.Tags, top, () =>
            {
                OpenForm(new QL_TourDuLich.Modules.Admin.Forms.KhuyenMai.FrmKhuyenMai());

                MoveIndicatorToButton(sidebar.Controls["btnKhuyenMai"]);
            }));
            top += 50;

            sidebar.Controls.Add(MakeMenuButton("btnDangKy", "Đăng ký tour", IconChar.ClipboardList, top, () =>
            {
                OpenForm(new FrmDangKyTour());
                SetActiveMenu(sidebar.Controls["btnDangKy"] as IconButton, "Đăng ký");
            }));
            top += 50;

            sidebar.Controls.Add(MakeMenuButton("btnThanhToan", "Thanh toán", IconChar.CreditCard, top, () =>
            {
                OpenForm(new FrmThanhToan());
                SetActiveMenu(sidebar.Controls["btnThanhToan"] as IconButton, "Thanh toán");
            }));
            top += 50;

            sidebar.Controls.Add(MakeMenuButton("btnHuongDanVien", "Hướng dẫn viên", IconChar.CreditCard, top, () =>
            {
                OpenForm(new FrmHuongDanVien());
                SetActiveMenu(sidebar.Controls["btnHuongDanVien"] as IconButton, "Hướng dẫn viên");
            }));
            top += 50;

            

            sidebar.Controls.Add(MakeMenuButton("btnBaoCao", "Báo cáo", IconChar.ChartLine, top, () =>
            {
                OpenForm(new FrmThongKeBaoCao());
                SetActiveMenu(sidebar.Controls["btnBaoCao"] as IconButton, "Báo cáo");
            }));

            sidebar.ResumeLayout(true);
        }

        private IconButton MakeMenuButton(string name, string text, IconChar icon, int top, Action onClick)
        {
            var btn = new IconButton
            {
                Name = name,
                Text = "   " + text,
                IconChar = icon,
                IconColor = Theme.PrimaryBlue,
                IconSize = 20,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Left = 6,
                Top = top,
                Width = 248,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = MenuNormalText,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                UseMnemonic = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (_, __) => { if (_activeBtn != btn) btn.BackColor = MenuHoverBg; };
            btn.MouseLeave += (_, __) => { if (_activeBtn != btn) btn.BackColor = Color.White; };

            btn.Click += (_, __) => onClick();
            return btn;
        }

        private void SetActiveMenu(IconButton btn, string pageTitle)
        {
            if (btn == null) return;

            if (_activeBtn != null && !_activeBtn.IsDisposed)
            {
                _activeBtn.BackColor = Color.White;
                _activeBtn.ForeColor = MenuNormalText;
                _activeBtn.IconColor = Theme.PrimaryBlue;
            }

            _activeBtn = btn;

            btn.BackColor = MenuActiveBg;
            btn.ForeColor = MenuActiveText;
            btn.IconColor = MenuActiveText;

            MoveIndicatorToButton(btn);

            if (lbPageTitle != null && !lbPageTitle.IsDisposed)
                lbPageTitle.Text = pageTitle;
        }

        private void MoveIndicatorToButton(Control btn)
        {
            if (btn == null) return;
            indicator.Height = btn.Height;
            indicator.Top = btn.Top;
            indicator.SendToBack();
        }

        // ================= CONTENT =================
        private void BuildContent()
        {
            content.BackColor = Color.White;
            content.Padding = new Padding(18);
        }

        private void OpenForm(Form f)
        {
            content.SuspendLayout();

            foreach (Control c in content.Controls) c.Dispose();
            content.Controls.Clear();

            f.TopLevel = false;
            f.FormBorderStyle = FormBorderStyle.None;
            f.Dock = DockStyle.Fill;

            content.Controls.Add(f);
            f.Show();

            content.ResumeLayout(true);
        }
    }
}
