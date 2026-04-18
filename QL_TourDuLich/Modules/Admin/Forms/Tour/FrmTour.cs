using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;
using System.Diagnostics;
namespace QL_TourDuLich.Modules.Admin.Forms.Tour
{
    public class FrmTour : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        // Search
        private TextBox txtSearch = new TextBox();

        // Main image (ảnh chính - to)
        private PictureBox picMain = new PictureBox();

        // Form inputs
        private TextBox txtTenTour = new TextBox();
        private ComboBox cbLoaiTour = new ComboBox();
        private TextBox txtDiaDiem = new TextBox();
        private NumericUpDown numGia = new NumericUpDown();
        private DateTimePicker dtKhoiHanh = new DateTimePicker();
        private NumericUpDown numSoCho = new NumericUpDown();
        private ComboBox cbTrangThai = new ComboBox();
        private TextBox txtLyDoHuy = new TextBox();

        // Buttons
        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear;
        private IconButton btnMoBan, btnDongBan, btnHuyTour, btnLichTrinh;
        private IconButton btnAddImg, btnViewDetail;

        private Label lbHint = new Label();

        // Images area (thumbnail)
        private FlowLayoutPanel pnlImages = new FlowLayoutPanel();
        private Label lbImgTitle = new Label();

        public FrmTour()
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

            // Events
            Load += (_, __) => Reload();
            grid.SelectionChanged += Grid_SelectionChanged;
            txtSearch.TextChanged += (_, __) => ApplySearch();

            btnReload.Click += (_, __) => Reload();
            btnClear.Click += (_, __) => ClearForm();

            btnAdd.Click += (_, __) => DoCreate();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnDelete.Click += (_, __) => DoDelete();

            btnMoBan.Click += (_, __) => DoSetTrangThai("DANG_MO");
            btnDongBan.Click += (_, __) => DoSetTrangThai("KET_THUC");
            btnHuyTour.Click += (_, __) => DoHuyTour();
            btnLichTrinh.Click += (_, __) => OpenLichTrinh();

            // add image + view detail
            btnAddImg.Click += (_, __) => DoAddImage();
            btnViewDetail.Click += (_, __) => OpenTourDetail();

            // dblclick ảnh chính cũng mở chi tiết
            picMain.DoubleClick += (_, __) => OpenTourDetail();
        }

        // ======================= CARD LIST =======================
        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách tour",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 220,
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
            TrySetCue(txtSearch, "Tìm theo tên tour / địa điểm / trạng thái...");

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

            // HEADER
            var head = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            var lbTitle = new Label
            {
                Text = "Thông tin tour",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật tour, mở bán, đóng bán hoặc hủy tour (bắt buộc lý do).",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 28)
            };

            head.Controls.Add(lbTitle);
            head.Controls.Add(lbSub);

            // BODY SPLIT (anti-crash)
            var body = new SplitContainer
            {
                Dock = DockStyle.Fill,                 // ✅ quan trọng: fill toàn bộ phần còn lại
                Orientation = Orientation.Vertical,
                SplitterWidth = 10,
                BackColor = Color.White
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
                    int p1Min = Math.Max(260, width - p2Min - sw);

                    if (p1Min + p2Min + sw > width)
                    {
                        p1Min = Math.Max(200, width - p2Min - sw);
                        p2Min = Math.Max(200, width - p1Min - sw);
                    }

                    int desired = (int)(width * 0.68);
                    int minNew = p1Min;
                    int maxNew = width - p2Min - sw;
                    if (maxNew < minNew) maxNew = minNew;

                    body.SplitterDistance = Math.Max(minNew, Math.Min(desired, maxNew));

                    body.Panel1MinSize = p1Min;
                    body.Panel2MinSize = p2Min;

                    desired = (int)(width * 0.68);
                    int min = body.Panel1MinSize;
                    int max = width - body.Panel2MinSize - sw;
                    if (max < min) max = min;
                    body.SplitterDistance = Math.Max(min, Math.Min(desired, max));
                }
                finally
                {
                    _applyingSplit = false;
                }
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
                RowCount = 5,
                BackColor = Color.White
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            StyleText(txtTenTour);
            StyleCombo(cbLoaiTour);
            StyleText(txtDiaDiem);

            numGia.Maximum = 2000000000;
            numGia.Minimum = 0;
            numGia.Increment = 50000;
            numGia.ThousandsSeparator = true;
            numGia.Font = new Font("Segoe UI", 10);

            dtKhoiHanh.Format = DateTimePickerFormat.Custom;
            dtKhoiHanh.CustomFormat = "dd/MM/yyyy";
            dtKhoiHanh.Font = new Font("Segoe UI", 10);

            numSoCho.Maximum = 500;
            numSoCho.Minimum = 1;
            numSoCho.Value = 20;
            numSoCho.Font = new Font("Segoe UI", 10);

            cbTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTrangThai.Items.Clear();
            cbTrangThai.Items.AddRange(new object[] { "DANG_MO", "HUY", "KET_THUC" });
            cbTrangThai.SelectedIndex = 0;
            StyleCombo(cbTrangThai);

            txtLyDoHuy.Multiline = true;
            txtLyDoHuy.Height = 60;
            StyleText(txtLyDoHuy);

            AddRow(tlp, 0, "Tên tour", txtTenTour, "Loại tour", cbLoaiTour);
            AddRow(tlp, 1, "Địa điểm", txtDiaDiem, "Giá", numGia);
            AddRow(tlp, 2, "Khởi hành", dtKhoiHanh, "Số chỗ", numSoCho);
            AddRow(tlp, 3, "Trạng thái", cbTrangThai, "", new Label());

            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            var lbLyDo = MakeLabel("Lý do hủy");
            txtLyDoHuy.Dock = DockStyle.Fill;
            tlp.Controls.Add(lbLyDo, 0, 4);
            tlp.Controls.Add(txtLyDoHuy, 1, 4);
            tlp.SetColumnSpan(txtLyDoHuy, 3);

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

            btnAdd = MakeBtn("Tạo tour", IconChar.Plus, outline: false, w: 130);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 130);
            btnDelete = MakeBtn("Xóa tour", IconChar.Trash, outline: true, w: 130);

            btnAdd.Margin = new Padding(0, 0, 10, 0);
            btnUpdate.Margin = new Padding(0, 0, 10, 0);

            crudRow.Controls.Add(btnAdd);
            crudRow.Controls.Add(btnUpdate);
            crudRow.Controls.Add(btnDelete);

            left.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White });
            left.Controls.Add(crudRow);
            left.Controls.Add(tlp);

            // ======================= RIGHT (FIX CHỒNG + HIỆN ĐỦ TÁC VỤ + RÀNG BUỘC) =======================
            var right = NewSoftCard();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(12);
            right.AutoScroll = true;
            right.AutoScrollMinSize = new Size(0, 1);
            right.AutoScrollMargin = new Size(0, 18);

            // ✅ Flow layout top-down: chống chồng tuyệt đối trong panel scroll
            var flow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };


            // Title
            var prTitle = new Label
            {
                Text = "Tác vụ nhanh",
                AutoSize = false,
                Height = 26,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Margin = new Padding(0, 0, 0, 6)
            };

            // Actions container
            var act = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(0)
            };

            // Buttons (width sẽ được resize tự động bên dưới)
            btnMoBan = MakeBtn("Mở bán (DANG_MO)", IconChar.CirclePlay, outline: false, w: 230);
            btnDongBan = MakeBtn("Đóng/Kết thúc", IconChar.CircleCheck, outline: true, w: 230);
            btnHuyTour = MakeBtn("Hủy tour (HUY)", IconChar.Ban, outline: true, w: 230);
            btnLichTrinh = MakeBtn("Lịch trình tour", IconChar.Route, outline: true, w: 230);
            btnAddImg = MakeBtn("Thêm ảnh", IconChar.Image, outline: true, w: 230);
            btnViewDetail = MakeBtn("Xem chi tiết", IconChar.Eye, outline: false, w: 230);

            btnMoBan.Margin = new Padding(0, 0, 0, 8);
            btnDongBan.Margin = new Padding(0, 0, 0, 8);
            btnHuyTour.Margin = new Padding(0, 0, 0, 8);
            btnLichTrinh.Margin = new Padding(0, 0, 0, 8);
            btnAddImg.Margin = new Padding(0, 0, 0, 8);
            btnViewDetail.Margin = new Padding(0, 0, 0, 8);

            act.Controls.Add(btnMoBan);
            act.Controls.Add(btnDongBan);
            act.Controls.Add(btnHuyTour);
            act.Controls.Add(btnLichTrinh);
            act.Controls.Add(btnAddImg);
            act.Controls.Add(btnViewDetail);

            // NOTE (ràng buộc) - luôn hiển thị, xuống dòng tự nhiên
            var note = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Text =
                    "Chú ý:\n" +
                    "• Không xóa tour đã có khách đăng ký.\n" +
                    "• Hủy tour phải nhập lý do.\n" +
                    "• Chuột phải ảnh để xóa / đặt ảnh chính.",
                Padding = new Padding(0, 6, 0, 6),
                Margin = new Padding(0, 0, 0, 10)
            };

            // Images card
            var imgCard = NewSoftCard();
            imgCard.Padding = new Padding(10);
            imgCard.Margin = new Padding(0);
            imgCard.Width = 300;                // sẽ resize lại
            imgCard.Height = 320;               // giữ đẹp; nếu thiếu sẽ scroll ở panel right

            lbImgTitle = new Label
            {
                Text = "Hình ảnh tour",
                AutoSize = false,
                Height = 22,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Theme.TextDark
            };

            picMain = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 180,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            pnlImages = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White,
                Padding = new Padding(4)
            };

            imgCard.Controls.Add(pnlImages);
            imgCard.Controls.Add(picMain);
            imgCard.Controls.Add(lbImgTitle);

            // add vào flow theo thứ tự từ trên xuống
            flow.Controls.Add(prTitle);
            flow.Controls.Add(act);
            flow.Controls.Add(note);
            

            right.Controls.Clear();
            right.Controls.Add(flow);

            body.Panel2.Controls.Add(right);

            // ✅ Auto fit width (không che, không tràn)
            void FitRightWidth()
            {
                int w = right.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6;
                if (w < 240) w = 240;
                flow.Width = w;
                prTitle.Width = w;
                act.Width = w;
                note.MaximumSize = new Size(w, 0);

                
               
                // nút = full width
                foreach (Control c in act.Controls)
                {
                    c.Width = w;
                }
            }
            right.HandleCreated += (_, __) => right.BeginInvoke((Action)FitRightWidth);
            right.SizeChanged += (_, __) => FitRightWidth();







            // HINT
            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chọn 1 tour trong danh sách để cập nhật / mở bán / hủy.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = false,
                Height = 26,
                Padding = new Padding(0, 6, 0, 0)
            };

            card.Controls.Add(body);    // Fill
            card.Controls.Add(lbHint);  // Top
            card.Controls.Add(head);    // Top


            return card;
        }

        // ======================= DATA =======================
        private void Reload()
        {
            var dtLoai = TourService.GetLoaiTour();
            cbLoaiTour.DisplayMember = "TenLoai";
            cbLoaiTour.ValueMember = "LoaiTourID";
            cbLoaiTour.DataSource = dtLoai;

            _dtAll = TourService.GetAll();
            grid.DataSource = _dtAll;

            _selectedId = -1;
            ClearForm();
            LoadImages(-1);
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
            dv.RowFilter = $"TenTour LIKE '%{safe}%' OR DiaDiem LIKE '%{safe}%' OR TrangThai LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void DoCreate()
        {
            if (string.IsNullOrWhiteSpace(txtTenTour.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên tour.");
                return;
            }
            if (cbLoaiTour.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Loại tour.");
                return;
            }

            TourService.Create(
                txtTenTour.Text.Trim(),
                Convert.ToInt32(cbLoaiTour.SelectedValue),
                txtDiaDiem.Text.Trim(),
                Convert.ToDecimal(numGia.Value),
                dtKhoiHanh.Value.Date,
                Convert.ToInt32(numSoCho.Value),
                cbTrangThai.Text,
                (cbTrangThai.Text == "HUY") ? txtLyDoHuy.Text.Trim() : null
            );

            Reload();
            SetHint("Tạo tour thành công.", ok: true);
        }

        private void DoUpdate()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tour để cập nhật."); return; }

            if (cbTrangThai.Text == "HUY" && string.IsNullOrWhiteSpace(txtLyDoHuy.Text))
            {
                MessageBox.Show("Hủy tour phải nhập lý do.");
                return;
            }

            TourService.Update(
                _selectedId,
                txtTenTour.Text.Trim(),
                Convert.ToInt32(cbLoaiTour.SelectedValue),
                txtDiaDiem.Text.Trim(),
                Convert.ToDecimal(numGia.Value),
                dtKhoiHanh.Value.Date,
                Convert.ToInt32(numSoCho.Value),
                cbTrangThai.Text,
                (cbTrangThai.Text == "HUY") ? txtLyDoHuy.Text.Trim() : null
            );

            Reload();
            SetHint("Cập nhật tour thành công.", ok: true);
        }

        private void DoDelete()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tour để xóa."); return; }

            if (TourService.HasDangKy(_selectedId))
            {
                MessageBox.Show("Không thể xóa tour đã có khách đăng ký.");
                return;
            }

            if (MessageBox.Show("Xóa tour này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            TourService.Delete(_selectedId);
            Reload();
            SetHint("Xóa tour thành công.", ok: true);
        }

        private void DoSetTrangThai(string trangThai)
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tour trước."); return; }

            if (trangThai == "DANG_MO")
            {
                TourService.SetTrangThai(_selectedId, "DANG_MO", null);
                Reload();
                SetHint("Đã mở bán tour.", ok: true);
                return;
            }

            if (trangThai == "KET_THUC")
            {
                TourService.SetTrangThai(_selectedId, "KET_THUC", null);
                Reload();
                SetHint("Đã kết thúc tour.", ok: true);
                return;
            }
        }

        private void DoHuyTour()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 tour trước."); return; }

            var reason = (txtLyDoHuy.Text ?? "").Trim();
            if (reason.Length == 0)
            {
                MessageBox.Show("Vui lòng nhập Lý do hủy tour.");
                return;
            }

            TourService.SetTrangThai(_selectedId, "HUY", reason);
            Reload();
            SetHint("Đã hủy tour.", ok: true);
        }

        private void OpenLichTrinh()
        {
            if (_selectedId < 0)
            {
                MessageBox.Show("Chọn 1 tour trước.");
                return;
            }

            using (var f = new FrmLichTrinhTour(_selectedId))
            {
                f.ShowDialog(this);
            }
        }

        private void ClearForm()
        {
            txtTenTour.Text = "";
            txtDiaDiem.Text = "";
            numGia.Value = 0;
            dtKhoiHanh.Value = DateTime.Today;
            numSoCho.Value = 20;
            cbTrangThai.SelectedIndex = 0;
            txtLyDoHuy.Text = "";

            SetHint("Chọn 1 tour trong danh sách để cập nhật / mở bán / hủy.", ok: true);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow;
            if (row.Cells["TourID"]?.Value == null) return;

            _selectedId = Convert.ToInt32(row.Cells["TourID"].Value);

            txtTenTour.Text = row.Cells["TenTour"].Value?.ToString();
            txtDiaDiem.Text = row.Cells["DiaDiem"].Value?.ToString();

            if (row.Cells["LoaiTourID"]?.Value != null)
                cbLoaiTour.SelectedValue = Convert.ToInt32(row.Cells["LoaiTourID"].Value);

            if (row.Cells["Gia"]?.Value != null) numGia.Value = Convert.ToDecimal(row.Cells["Gia"].Value);

            if (row.Cells["NgayKhoiHanh"]?.Value != null &&
                DateTime.TryParse(row.Cells["NgayKhoiHanh"].Value.ToString(), out var d))
                dtKhoiHanh.Value = d;

            if (row.Cells["SoChoToiDa"]?.Value != null)
                numSoCho.Value = Convert.ToDecimal(row.Cells["SoChoToiDa"].Value);

            cbTrangThai.Text = row.Cells["TrangThai"].Value?.ToString() ?? "DANG_MO";
            txtLyDoHuy.Text = row.Cells["LyDoHuy"].Value?.ToString() ?? "";

            SetHint($"Đang chọn TourID = {_selectedId}.", ok: true);

            // ✅ bình thường: hiện ảnh nhỏ + ảnh chính
            LoadImages(_selectedId);
        }

        // ======================= IMAGES =======================
        private void DoAddImage()
        {
            if (_selectedId <= 0)
            {
                MessageBox.Show("Chọn 1 tour trước khi thêm ảnh.");
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh tour";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                string dbPath;
                try
                {
                    dbPath = SaveImagePortable(ofd.FileName); // => "/img/xxx.jpg"
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi copy ảnh: " + ex.Message);
                    return;
                }

                var setMain = MessageBox.Show("Đặt ảnh này làm ảnh chính?", "Ảnh chính",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

                string mota = PromptText("Mô tả ảnh", "Nhập mô tả ảnh (có thể bỏ trống):", "");

                try
                {
                    TourService.AddHinhAnhTour(_selectedId, dbPath, mota, setMain);
                    LoadImages(_selectedId);
                    SetHint("Đã thêm ảnh cho tour.", ok: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lưu DB: " + ex.Message);
                }
            }
        }

        private static string PromptText(string title, string message, string defaultValue)
        {
            using (var f = new Form())
            using (var lb = new Label())
            using (var tb = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                f.Text = title;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.MinimizeBox = false;
                f.MaximizeBox = false;
                f.ClientSize = new Size(420, 150);

                lb.Text = message;
                lb.AutoSize = false;
                lb.SetBounds(12, 12, 396, 40);

                tb.Text = defaultValue ?? "";
                tb.SetBounds(12, 56, 396, 24);

                btnOk.Text = "OK";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.SetBounds(252, 100, 75, 28);

                btnCancel.Text = "Hủy";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.SetBounds(333, 100, 75, 28);

                f.Controls.AddRange(new Control[] { lb, tb, btnOk, btnCancel });
                f.AcceptButton = btnOk;
                f.CancelButton = btnCancel;

                return f.ShowDialog() == DialogResult.OK ? (tb.Text ?? "").Trim() : (defaultValue ?? "");
            }
        }

        private void LoadImages(int tourId)
        {
            pnlImages.Controls.Clear();
            picMain.Image = null;

            if (tourId <= 0)
            {
                lbImgTitle.Text = "Hình ảnh tour";
                pnlImages.Controls.Add(new Label
                {
                    Text = "Chọn 1 tour để xem hình.",
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Padding = new Padding(6)
                });
                return;
            }

            DataTable dt;
            try
            {
                dt = TourService.GetHinhAnhTour(tourId);
            }
            catch
            {
                lbImgTitle.Text = "Hình ảnh tour (lỗi load)";
                return;
            }

            lbImgTitle.Text = $"Hình ảnh tour (TourID: {tourId})";

            if (dt == null || dt.Rows.Count == 0)
            {
                pnlImages.Controls.Add(new Label
                {
                    Text = "Tour này chưa có hình ảnh.",
                    AutoSize = true,
                    ForeColor = Color.Gray,
                    Padding = new Padding(6)
                });
                return;
            }

            // ===== set ảnh chính lên picMain =====
            DataRow mainRow = null;
            foreach (DataRow rr in dt.Rows)
            {
                try
                {
                    if (Convert.ToInt32(rr["LaAnhChinh"]) == 1) { mainRow = rr; break; }
                }
                catch { }
            }
            if (mainRow == null) mainRow = dt.Rows[0];

            string dbgMain;
            string mainDbPath = (mainRow["DuongDan"]?.ToString() ?? "").Trim();
            picMain.Image = LoadBitmapSafe(mainDbPath, out dbgMain);

            // bật 1 lần để debug nếu vẫn không lên
            // if (picMain.Image == null) MessageBox.Show("Không load được ảnh chính:\n" + dbgMain);




            // ===== thumbnails bình thường =====
            foreach (DataRow r in dt.Rows)
            {
                int hinhAnhId = 0;
                try { hinhAnhId = Convert.ToInt32(r["HinhAnhID"]); } catch { }

                string dbPath = (r["DuongDan"]?.ToString() ?? "").Trim();
                string mota = (r["MoTa"]?.ToString() ?? "").Trim();

                bool laChinh = false;
                try { laChinh = Convert.ToInt32(r["LaAnhChinh"]) == 1; } catch { }

                var tag = new ImgTag { TourId = tourId, HinhAnhId = hinhAnhId, DbPath = dbPath, LaChinh = laChinh };

                var item = new Panel
                {
                    Width = 170,
                    Height = 150,
                    BackColor = Color.White,
                    Margin = new Padding(6),
                    Tag = tag
                };
                UiFX.Round(item, 12);

                var pic = new PictureBox
                {
                    Dock = DockStyle.Top,
                    Height = 110,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = tag,
                    Cursor = Cursors.Hand
                };

                string dbgThumb;
                pic.Image = LoadBitmapSafe(dbPath, out dbgThumb);

                // nếu muốn bắt lỗi thumbnail thì bật dòng này
                // if (pic.Image == null) Debug.WriteLine("Thumb fail:\n" + dbgThumb);


                // click thumbnail => set lên ảnh chính
                pic.Click += (_, __) =>
                {
                    string dbgClick;
                    var bmp = LoadBitmapSafe(tag.DbPath, out dbgClick);
                    if (bmp != null) picMain.Image = bmp;
                    // else MessageBox.Show("Không load được ảnh khi click:\n" + dbgClick);
                };


                var lb = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = (laChinh ? "★ " : "") + (string.IsNullOrWhiteSpace(mota) ? dbPath : mota),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = laChinh ? Theme.PrimaryBlue : Color.Gray,
                    Padding = new Padding(4),
                    AutoEllipsis = true,
                    Tag = tag
                };

                // context menu (chuột phải)
                var menu = BuildImageContextMenu();
                item.ContextMenuStrip = menu;
                pic.ContextMenuStrip = menu;
                lb.ContextMenuStrip = menu;

                item.Controls.Add(lb);
                item.Controls.Add(pic);

                pnlImages.Controls.Add(item);
            }
        }

        private ContextMenuStrip BuildImageContextMenu()
        {
            var menu = new ContextMenuStrip();

            var miSetMain = new ToolStripMenuItem("Đặt làm ảnh chính");
            var miDelete = new ToolStripMenuItem("Xóa ảnh");

            miSetMain.Click += (_, __) =>
            {
                var tag = GetImgTagFromContextMenu(menu);
                if (tag == null) return;

                try
                {
                    TourService.SetAnhChinh(tag.TourId, tag.HinhAnhId);
                    LoadImages(tag.TourId);
                    SetHint("Đã đặt ảnh chính.", ok: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đặt ảnh chính: " + ex.Message);
                }
            };

            miDelete.Click += (_, __) =>
            {
                var tag = GetImgTagFromContextMenu(menu);
                if (tag == null) return;

                if (MessageBox.Show("Xóa ảnh này khỏi tour?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    TourService.DeleteHinhAnh(tag.TourId, tag.HinhAnhId);
                    LoadImages(tag.TourId);
                    SetHint("Đã xóa ảnh.", ok: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa ảnh: " + ex.Message);
                }
            };

            menu.Items.Add(miSetMain);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miDelete);

            return menu;
        }

        private ImgTag GetImgTagFromContextMenu(ContextMenuStrip menu)
        {
            var src = menu.SourceControl;
            if (src == null) return null;

            if (src.Tag is ImgTag t1) return t1;
            if (src.Parent != null && src.Parent.Tag is ImgTag t2) return t2;

            return null;
        }

        private class ImgTag
        {
            public int TourId;
            public int HinhAnhId;
            public string DbPath;
            public bool LaChinh;
        }

        // ======================= VIEW DETAIL (hiện tất cả ảnh + thông tin) =======================
        private void OpenTourDetail()
        {
            if (_selectedId <= 0)
            {
                MessageBox.Show("Chọn 1 tour trước.");
                return;
            }

            using (var f = new FrmTourDetail(
                _selectedId,
                txtTenTour.Text,
                cbLoaiTour.Text,
                txtDiaDiem.Text,
                numGia.Value,
                dtKhoiHanh.Value,
                (int)numSoCho.Value,
                cbTrangThai.Text,
                txtLyDoHuy.Text
            ))
            {
                f.ShowDialog(this);
            }
        }

        private class FrmTourDetail : Form
        {
            private readonly int _tourId;
            private readonly string _ten, _loai, _diaDiem, _trangThai, _lyDo;
            private readonly decimal _gia;
            private readonly DateTime _khoiHanh;
            private readonly int _soCho;

            private PictureBox picBig = new PictureBox();
            private FlowLayoutPanel pnlThumb = new FlowLayoutPanel();

            public FrmTourDetail(int tourId, string ten, string loai, string diaDiem, decimal gia,
                DateTime khoiHanh, int soCho, string trangThai, string lyDo)
            {
                _tourId = tourId;
                _ten = ten ?? "";
                _loai = loai ?? "";
                _diaDiem = diaDiem ?? "";
                _gia = gia;
                _khoiHanh = khoiHanh;
                _soCho = soCho;
                _trangThai = trangThai ?? "";
                _lyDo = lyDo ?? "";

                Text = "Chi tiết tour";
                Width = 1060;
                Height = 680;
                StartPosition = FormStartPosition.CenterParent;
                BackColor = Color.White;
                Font = new Font("Segoe UI", 9);

                BuildUI();
                Load += (_, __) => Reload();
            }

            private void BuildUI()
            {
                // ===== HEADER (mỏng, gọn) =====
                var header = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 44,
                    BackColor = Theme.PrimaryBlue,
                    Padding = new Padding(16, 0, 16, 0)
                };
                var lbHeader = new Label
                {
                    Text = "CHI TIẾT TOUR",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White
                };
                header.Controls.Add(lbHeader);

                // ===== INFO WRAP (AUTO SIZE => KHÔNG CẮT CHỮ) =====
                var infoWrap = new Panel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = Color.White,
                    Padding = new Padding(16, 12, 16, 8)
                };

                var infoCard = new Panel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(14),
                    BackColor = Color.FromArgb(248, 250, 255)
                };
                infoCard.Paint += (_, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(210, 225, 245));
                    e.Graphics.DrawRectangle(pen, 0, 0, infoCard.Width - 1, infoCard.Height - 1);
                };

                var tlp = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 2,
                    BackColor = Color.Transparent
                };
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                void AddRow(string label, string value, bool emphasize = false)
                {
                    int r = tlp.RowCount++;
                    tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                    var lb = new Label
                    {
                        Text = label,
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Theme.PrimaryBlue,
                        Margin = new Padding(0, 3, 10, 3)
                    };

                    var val = new Label
                    {
                        Text = value ?? "",
                        AutoSize = true,
                        MaximumSize = new Size(760, 0),   // cho phép xuống dòng nếu dài
                        Font = emphasize ? new Font("Segoe UI", 10, FontStyle.Bold) : new Font("Segoe UI", 9),
                        ForeColor = emphasize ? Theme.PrimaryBlue : Color.FromArgb(30, 30, 30),
                        Margin = new Padding(0, 3, 0, 3)
                    };

                    tlp.Controls.Add(lb, 0, r);
                    tlp.Controls.Add(val, 1, r);
                }

                AddRow("Tour ID:", _tourId.ToString());
                AddRow("Tên tour:", _ten, emphasize: true);
                AddRow("Loại tour:", _loai);
                AddRow("Địa điểm:", _diaDiem);
                AddRow("Giá:", $"{_gia:N0} VNĐ", emphasize: true);
                AddRow("Khởi hành:", _khoiHanh.ToString("dd/MM/yyyy"));
                AddRow("Số chỗ:", _soCho.ToString());

                // trạng thái có badge (nhấn xanh dương / đỏ)
                string stText = string.IsNullOrWhiteSpace(_trangThai) ? "—" : _trangThai;
                AddRow("Trạng thái:", stText, emphasize: true);
                AddRow("Lý do hủy:", string.IsNullOrWhiteSpace(_lyDo) ? "—" : _lyDo);

                infoCard.Controls.Add(tlp);
                infoWrap.Controls.Add(infoCard);

                // ===== BODY: layout 2 cột chuẩn =====
                var body = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = Color.White,
                    Padding = new Padding(16, 8, 16, 8)
                };
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420)); // ảnh lớn trái fixed đẹp
                body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                // LEFT: big image card
                var leftCard = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };
                leftCard.Paint += (_, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(235, 235, 235));
                    e.Graphics.DrawRectangle(pen, 0, 0, leftCard.Width - 1, leftCard.Height - 1);
                };

                var lbBig = new Label
                {
                    Text = "Ảnh chính",
                    Dock = DockStyle.Top,
                    Height = 22,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Theme.PrimaryBlue
                };

                picBig.Dock = DockStyle.Fill;
                picBig.SizeMode = PictureBoxSizeMode.Zoom;
                picBig.BackColor = Color.White;
                picBig.BorderStyle = BorderStyle.FixedSingle;

                leftCard.Controls.Add(picBig);
                leftCard.Controls.Add(lbBig);

                // RIGHT: thumbnails grid
                var rightCard = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10),
                    BackColor = Color.White
                };
                rightCard.Paint += (_, e) =>
                {
                    using var pen = new Pen(Color.FromArgb(235, 235, 235));
                    e.Graphics.DrawRectangle(pen, 0, 0, rightCard.Width - 1, rightCard.Height - 1);
                };

                var lbThumb = new Label
                {
                    Text = "Tất cả hình ảnh",
                    Dock = DockStyle.Top,
                    Height = 22,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Theme.PrimaryBlue
                };

                pnlThumb.Dock = DockStyle.Fill;
                pnlThumb.AutoScroll = true;
                pnlThumb.WrapContents = true;
                pnlThumb.FlowDirection = FlowDirection.LeftToRight;
                pnlThumb.Padding = new Padding(4);
                pnlThumb.BackColor = Color.White;

                rightCard.Controls.Add(pnlThumb);
                rightCard.Controls.Add(lbThumb);

                body.Controls.Add(leftCard, 0, 0);
                body.Controls.Add(rightCard, 1, 0);

                // ===== FOOTER: nút nhỏ gọn =====
                var footer = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.White,
                    Padding = new Padding(16, 8, 16, 8)
                };

                var btnClose = new Button
                {
                    Text = "Đóng",
                    Width = 90,
                    Height = 32,
                    BackColor = Theme.PrimaryBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (_, __) => Close();

                footer.Controls.Add(btnClose);
                footer.Resize += (_, __) =>
                {
                    btnClose.Left = footer.ClientSize.Width - btnClose.Width;
                    btnClose.Top = (footer.ClientSize.Height - btnClose.Height) / 2;
                };

                // ===== ADD ORDER =====
                Controls.Add(body);
                Controls.Add(footer);
                Controls.Add(infoWrap);
                Controls.Add(header);
            }

            private void Reload()
            {
                pnlThumb.Controls.Clear();
                picBig.Image = null;

                DataTable dt;
                try { dt = TourService.GetHinhAnhTour(_tourId); }
                catch { return; }

                if (dt == null || dt.Rows.Count == 0)
                {
                    pnlThumb.Controls.Add(new Label { Text = "Tour này chưa có hình ảnh.", AutoSize = true, ForeColor = Color.Gray });
                    return;
                }

                // main
                DataRow main = null;
                foreach (DataRow rr in dt.Rows)
                {
                    try { if (Convert.ToInt32(rr["LaAnhChinh"]) == 1) { main = rr; break; } } catch { }
                }
                if (main == null) main = dt.Rows[0];
                SetBig(main["DuongDan"]?.ToString());

                // thumbs
                foreach (DataRow r in dt.Rows)
                {
                    string dbPath = (r["DuongDan"]?.ToString() ?? "").Trim();
                    string mota = (r["MoTa"]?.ToString() ?? "").Trim();
                    bool laChinh = false;
                    try { laChinh = Convert.ToInt32(r["LaAnhChinh"]) == 1; } catch { }

                    var card = new Panel
                    {
                        Width = 220,
                        Height = 170,
                        BackColor = Color.White,
                        Margin = new Padding(8),
                        Padding = new Padding(6)
                    };
                    card.Paint += (_, e) =>
                    {
                        using var pen = new Pen(Color.FromArgb(235, 235, 235));
                        e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                    };

                    var p = new PictureBox
                    {
                        Dock = DockStyle.Top,
                        Height = 125,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BorderStyle = BorderStyle.FixedSingle,
                        Cursor = Cursors.Hand,
                        BackColor = Color.White
                    };

                    // load thumb
                    var full = ResolveImagePathStatic(dbPath);
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(full) && File.Exists(full))
                        {
                            using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var img = Image.FromStream(fs))
                                p.Image = new Bitmap(img);
                        }
                    }
                    catch { }

                    p.Click += (_, __) => SetBig(dbPath);

                    var lb = new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = (laChinh ? "★ " : "") + (string.IsNullOrWhiteSpace(mota) ? dbPath : mota),
                        Font = new Font("Segoe UI", 8),
                        ForeColor = laChinh ? Theme.PrimaryBlue : Color.Gray,
                        Padding = new Padding(2, 6, 2, 0),
                        AutoEllipsis = true
                    };

                    card.Controls.Add(lb);
                    card.Controls.Add(p);
                    pnlThumb.Controls.Add(card);
                }
            }

            private void SetBig(string dbPath)
            {
                try
                {
                    var full = ResolveImagePathStatic(dbPath);
                    if (!string.IsNullOrWhiteSpace(full) && File.Exists(full))
                    {
                        using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var img = Image.FromStream(fs))
                            picBig.Image = new Bitmap(img);
                    }
                }
                catch { }
            }

            private static string ResolveImagePathStatic(string dbPath)
            {
                if (string.IsNullOrWhiteSpace(dbPath)) return "";
                dbPath = dbPath.Trim();

                if (dbPath.Length >= 2 && char.IsLetter(dbPath[0]) && dbPath[1] == ':') return dbPath;
                if (dbPath.StartsWith(@"\\") || dbPath.StartsWith("//")) return dbPath;

                string rel = dbPath.Replace('/', '\\').TrimStart('\\', '/');
                return Path.Combine(Application.StartupPath, rel);
            }
        }


        // ======================= PATHS =======================
        private static string GetRuntimeImgFolder()
        {
            return Path.Combine(Application.StartupPath, "img");
        }

        private string SaveImagePortable(string sourceFile)
        {
            var folder = GetRuntimeImgFolder();
            Directory.CreateDirectory(folder);

            string ext = Path.GetExtension(sourceFile);
            string baseName = Path.GetFileNameWithoutExtension(sourceFile);
            string fileName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";

            string dest = Path.Combine(folder, fileName);
            File.Copy(sourceFile, dest, overwrite: true);

            return "/img/" + fileName; // DB lưu portable
        }

        private static bool IsWindowsAbsolutePath(string p)
        {
            if (string.IsNullOrWhiteSpace(p)) return false;

            // C:\... hoặc D:\...
            if (p.Length >= 2 && char.IsLetter(p[0]) && p[1] == ':') return true;

            // \\server\share\...
            if (p.StartsWith(@"\\") || p.StartsWith("//")) return true;

            return false;
        }

        private static string ResolveImagePath(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath)) return "";

            dbPath = dbPath.Trim();

            // absolute thật sự kiểu Windows (C:\... hoặc \\server\share)
            if (IsWindowsAbsolutePath(dbPath))
                return dbPath;

            // ✅ normalize: đổi hết về '\', bỏ mọi ký tự phân cách đầu chuỗi
            string rel = dbPath.Replace('/', '\\').TrimStart('\\', '/');

            // nơi exe đang chạy
            string startup = Application.StartupPath;

            // 1) startup\rel  (vd: ...\bin\Debug\img\a.jpg)
            string p1 = Path.Combine(startup, rel);
            if (File.Exists(p1)) return p1;

            // 2) startup\img\<filename> (fallback nếu rel bị sai dạng)
            string onlyName = Path.GetFileName(rel);
            string p2 = Path.Combine(startup, "img", onlyName);
            if (File.Exists(p2)) return p2;

            // 3) fallback: lùi lên 1-2 cấp (hay gặp khi chạy ...\bin\Debug\net8.0-windows\)
            try
            {
                var dir = new DirectoryInfo(startup);
                for (int i = 0; i < 3 && dir != null; i++)
                {
                    string up1 = Path.Combine(dir.FullName, rel);
                    if (File.Exists(up1)) return up1;

                    string up2 = Path.Combine(dir.FullName, "img", onlyName);
                    if (File.Exists(up2)) return up2;

                    dir = dir.Parent;
                }
            }
            catch { }

            return p1; // trả về để debug
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
        private static Bitmap LoadBitmapSafe(string dbPath, out string debug)
        {
            debug = "";
            try
            {
                string full = ResolveImagePath(dbPath);
                debug = $"DB={dbPath}\nFULL={full}";

                if (string.IsNullOrWhiteSpace(full) || !File.Exists(full))
                {
                    debug += "\n=> FILE NOT FOUND";
                    return null;
                }

                using (var fs = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var img = Image.FromStream(fs))
                {
                    return new Bitmap(img);
                }
            }
            catch (Exception ex)
            {
                debug += "\nEX=" + ex.Message;
                return null;
            }
        }


    }
}
