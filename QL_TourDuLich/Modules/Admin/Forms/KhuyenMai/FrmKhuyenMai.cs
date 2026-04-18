using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.KhuyenMai
{
    public class FrmKhuyenMai : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        private TextBox txtSearch = new TextBox();

        private TextBox txtTen = new TextBox();
        private NumericUpDown numPhanTram = new NumericUpDown();
        private DateTimePicker dtTuNgay = new DateTimePicker();
        private DateTimePicker dtDenNgay = new DateTimePicker();
        private ComboBox cbTrangThai = new ComboBox();

        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear;
        private Label lbHint = new Label();

        public FrmKhuyenMai()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            BuildUI();

            Load += (_, __) => Reload();
            txtSearch.TextChanged += (_, __) => ApplySearch();
            grid.SelectionChanged += Grid_SelectionChanged;

            btnReload.Click += (_, __) => Reload();
            btnClear.Click += (_, __) => ClearForm();

            btnAdd.Click += (_, __) => DoCreate();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnDelete.Click += (_, __) => DoDelete();
        }

        private void BuildUI()
        {
            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(16) };
            Controls.Add(wrap);

            // ===== CARD LIST =====
            var cardList = NewCard();
            cardList.Dock = DockStyle.Top;
            cardList.Height = 300;
            cardList.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách khuyến mãi",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                Width = 260,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 320,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
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

            var searchRow = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(0, 6, 0, 6) };

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
            txtSearch.Location = new Point(34, 7);
            txtSearch.Width = 370;
            TrySetCue(txtSearch, "Tìm theo tên khuyến mãi / trạng thái...");

            searchWrap.Controls.Add(ico);
            searchWrap.Controls.Add(txtSearch);
            searchRow.Controls.Add(searchWrap);

            var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 0, 0) };
            grid.Dock = DockStyle.Fill;
            SetupGridStyle(grid);
            gridPanel.Controls.Add(grid);

            cardList.Controls.Add(gridPanel);
            cardList.Controls.Add(searchRow);
            cardList.Controls.Add(header);

            // ===== CARD FORM =====
            var cardForm = NewCard();
            cardForm.Dock = DockStyle.Fill;
            cardForm.Padding = new Padding(14);

            var lbFormTitle = new Label
            {
                Text = "Thông tin khuyến mãi",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 28
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật / xóa khuyến mãi.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 20
            };

            // Form grid
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                BackColor = Color.White
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            StyleText(txtTen);

            numPhanTram.Minimum = 0;
            numPhanTram.Maximum = 100;
            numPhanTram.Value = 10;
            numPhanTram.Font = new Font("Segoe UI", 10);

            dtTuNgay.Format = DateTimePickerFormat.Custom;
            dtTuNgay.CustomFormat = "dd/MM/yyyy";
            dtTuNgay.Font = new Font("Segoe UI", 10);

            dtDenNgay.Format = DateTimePickerFormat.Custom;
            dtDenNgay.CustomFormat = "dd/MM/yyyy";
            dtDenNgay.Font = new Font("Segoe UI", 10);

            cbTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTrangThai.Items.Clear();
            cbTrangThai.Items.AddRange(new object[] { "HOAT_DONG", "TAM_DUNG", "HET_HAN" });
            cbTrangThai.SelectedIndex = 0;
            StyleCombo(cbTrangThai);

            AddRow(tlp, 0, "Tên khuyến mãi", txtTen, "Phần trăm giảm", numPhanTram);
            AddRow(tlp, 1, "Từ ngày", dtTuNgay, "Đến ngày", dtDenNgay);
            AddRow(tlp, 2, "Trạng thái", cbTrangThai, "", new Label());

            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            btnAdd = MakeBtn("Thêm", IconChar.Plus, outline: false, w: 120);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 120);
            btnDelete = MakeBtn("Xóa", IconChar.Trash, outline: true, w: 120);

            btnAdd.Margin = new Padding(0, 0, 10, 0);
            btnUpdate.Margin = new Padding(0, 0, 10, 0);

            btnRow.Controls.Add(btnAdd);
            btnRow.Controls.Add(btnUpdate);
            btnRow.Controls.Add(btnDelete);

            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(0, 8, 0, 0),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Text = "Chọn 1 dòng để sửa / xóa."
            };

            cardForm.Controls.Add(new Panel { Dock = DockStyle.Fill });
            cardForm.Controls.Add(lbHint);
            cardForm.Controls.Add(btnRow);
            cardForm.Controls.Add(tlp);
            cardForm.Controls.Add(lbSub);
            cardForm.Controls.Add(lbFormTitle);

            wrap.Controls.Add(cardForm);
            wrap.Controls.Add(new Panel { Height = 14, Dock = DockStyle.Top });
            wrap.Controls.Add(cardList);
        }

        private void Reload()
        {
            _dtAll = KhuyenMaiService.GetAll();
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
            dv.RowFilter = $"TenKhuyenMai LIKE '%{safe}%' OR TrangThai LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            if (grid.CurrentRow.Cells["KhuyenMaiID"]?.Value == null) return;

            _selectedId = Convert.ToInt32(grid.CurrentRow.Cells["KhuyenMaiID"].Value);

            txtTen.Text = grid.CurrentRow.Cells["TenKhuyenMai"]?.Value?.ToString() ?? "";

            if (grid.CurrentRow.Cells["PhanTramGiam"]?.Value != null)
                numPhanTram.Value = Convert.ToDecimal(grid.CurrentRow.Cells["PhanTramGiam"].Value);

            if (grid.CurrentRow.Cells["TuNgay"]?.Value != null && DateTime.TryParse(grid.CurrentRow.Cells["TuNgay"].Value.ToString(), out var d1))
                dtTuNgay.Value = d1;

            if (grid.CurrentRow.Cells["DenNgay"]?.Value != null && DateTime.TryParse(grid.CurrentRow.Cells["DenNgay"].Value.ToString(), out var d2))
                dtDenNgay.Value = d2;

            cbTrangThai.Text = grid.CurrentRow.Cells["TrangThai"]?.Value?.ToString() ?? "HOAT_DONG";

            SetHint($"Đang chọn KhuyenMaiID = {_selectedId}.", ok: true);
        }

        private void DoCreate()
        {
            try
            {
                KhuyenMaiService.Create(txtTen.Text, (int)numPhanTram.Value, dtTuNgay.Value, dtDenNgay.Value, cbTrangThai.Text);
                Reload();
                SetHint("Đã thêm khuyến mãi.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message);
            }
        }

        private void DoUpdate()
        {
            try
            {
                KhuyenMaiService.Update(_selectedId, txtTen.Text, (int)numPhanTram.Value, dtTuNgay.Value, dtDenNgay.Value, cbTrangThai.Text);
                Reload();
                SetHint("Đã cập nhật khuyến mãi.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message);
            }
        }

        private void DoDelete()
        {
            try
            {
                if (_selectedId <= 0) { MessageBox.Show("Chọn 1 khuyến mãi trước."); return; }

                if (MessageBox.Show("Xóa khuyến mãi này?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                KhuyenMaiService.Delete(_selectedId);
                Reload();
                SetHint("Đã xóa khuyến mãi.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearForm()
        {
            txtTen.Text = "";
            numPhanTram.Value = 10;
            dtTuNgay.Value = DateTime.Today;
            dtDenNgay.Value = DateTime.Today;
            cbTrangThai.SelectedIndex = 0;

            _selectedId = -1;
            SetHint("Chọn 1 dòng để sửa / xóa.", ok: true);
        }

        private void SetHint(string msg, bool ok)
        {
            lbHint.Text = msg;
            lbHint.ForeColor = ok ? Color.Gray : Color.IndianRed;
        }

        // ===== helpers =====
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
                else b.BackColor = Color.FromArgb(
                    Math.Max(0, (int)(Theme.PrimaryBlue.R * 0.9)),
                    Math.Max(0, (int)(Theme.PrimaryBlue.G * 0.9)),
                    Math.Max(0, (int)(Theme.PrimaryBlue.B * 0.9))
                );
            };
            b.MouseLeave += (_, __) =>
            {
                if (outline) b.BackColor = Color.White;
                else b.BackColor = Theme.PrimaryBlue;
            };

            return b;
        }

        // cue banner
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
