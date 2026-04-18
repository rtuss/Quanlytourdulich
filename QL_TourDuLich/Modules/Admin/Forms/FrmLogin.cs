using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms
{
    public class FrmLogin : Form
    {
        private Panel leftHero = new Panel();
        private Panel card = new Panel();

        private IconPictureBox iconApp = new IconPictureBox();
        private Label lbTitle = new Label();
        private Label lbSub = new Label();

        private Panel rowUser = new Panel();
        private Panel rowPass = new Panel();

        private IconPictureBox iconUser = new IconPictureBox();
        private IconPictureBox iconPass = new IconPictureBox();
        private Label lbUser = new Label();
        private Label lbPass = new Label();

        private TextBox txtUser = new TextBox();
        private TextBox txtPass = new TextBox();

        private IconButton btnLogin = new IconButton();
        private Label lbStatus = new Label();

        // =========================
        // Cue Banner (placeholder xịn của Windows)
        // =========================
        // =========================
        // Cue Banner (placeholder xịn của Windows)
        // =========================
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private static void SetCue(TextBox tb, string cue)
        {
            // wParam = 0 => ✅ placeholder sẽ biến mất khi textbox được focus (click vào là mất ngay)
            // wParam = 1 => placeholder vẫn hiện khi focus
            if (tb.IsHandleCreated)
                SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)0, cue);
            else
                tb.HandleCreated += (_, __) => SendMessage(tb.Handle, EM_SETCUEBANNER, (IntPtr)0, cue);
        }


        public FrmLogin()
        {
            Text = "Quản lý Tour du lịch";
            Width = 1020;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Theme.ApplyForm(this);
            BackColor = Color.White;
            KeyPreview = true;

            BuildLeftHero();
            BuildLoginCard();

            // Enter để login
            txtUser.KeyDown += OnKeyDownLogin;
            txtPass.KeyDown += OnKeyDownLogin;

            Shown += (_, __) =>
            {
                txtUser.Focus();
                txtUser.Select(0, 0);
            };
        }

        private void OnKeyDownLogin(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                BtnLogin_Click(btnLogin, EventArgs.Empty);
            }
        }

        private void BuildLeftHero()
        {
            leftHero.Dock = DockStyle.Left;
            leftHero.Width = 480;
            leftHero.BackColor = Theme.PrimaryBlue;

            var big = new Label
            {
                Text = "QUẢN LÝ\nTOUR DU LỊCH",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                AutoSize = false,
                Left = 46,
                Top = 90,
                Width = leftHero.Width - 92,
                Height = 120
            };

            var small = new Label
            {
                Text = "Đăng nhập để quản trị hệ thống:\nTour • Khách hàng • Đăng ký • Thanh toán • Báo cáo",
                ForeColor = Color.FromArgb(235, 255, 255, 255),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                AutoSize = false,
                Left = 46,
                Top = 220,
                Width = leftHero.Width - 92,
                Height = 90
            };

            var heroIcon = new IconPictureBox
            {
                IconChar = IconChar.MapMarkedAlt,
                IconColor = Color.FromArgb(60, 255, 255, 255),
                IconSize = 180,
                BackColor = Theme.PrimaryBlue,
                Size = new Size(200, 200),
                Location = new Point(46, 330)
            };

            leftHero.Controls.Clear();
            leftHero.Controls.Add(big);
            leftHero.Controls.Add(small);
            leftHero.Controls.Add(heroIcon);

            if (!Controls.Contains(leftHero)) Controls.Add(leftHero);
        }

        private void BuildLoginCard()
        {
            card.Width = 470;
            card.Height = 430;
            card.BackColor = Color.White;
            card.Left = leftHero.Width + 50;
            card.Top = 80;

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(235, 235, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            UiFX.Round(card, 22);

            iconApp.IconChar = IconChar.UserShield;
            iconApp.IconColor = Theme.PrimaryBlue;
            iconApp.IconSize = 40;
            iconApp.BackColor = Color.White;
            iconApp.Location = new Point(28, 26);
            iconApp.Size = new Size(40, 40);

            lbTitle.Text = "Quản lý Tour du lịch";
            lbTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lbTitle.ForeColor = Theme.TextDark;
            lbTitle.AutoSize = true;
            lbTitle.Location = new Point(80, 26);

            lbSub.Text = "Đăng nhập Admin để vào hệ thống";
            lbSub.Font = new Font("Segoe UI", 10);
            lbSub.ForeColor = Color.Gray;
            lbSub.AutoSize = true;
            lbSub.Location = new Point(82, 58);

            // Row user
            rowUser.SetBounds(34, 110, 392, 26);
            rowUser.BackColor = Color.White;

            iconUser.IconChar = IconChar.User;
            iconUser.IconColor = Theme.PrimaryBlue;
            iconUser.IconSize = 18;
            iconUser.BackColor = Color.White;
            iconUser.Location = new Point(0, 4);
            iconUser.Size = new Size(18, 18);

            lbUser.Text = "Tài khoản";
            lbUser.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbUser.ForeColor = Theme.TextDark;
            lbUser.AutoSize = true;
            lbUser.Location = new Point(26, 3);

            rowUser.Controls.Add(iconUser);
            rowUser.Controls.Add(lbUser);

            // Textbox user
            txtUser.SetBounds(34, 140, 392, 34);
            StyleInput(txtUser);
            SetCue(txtUser, "Nhập tên đăng nhập...");

            // Row pass
            rowPass.SetBounds(34, 190, 392, 26);
            rowPass.BackColor = Color.White;

            iconPass.IconChar = IconChar.Lock;
            iconPass.IconColor = Theme.PrimaryBlue;
            iconPass.IconSize = 18;
            iconPass.BackColor = Color.White;
            iconPass.Location = new Point(0, 4);
            iconPass.Size = new Size(18, 18);

            lbPass.Text = "Mật khẩu";
            lbPass.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbPass.ForeColor = Theme.TextDark;
            lbPass.AutoSize = true;
            lbPass.Location = new Point(26, 3);

            rowPass.Controls.Add(iconPass);
            rowPass.Controls.Add(lbPass);

            // Textbox pass (KHÔNG placeholder fake nữa)
            txtPass.SetBounds(34, 222, 392, 34);
            StyleInput(txtPass);
            txtPass.UseSystemPasswordChar = true;
            SetCue(txtPass, "Nhập mật khẩu...");

            // Button login
            btnLogin.Text = "Đăng nhập";
            btnLogin.IconChar = IconChar.RightToBracket;
            btnLogin.IconColor = Color.White;
            btnLogin.IconSize = 18;
            btnLogin.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnLogin.ImageAlign = ContentAlignment.MiddleLeft;
            btnLogin.Padding = new Padding(14, 0, 0, 0);
            btnLogin.SetBounds(34, 295, 392, 46);
            StylePrimaryIconButton(btnLogin, Theme.PrimaryBlue);

            btnLogin.MouseEnter += (_, __) => btnLogin.BackColor = Darken(Theme.PrimaryBlue, 0.12);
            btnLogin.MouseLeave += (_, __) => btnLogin.BackColor = Theme.PrimaryBlue;

            lbStatus.Text = "";
            lbStatus.AutoSize = false;
            lbStatus.SetBounds(34, 350, 392, 50);
            lbStatus.ForeColor = Color.Gray;

            btnLogin.Click += BtnLogin_Click;

            card.Controls.AddRange(new Control[]
            {
                iconApp, lbTitle, lbSub,
                rowUser, txtUser,
                rowPass, txtPass,
                btnLogin, lbStatus
            });

            Controls.Add(card);
        }

        private void StyleInput(TextBox t)
        {
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 11);
            t.BackColor = Color.White; // luôn thấy khung
        }

        private void StylePrimaryIconButton(IconButton b, Color bg)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = bg;
            b.ForeColor = Color.White;
            b.Cursor = Cursors.Hand;
            b.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var u = (txtUser.Text ?? "").Trim();
            var p = (txtPass.Text ?? "");

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                SetStatus("Vui lòng nhập tài khoản và mật khẩu.", isError: true);
                Shake(card);
                return;
            }

            try
            {
                ToggleBusy(true);

                var res = AuthService.Login(u, p);

                if (res == null)
                {
                    SetStatus("Sai tài khoản hoặc mật khẩu.", isError: true);
                    Shake(card);
                    return;
                }

                if (!string.Equals(res.TrangThai, "HOATDONG", StringComparison.OrdinalIgnoreCase))
                {
                    SetStatus("Tài khoản đang bị khóa.", isError: true);
                    return;
                }

                if (!string.Equals(res.VaiTro, "ADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    SetStatus("Bạn không có quyền Admin.", isError: true);
                    return;
                }

                SetStatus("Đăng nhập thành công!", isError: false);

                Hide();
                var dash = new FrmAdminDashboard(res);
                dash.FormClosed += (_, __) => Close();
                dash.Show();
            }
            catch (Exception ex)
            {
                SetStatus("Lỗi: " + ex.Message, isError: true);
            }
            finally
            {
                ToggleBusy(false);
            }
        }

        private void SetStatus(string msg, bool isError)
        {
            lbStatus.Text = msg;
            lbStatus.ForeColor = isError ? Color.IndianRed : Theme.PrimaryBlue;
        }

        private void ToggleBusy(bool busy)
        {
            btnLogin.Enabled = !busy;
            btnLogin.Cursor = busy ? Cursors.WaitCursor : Cursors.Hand;
            UseWaitCursor = busy;
        }

        private static Color Darken(Color c, double amount01)
        {
            int r = Math.Max(0, (int)(c.R * (1 - amount01)));
            int g = Math.Max(0, (int)(c.G * (1 - amount01)));
            int b = Math.Max(0, (int)(c.B * (1 - amount01)));
            return Color.FromArgb(c.A, r, g, b);
        }

        private void Shake(Control c)
        {
            int original = c.Left;
            int count = 0;

            var t = new Timer { Interval = 15 };
            t.Tick += (_, __) =>
            {
                c.Left = original + ((count % 2 == 0) ? -8 : 8);
                count++;
                if (count >= 10)
                {
                    c.Left = original;
                    t.Stop();
                    t.Dispose();
                }
            };
            t.Start();
        }
    }
}
