using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.Tour
{
    public class FrmLichTrinhTour : Form
    {
        private readonly int _tourId;

        private readonly DataGridView grid = new DataGridView();
        private readonly Label lbTitle = new Label();
        private readonly IconButton btnClose = new IconButton();

        public FrmLichTrinhTour(int tourId)
        {
            _tourId = tourId;

            Text = "Lịch trình tour";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;

            BuildUI();

            Load += (_, __) => Reload();
        }

        private void BuildUI()
        {
            var top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White,
                Padding = new Padding(16, 12, 16, 10)
            };

            lbTitle.Text = $"LỊCH TRÌNH TOUR - TourID: {_tourId}";
            lbTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lbTitle.ForeColor = Theme.TextDark;
            lbTitle.Dock = DockStyle.Left;
            lbTitle.AutoSize = true;

            btnClose.Text = "Đóng";
            btnClose.IconChar = IconChar.Xmark;
            btnClose.IconSize = 18;
            btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClose.Dock = DockStyle.Right;
            btnClose.Width = 110;
            btnClose.Height = 34;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnClose.BackColor = Color.White;
            btnClose.ForeColor = Theme.PrimaryBlue;
            btnClose.IconColor = Theme.PrimaryBlue;
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.BorderColor = Theme.PrimaryBlue;
            btnClose.Click += (_, __) => Close();

            top.Controls.Add(btnClose);
            top.Controls.Add(lbTitle);

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16, 8, 16, 16)
            };

            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = true; // ✅ chắc chắn hiện cột
            SetupGridStyle(grid);

            content.Controls.Add(grid);

            Controls.Add(content);
            Controls.Add(top);
        }

        private void Reload()
        {
            // ✅ vì bạn đã gộp vào TourService/TourRepo
            DataTable dt = TourService.GetLichTrinhByTourId(_tourId);
            grid.DataSource = dt;

            // Ẩn TourID nếu có
            if (grid.Columns.Contains("TourID"))
                grid.Columns["TourID"].Visible = false;

            if (grid.Columns.Contains("NgayThu"))
            {
                grid.Columns["NgayThu"].HeaderText = "Ngày thứ";
                grid.Columns["NgayThu"].FillWeight = 12;
            }
            if (grid.Columns.Contains("Buoi"))
            {
                grid.Columns["Buoi"].HeaderText = "Buổi";
                grid.Columns["Buoi"].FillWeight = 14;
            }
            if (grid.Columns.Contains("DiaDiem"))
            {
                grid.Columns["DiaDiem"].HeaderText = "Địa điểm";
                grid.Columns["DiaDiem"].FillWeight = 22;
            }
            if (grid.Columns.Contains("NoiDung"))
            {
                grid.Columns["NoiDung"].HeaderText = "Nội dung";
                grid.Columns["NoiDung"].FillWeight = 52;
            }
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
    }
}
