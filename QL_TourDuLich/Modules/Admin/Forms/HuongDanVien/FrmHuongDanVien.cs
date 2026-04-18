using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;

namespace QL_TourDuLich.Modules.Admin.Forms.HuongDanVien
{
    public class FrmHuongDanVien : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        private TextBox txtSearch = new TextBox();

        private TextBox txtHoTen = new TextBox();
        private TextBox txtDienThoai = new TextBox();
        private TextBox txtKinhNghiem = new TextBox();
        private TextBox txtNgonNgu = new TextBox();
        private ComboBox cbTrangThai = new ComboBox();

        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear, btnAssign;
        private Label lbHint = new Label();

        // Right preview (assign history)
        private Label lbRightTitle = new Label();
        private Label lbRightSub = new Label();
        private DataGridView gridAssign = new DataGridView();
        private Label lbAssignEmpty = new Label();

        public FrmHuongDanVien()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14) };
            Controls.Add(wrap);

            var cardList = BuildCardList();
            cardList.Dock = DockStyle.Top;
            cardList.Height = 320;
            cardList.MinimumSize = new Size(0, 260);

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
            btnAssign.Click += (_, __) => OpenAssign();

            // initial state
            SetAssignButtonState();
            LoadAssignPreview(-1, "");
        }

        // ======================= CARD LIST =======================
        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };

            var title = new Label
            {
                Text = "Danh sách hướng dẫn viên",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 340,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
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

            // Search row
            var searchRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.White,
                Padding = new Padding(0, 8, 0, 6)
            };

            var searchWrap = new Panel
            {
                Dock = DockStyle.Left,
                Width = 620,
                Height = 34,
                BackColor = Color.White
            };
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
                Location = new Point(10, 9)
            };

            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.ForeColor = Theme.TextDark;
            txtSearch.Location = new Point(34, 8);
            txtSearch.Width = 580;

            TrySetCue(txtSearch, "Tìm theo tên / SĐT / kinh nghiệm / ngôn ngữ / trạng thái...");

            searchWrap.Controls.Add(ico);
            searchWrap.Controls.Add(txtSearch);
            searchRow.Controls.Add(searchWrap);

            // Grid
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0, 6, 0, 0)
            };
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

            var head = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = Color.White };

            var lbTitle = new Label
            {
                Text = "Thông tin hướng dẫn viên",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật / xóa hướng dẫn viên. Có thể phân công dẫn tour.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, 30)
            };

            head.Controls.Add(lbTitle);
            head.Controls.Add(lbSub);

            // ---------- Body split: LEFT form / RIGHT preview ----------
            var body = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 10,
                BackColor = Color.White
            };

            // ✅ Safe layout for SplitContainer (no crash)
            bool applying = false;
            void ApplySplitSafe()
            {
                if (applying) return;
                if (body.IsDisposed) return;
                if (!body.IsHandleCreated) return;

                int w = body.ClientSize.Width;
                int sw = body.SplitterWidth;
                if (w <= 0) return;

                applying = true;
                try
                {
                    // ====== 1) tính minsize động (luôn khả thi) ======
                    int desiredMinLeft = 380;
                    int desiredMinRight = 320;
                    int hardMin = 220; // tối thiểu để không "nát" UI

                    int available = w - sw;
                    if (available <= hardMin * 2)
                    {
                        // quá chật: chia đều
                        desiredMinLeft = Math.Max(0, available / 2);
                        desiredMinRight = Math.Max(0, available - desiredMinLeft);
                    }
                    else if (available < desiredMinLeft + desiredMinRight)
                    {
                        // chật vừa: ưu tiên giữ phải
                        desiredMinRight = Math.Max(hardMin, Math.Min(desiredMinRight, available - hardMin));
                        desiredMinLeft = Math.Max(hardMin, available - desiredMinRight);
                    }

                    int minLeft = desiredMinLeft;
                    int minRight = desiredMinRight;

                    // ====== 2) ép splitterDistance hiện tại về range CŨ an toàn trước khi đổi min ======
                    // (đây là bước tránh crash khi set PanelMinSize)
                    int maxLeftOld = Math.Max(0, w - body.Panel2MinSize - sw);
                    int minLeftOld = body.Panel1MinSize;
                    int sd = body.SplitterDistance;

                    if (maxLeftOld < minLeftOld) maxLeftOld = minLeftOld;
                    sd = Math.Max(minLeftOld, Math.Min(sd, maxLeftOld));
                    if (body.SplitterDistance != sd) body.SplitterDistance = sd;

                    // ====== 3) set minsize (giờ splitterDistance đã hợp lệ nên không nổ) ======
                    body.Panel1MinSize = minLeft;
                    body.Panel2MinSize = minRight;

                    // ====== 4) set splitterDistance mục tiêu (kẹp theo range MỚI) ======
                    int maxLeft = w - body.Panel2MinSize - sw;
                    if (maxLeft < body.Panel1MinSize) maxLeft = body.Panel1MinSize;

                    int desiredLeft = (int)(w * 0.62);
                    int left = Math.Max(body.Panel1MinSize, Math.Min(desiredLeft, maxLeft));

                    if (body.SplitterDistance != left) body.SplitterDistance = left;
                }
                finally
                {
                    applying = false;
                }
            }


            body.HandleCreated += (_, __) => body.BeginInvoke((Action)ApplySplitSafe);
            body.SizeChanged += (_, __) => ApplySplitSafe();

            // ---------- LEFT side ----------
            var left = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            body.Panel1.Controls.Add(left);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // label fixed => không wrap / không mất chữ
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            StyleText(txtHoTen);
            StyleText(txtDienThoai);
            StyleText(txtKinhNghiem);
            StyleText(txtNgonNgu);

            cbTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            StyleCombo(cbTrangThai);
            cbTrangThai.Items.Clear();
            cbTrangThai.Items.AddRange(new object[] { "RANH", "DANG_DAN" });
            cbTrangThai.SelectedIndex = 0;

            AddRow(tlp, 0, "Họ tên", txtHoTen, "Trạng thái", cbTrangThai);
            AddRow(tlp, 1, "Điện thoại", txtDienThoai, "Ngôn ngữ", txtNgonNgu);
            AddRow(tlp, 2, "Kinh nghiệm", txtKinhNghiem, "", new Label());

            // hint
            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chọn 1 hướng dẫn viên trong danh sách để cập nhật / xóa.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = false,
                Height = 28,
                Padding = new Padding(0, 10, 0, 0)
            };

            // buttons row (wrap => không tràn)
            var crudRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 10, 0, 0),
                Margin = new Padding(0)
            };

            btnAdd = MakeBtn("Tạo HDV", IconChar.Plus, outline: false, w: 140);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 140);
            btnDelete = MakeBtn("Xóa HDV", IconChar.Trash, outline: true, w: 140);

            btnAdd.Margin = new Padding(0, 0, 10, 10);
            btnUpdate.Margin = new Padding(0, 0, 10, 10);
            btnDelete.Margin = new Padding(0, 0, 10, 10);

            crudRow.Controls.Add(btnAdd);
            crudRow.Controls.Add(btnUpdate);
            crudRow.Controls.Add(btnDelete);
            btnAssign = MakeBtn("Phân công HDV", IconChar.UserCheck, outline: false, w: 180);
            btnAssign.Margin = new Padding(0, 0, 10, 10);

            // đưa nút phân công lên chung hàng
            crudRow.Controls.Add(btnAssign);



            left.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White });
            left.Controls.Add(crudRow);
            left.Controls.Add(lbHint);
            left.Controls.Add(tlp);

            // ---------- RIGHT side preview ----------
            var right = NewSoftCard();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(12);

            lbRightTitle = new Label
            {
                Text = "Lịch phân công gần nhất",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 26
            };

            lbRightSub = new Label
            {
                Text = "Chọn 1 HDV để xem lịch phân công.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Height = 22
            };

            var previewWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            gridAssign.Dock = DockStyle.Fill;
            SetupGridStyleSmall(gridAssign);

            lbAssignEmpty = new Label
            {
                Text = "Chưa có dữ liệu phân công.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Visible = false
            };

            previewWrap.Controls.Add(gridAssign);
            previewWrap.Controls.Add(lbAssignEmpty);

            right.Controls.Add(previewWrap);
            right.Controls.Add(lbRightSub);
            right.Controls.Add(lbRightTitle);

            body.Panel2.Controls.Add(right);

            card.Controls.Add(body);
            card.Controls.Add(head);

            return card;
        }

        // ======================= DATA =======================
        private void Reload()
        {
            _dtAll = HuongDanVienService.GetAll();
            grid.DataSource = _dtAll;

            if (grid.Columns["HDVID"] != null) grid.Columns["HDVID"].HeaderText = "ID";
            if (grid.Columns["HoTen"] != null) grid.Columns["HoTen"].HeaderText = "Họ tên";
            if (grid.Columns["DienThoai"] != null) grid.Columns["DienThoai"].HeaderText = "Điện thoại";
            if (grid.Columns["KinhNghiem"] != null) grid.Columns["KinhNghiem"].HeaderText = "Kinh nghiệm";
            if (grid.Columns["NgonNgu"] != null) grid.Columns["NgonNgu"].HeaderText = "Ngôn ngữ";
            if (grid.Columns["TrangThai"] != null) grid.Columns["TrangThai"].HeaderText = "Trạng thái";

            _selectedId = -1;
            ClearForm();
        }

        private void ApplySearch()
        {
            if (_dtAll == null) return;

            var q = (txtSearch.Text ?? "").Trim();
            if (q.Length == 0) { grid.DataSource = _dtAll; return; }

            var dv = _dtAll.DefaultView;
            var safe = q.Replace("'", "''");
            dv.RowFilter =
                $"HoTen LIKE '%{safe}%' OR DienThoai LIKE '%{safe}%' OR KinhNghiem LIKE '%{safe}%' OR NgonNgu LIKE '%{safe}%' OR TrangThai LIKE '%{safe}%'";
            grid.DataSource = dv;
        }

        private void DoCreate()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Họ tên.");
                return;
            }

            try
            {
                HuongDanVienService.Create(
                    txtHoTen.Text,
                    txtDienThoai.Text,
                    txtKinhNghiem.Text,
                    txtNgonNgu.Text,
                    cbTrangThai.Text
                );

                Reload();
                SetHint("Tạo hướng dẫn viên thành công.", ok: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoUpdate()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 hướng dẫn viên để cập nhật."); return; }

            try
            {
                HuongDanVienService.Update(
                    _selectedId,
                    txtHoTen.Text,
                    txtDienThoai.Text,
                    txtKinhNghiem.Text,
                    txtNgonNgu.Text,
                    cbTrangThai.Text
                );

                Reload();
                SetHint("Cập nhật hướng dẫn viên thành công.", ok: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DoDelete()
        {
            if (_selectedId < 0) { MessageBox.Show("Chọn 1 hướng dẫn viên để xóa."); return; }

            if (MessageBox.Show("Xóa hướng dẫn viên này?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                HuongDanVienService.Delete(_selectedId);
                Reload();
                SetHint("Xóa hướng dẫn viên thành công.", ok: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenAssign()
        {
            if (_selectedId <= 0)
            {
                MessageBox.Show("Chọn HDV trước khi phân công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var name = (txtHoTen.Text ?? "").Trim();
            using (var f = new FrmPhanCongHDV(_selectedId, name))
            {
                f.ShowDialog(this);
            }

            Reload();
        }

        private void ClearForm()
        {
            txtHoTen.Text = "";
            txtDienThoai.Text = "";
            txtKinhNghiem.Text = "";
            txtNgonNgu.Text = "";
            cbTrangThai.SelectedIndex = 0;

            grid.ClearSelection();
            _selectedId = -1;

            SetAssignButtonState();
            LoadAssignPreview(-1, "");
            SetHint("Chọn 1 hướng dẫn viên trong danh sách để cập nhật / xóa.", ok: true);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;

            object idObj = grid.CurrentRow.Cells["HDVID"]?.Value;
            if (idObj == null || idObj == DBNull.Value) return;

            _selectedId = Convert.ToInt32(idObj);

            txtHoTen.Text = grid.CurrentRow.Cells["HoTen"]?.Value?.ToString() ?? "";
            txtDienThoai.Text = grid.CurrentRow.Cells["DienThoai"]?.Value?.ToString() ?? "";
            txtKinhNghiem.Text = grid.CurrentRow.Cells["KinhNghiem"]?.Value?.ToString() ?? "";
            txtNgonNgu.Text = grid.CurrentRow.Cells["NgonNgu"]?.Value?.ToString() ?? "";

            var st = (grid.CurrentRow.Cells["TrangThai"]?.Value?.ToString() ?? "RANH").Trim();
            cbTrangThai.SelectedItem = (st == "DANG_DAN") ? "DANG_DAN" : "RANH";

            SetAssignButtonState();
            LoadAssignPreview(_selectedId, txtHoTen.Text.Trim());
            SetHint($"Đang chọn HDVID = {_selectedId}.", ok: true);
        }

        // ======================= ASSIGN PREVIEW =======================
        private void SetAssignButtonState()
        {
            bool ok = _selectedId > 0;
            btnAssign.Enabled = ok;

            if (ok)
            {
                btnAssign.BackColor = Theme.PrimaryBlue;
                btnAssign.ForeColor = Color.White;
                btnAssign.IconColor = Color.White;
                btnAssign.Cursor = Cursors.Hand;
            }
            else
            {
                btnAssign.BackColor = Color.FromArgb(235, 235, 235);
                btnAssign.ForeColor = Color.Gray;
                btnAssign.IconColor = Color.Gray;
                btnAssign.Cursor = Cursors.No;
            }
        }

        private void LoadAssignPreview(int hdvId, string hdvName)
        {
            lbRightSub.Text = (hdvId > 0)
                ? $"HDV: {hdvName} (ID {hdvId})"
                : "Chọn 1 HDV để xem lịch phân công.";

            try
            {
                if (hdvId <= 0)
                {
                    gridAssign.DataSource = null;
                    lbAssignEmpty.Visible = true;
                    lbAssignEmpty.Text = "Chưa có dữ liệu phân công.";
                    return;
                }

                // ✅ Nếu service bạn đặt tên khác thì đổi đúng dòng này
                DataTable dt = PhanCongHDVService.GetByHDV(hdvId);

                // Sort mới nhất trước nếu có cột TuNgay
                if (dt != null)
                {
                    var dv = dt.DefaultView;
                    if (dt.Columns.Contains("TuNgay"))
                        dv.Sort = "TuNgay DESC";
                    dt = dv.ToTable();
                }

                // Top 5
                DataTable top = dt?.Clone();
                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count && i < 5; i++)
                        top.ImportRow(dt.Rows[i]);
                }

                gridAssign.DataSource = top;

                // header
                if (gridAssign.Columns["PhanCongID"] != null) gridAssign.Columns["PhanCongID"].HeaderText = "PC ID";

                // bạn có thể có TourID + TenTour. Ưu tiên show TenTour nếu có.
                if (gridAssign.Columns["TenTour"] != null)
                {
                    gridAssign.Columns["TenTour"].HeaderText = "Tour";
                    if (gridAssign.Columns["TourID"] != null) gridAssign.Columns["TourID"].Visible = false;
                }
                else if (gridAssign.Columns["TourID"] != null)
                {
                    gridAssign.Columns["TourID"].HeaderText = "Tour";
                }

                if (gridAssign.Columns["HDVID"] != null) gridAssign.Columns["HDVID"].Visible = false;
                if (gridAssign.Columns["TuNgay"] != null) gridAssign.Columns["TuNgay"].HeaderText = "Từ ngày";
                if (gridAssign.Columns["DenNgay"] != null) gridAssign.Columns["DenNgay"].HeaderText = "Đến ngày";

                if (gridAssign.Columns["TuNgay"] != null) gridAssign.Columns["TuNgay"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (gridAssign.Columns["DenNgay"] != null) gridAssign.Columns["DenNgay"].DefaultCellStyle.Format = "dd/MM/yyyy";

                lbAssignEmpty.Visible = (top == null || top.Rows.Count == 0);
                if (lbAssignEmpty.Visible) lbAssignEmpty.Text = "Chưa có dữ liệu phân công.";
            }
            catch
            {
                gridAssign.DataSource = null;
                lbAssignEmpty.Visible = true;
                lbAssignEmpty.Text = "Không tải được lịch phân công.";
            }
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

        private void SetupGridStyleSmall(DataGridView g)
        {
            SetupGridStyle(g);
            g.ColumnHeadersHeight = 36;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            g.RowTemplate.Height = 30;
        }

        private void AddRow(TableLayoutPanel tlp, int r, string l1, Control c1, string l2, Control c2)
        {
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            var lb1 = MakeLabel(l1, 110);
            var lb2 = MakeLabel(l2, 110);

            c1.Dock = DockStyle.Fill;
            c2.Dock = DockStyle.Fill;

            // margin để không dính nhau
            c1.Margin = new Padding(0, 8, 10, 0);
            c2.Margin = new Padding(0, 8, 0, 0);

            tlp.Controls.Add(lb1, 0, r);
            tlp.Controls.Add(c1, 1, r);
            tlp.Controls.Add(lb2, 2, r);
            tlp.Controls.Add(c2, 3, r);
        }

        private Label MakeLabel(string text, int width)
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
                Margin = new Padding(0, 8, 10, 0)
            };
        }

        private void StyleText(TextBox t)
        {
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 10);
            t.BackColor = Color.White;
            t.MinimumSize = new Size(0, 34);
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
                if (!b.Enabled) return;
                if (outline) b.BackColor = Color.FromArgb(245, 249, 255);
                else b.BackColor = Darken(Theme.PrimaryBlue, 0.10);
            };
            b.MouseLeave += (_, __) =>
            {
                if (!b.Enabled) return;
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

        // ======================= CUE BANNER =======================
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
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
