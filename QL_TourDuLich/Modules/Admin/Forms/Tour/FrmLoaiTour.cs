using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.Tour
{
    public class FrmLoaiTour : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        private TextBox txtSearch = new TextBox();
        private TextBox txtTenLoai = new TextBox();

        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear;
        private Label lbHint = new Label();

        public FrmLoaiTour()
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

            // CARD LIST
            var cardList = NewCard();
            cardList.Dock = DockStyle.Top;
            cardList.Height = 280;
            cardList.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách loại tour",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                Width = 240,
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

            // CARD FORM
            var cardForm = NewCard();
            cardForm.Dock = DockStyle.Fill;
            cardForm.Padding = new Padding(14);

            var lbFormTitle = new Label
            {
                Text = "Thông tin loại tour",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 28
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật / xóa loại tour. Không thể xóa nếu đang có tour sử dụng.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 20
            };

            var formRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                ColumnCount = 2
            };
            formRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            formRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lbTen = new Label
            {
                Text = "Tên loại tour",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };

            txtTenLoai.Font = new Font("Segoe UI", 10);
            txtTenLoai.BorderStyle = BorderStyle.FixedSingle;
            txtTenLoai.Dock = DockStyle.Fill;

            formRow.Controls.Add(lbTen, 0, 0);
            formRow.Controls.Add(txtTenLoai, 1, 0);

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
            cardForm.Controls.Add(formRow);
            cardForm.Controls.Add(lbSub);
            cardForm.Controls.Add(lbFormTitle);

            wrap.Controls.Add(cardForm);
            wrap.Controls.Add(new Panel { Height = 14, Dock = DockStyle.Top });
            wrap.Controls.Add(cardList);
        }

        private void Reload()
        {
            _dtAll = LoaiTourService.GetAll();
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
            dv.RowFilter = $"TenLoai LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            if (grid.CurrentRow.Cells["LoaiTourID"]?.Value == null) return;

            _selectedId = Convert.ToInt32(grid.CurrentRow.Cells["LoaiTourID"].Value);
            txtTenLoai.Text = grid.CurrentRow.Cells["TenLoai"]?.Value?.ToString() ?? "";

            SetHint($"Đang chọn LoaiTourID = {_selectedId}.", ok: true);
        }

        private void DoCreate()
        {
            try
            {
                LoaiTourService.Create(txtTenLoai.Text);
                Reload();
                SetHint("Đã thêm loại tour.", ok: true);
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
                LoaiTourService.Update(_selectedId, txtTenLoai.Text);
                Reload();
                SetHint("Đã cập nhật loại tour.", ok: true);
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
                if (_selectedId <= 0) { MessageBox.Show("Chọn 1 loại tour trước."); return; }

                if (MessageBox.Show("Xóa loại tour này?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                LoaiTourService.Delete(_selectedId);
                Reload();
                SetHint("Đã xóa loại tour.", ok: true);
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, ok: false);
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearForm()
        {
            txtTenLoai.Text = "";
            _selectedId = -1;
            SetHint("Chọn 1 dòng để sửa / xóa.", ok: true);
        }

        private void SetHint(string msg, bool ok)
        {
            lbHint.Text = msg;
            lbHint.ForeColor = ok ? Color.Gray : Color.IndianRed;
        }

        // ===== helpers giống style bạn đang dùng =====
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
    }
}
