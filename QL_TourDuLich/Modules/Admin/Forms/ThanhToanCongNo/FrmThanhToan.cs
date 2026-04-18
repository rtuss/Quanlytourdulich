using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;
using QL_TourDuLich.Modules.Admin.Services;
using QL_TourDuLich.Shared;
using QL_TourDuLich.Modules.Admin.Forms.DangKy;

namespace QL_TourDuLich.Modules.Admin.Forms.ThanhToan
{
    public class FrmThanhToan : Form
    {
        private DataGridView grid = new DataGridView();
        private DataTable _dtAll;
        private int _selectedId = -1;

        private string _currentMode = "GIAODICH";
        private bool _isSelectingFromCongNo = false;

        private TextBox txtSearch = new TextBox();
        private ComboBox cbFilter = new ComboBox();

        private ComboBox cbDangKy = new ComboBox();
        private NumericUpDown numSoTien = new NumericUpDown();
        private DateTimePicker dtNgayThanhToan = new DateTimePicker();
        private ComboBox cbTrangThai = new ComboBox();
        private TextBox txtGhiChu = new TextBox();

        private IconButton btnAdd, btnUpdate, btnDelete, btnReload, btnClear, btnInvoice;
        private IconButton btnViewTransactions, btnViewDebt;

        private Label lbHint = new Label();

        public FrmThanhToan()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Theme.ApplyForm(this);
            BackColor = Color.White;
            Padding = new Padding(0);

            var wrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(wrap);

            var cardList = BuildCardList();
            cardList.Dock = DockStyle.Top;
            cardList.Height = 320;
            cardList.MinimumSize = new Size(0, 250);

            var cardForm = BuildCardForm();
            cardForm.Dock = DockStyle.Fill;

            wrap.Controls.Add(cardForm);
            wrap.Controls.Add(cardList);

            Load += (_, __) => Reload();
            grid.SelectionChanged += Grid_SelectionChanged;
            txtSearch.TextChanged += (_, __) => ApplySearch();
            cbFilter.SelectedIndexChanged += (_, __) => ApplySearch();

            btnReload.Click += (_, __) => Reload();
            btnClear.Click += (_, __) => ClearForm();

            btnAdd.Click += (_, __) => DoCreate();
            btnUpdate.Click += (_, __) => DoUpdate();
            btnDelete.Click += (_, __) => DoDelete();
            btnInvoice.Click += (_, __) => DoInvoice();

            btnViewTransactions.Click += (_, __) =>
            {
                _currentMode = "GIAODICH";
                ReloadGridByMode();
            };

            btnViewDebt.Click += (_, __) =>
            {
                _currentMode = "CONGNO";
                ReloadGridByMode();
            };
        }

        private Panel BuildCardList()
        {
            var card = NewCard();
            card.Padding = new Padding(14);

            var header = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Color.White };

            var title = new Label
            {
                Text = "Quản lý thanh toán",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Location = new Point(0, 8),
                AutoSize = true
            };

            var modeRow = new FlowLayoutPanel
            {
                Location = new Point(0, 42),
                Width = 460,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White
            };

            btnViewTransactions = MakeBtn("Xem giao dịch", IconChar.Receipt, outline: false, w: 210);
            btnViewDebt = MakeBtn("Xem công nợ", IconChar.MoneyBillWave, outline: true, w: 200);

            btnViewTransactions.Margin = new Padding(0, 0, 10, 0);
            btnViewDebt.Margin = new Padding(0);

            modeRow.Controls.Add(btnViewTransactions);
            modeRow.Controls.Add(btnViewDebt);

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
            header.Controls.Add(modeRow);
            header.Controls.Add(title);

            var searchRow = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.White,
                Padding = new Padding(0, 6, 0, 6)
            };

            var searchWrap = new Panel { Dock = DockStyle.Left, Width = 430, Height = 32, BackColor = Color.White };
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
            txtSearch.Width = 390;
            TrySetCue(txtSearch, "Tìm theo Tour / Khách hàng / Trạng thái / Ghi chú...");

            searchWrap.Controls.Add(ico);
            searchWrap.Controls.Add(txtSearch);

            cbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cbFilter.Font = new Font("Segoe UI", 10);
            cbFilter.Width = 220;
            cbFilter.Location = new Point(450, 6);
            cbFilter.Items.Clear();
            cbFilter.Items.AddRange(new object[]
            {
                "TẤT CẢ",
                "ĐÃ THANH TOÁN ĐỦ",
                "CHƯA THANH TOÁN",
                "ĐÃ CỌC"
            });
            cbFilter.SelectedIndex = 0;

            searchRow.Controls.Add(searchWrap);
            searchRow.Controls.Add(cbFilter);

            var gridPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 6, 0, 0) };
            grid.Dock = DockStyle.Fill;
            SetupGridStyle(grid);
            gridPanel.Controls.Add(grid);

            card.Controls.Add(gridPanel);
            card.Controls.Add(searchRow);
            card.Controls.Add(header);

            return card;
        }

        private Panel BuildCardForm()
        {
            var card = NewCard();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(14, 14, 14, 18);

            var head = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            var lbTitle = new Label
            {
                Text = "Thông tin thanh toán",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lbSub = new Label
            {
                Text = "Tạo / cập nhật / hủy giao dịch và theo dõi công nợ khách hàng.",
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
                Height = 360
            };

            bool applyingSplit = false;
            void ApplySplitSafe()
            {
                if (applyingSplit || body.IsDisposed || !body.IsHandleCreated || body.Width <= 0) return;

                try
                {
                    applyingSplit = true;

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
                finally { applyingSplit = false; }
            }

            body.HandleCreated += (_, __) => body.BeginInvoke((Action)ApplySplitSafe);
            body.SizeChanged += (_, __) => ApplySplitSafe();

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

            StyleCombo(cbDangKy);

            numSoTien.Maximum = 2000000000;
            numSoTien.Minimum = 0;
            numSoTien.Increment = 50000;
            numSoTien.ThousandsSeparator = true;
            numSoTien.Font = new Font("Segoe UI", 10);

            dtNgayThanhToan.Format = DateTimePickerFormat.Custom;
            dtNgayThanhToan.CustomFormat = "dd/MM/yyyy";
            dtNgayThanhToan.Font = new Font("Segoe UI", 10);

            cbTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTrangThai.Items.Clear();
            cbTrangThai.Items.AddRange(new object[] { "CHUA_DU", "DA_THANH_TOAN" });
            cbTrangThai.SelectedIndex = 0;
            StyleCombo(cbTrangThai);
            cbTrangThai.Enabled = false;

            txtGhiChu.Multiline = true;
            txtGhiChu.Height = 64;
            StyleText(txtGhiChu);

            AddRow(tlp, 0, "Đăng ký", cbDangKy, "Số tiền", numSoTien);
            AddRow(tlp, 1, "Ngày TT", dtNgayThanhToan, "Trạng thái", cbTrangThai);

            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            var lbGhiChu = MakeLabel("Ghi chú");
            txtGhiChu.Dock = DockStyle.Fill;
            tlp.Controls.Add(lbGhiChu, 0, 2);
            tlp.Controls.Add(txtGhiChu, 1, 2);
            tlp.SetColumnSpan(txtGhiChu, 3);

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

            btnAdd = MakeBtn("Tạo thanh toán", IconChar.Plus, outline: false, w: 170);
            btnUpdate = MakeBtn("Cập nhật", IconChar.PenToSquare, outline: false, w: 130);
            btnDelete = MakeBtn("Hủy giao dịch", IconChar.Trash, outline: true, w: 140);
            btnInvoice = MakeBtn("Xuất hóa đơn", IconChar.FileInvoice, outline: true, w: 160);

            btnAdd.Margin = new Padding(0, 0, 10, 0);
            btnUpdate.Margin = new Padding(0, 0, 10, 0);
            btnDelete.Margin = new Padding(0, 0, 10, 0);

            crudRow.Controls.Add(btnAdd);
            crudRow.Controls.Add(btnUpdate);
            crudRow.Controls.Add(btnDelete);
            crudRow.Controls.Add(btnInvoice);

            left.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White });
            left.Controls.Add(crudRow);
            left.Controls.Add(tlp);

            var right = NewSoftCard();
            right.Dock = DockStyle.Fill;
            right.Padding = new Padding(12);

            var prTitle = new Label
            {
                Text = "Gợi ý",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 26
            };

            var note = new Label
            {
                Dock = DockStyle.Top,
                Height = 190,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Text =
                    "• CHƯA THANH TOÁN: tổng tiền đã trả = 0\n" +
                    "• ĐÃ CỌC: đã trả > 0 nhưng chưa đủ\n" +
                    "• ĐÃ THANH TOÁN: đã thanh toán đủ\n" +
                    "• Hủy giao dịch = xóa mềm, không mất dữ liệu\n" +
                    "• Chỉ xuất hóa đơn khi trạng thái = DA_THANH_TOAN",
                Padding = new Padding(0, 10, 0, 0)
            };

            right.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent });
            right.Controls.Add(note);
            right.Controls.Add(prTitle);

            body.Panel2.Controls.Add(right);

            lbHint = new Label
            {
                Dock = DockStyle.Top,
                Text = "Chọn 1 thanh toán trong danh sách để cập nhật / hủy.",
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

        private void Reload()
        {
            var dtDK = ThanhToanService.GetDangKyLookup();
            cbDangKy.DisplayMember = "TenHienThi";
            cbDangKy.ValueMember = "DangKyID";
            cbDangKy.DataSource = dtDK;

            _selectedId = -1;
            ReloadGridByMode();
            ClearForm();
        }

        private void ReloadGridByMode()
        {
            if (_currentMode == "CONGNO")
            {
                _dtAll = ThanhToanService.GetCongNoTongHop();
                grid.DataSource = _dtAll;

                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                btnInvoice.Enabled = false;

                SetHint("Đang xem danh sách công nợ tổng hợp. Chọn 1 dòng để đổ xuống form và tạo thanh toán.", true);
            }
            else
            {
                _dtAll = ThanhToanService.GetAll();
                grid.DataSource = _dtAll;

                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnInvoice.Enabled = false;

                SetHint("Đang xem danh sách giao dịch thanh toán.", true);
            }

            grid.ClearSelection();
            _selectedId = -1;

            ApplySearch();
            UpdateModeButtons();
        }

        private void UpdateModeButtons()
        {
            if (_currentMode == "CONGNO")
            {
                ApplyOutlineStyle(btnViewTransactions, true);
                ApplyOutlineStyle(btnViewDebt, false);
            }
            else
            {
                ApplyOutlineStyle(btnViewTransactions, false);
                ApplyOutlineStyle(btnViewDebt, true);
            }
        }

        private void ApplySearch()
        {
            if (_dtAll == null) return;

            var dv = _dtAll.DefaultView;
            var q = (txtSearch.Text ?? "").Trim();
            var safe = q.Replace("'", "''");

            string filterText = "";

            if (_currentMode == "CONGNO")
            {
                if (!string.IsNullOrWhiteSpace(q))
                {
                    filterText = $"TenTour LIKE '%{safe}%' OR HoTen LIKE '%{safe}%' OR TrangThaiCongNo LIKE '%{safe}%'";
                }

                string debtFilter = "";
                string selected = cbFilter.Text?.Trim() ?? "TẤT CẢ";

                if (selected == "ĐÃ THANH TOÁN ĐỦ")
                    debtFilter = "TrangThaiCongNo = 'DA_THANH_TOAN'";
                else if (selected == "CHƯA THANH TOÁN")
                    debtFilter = "TrangThaiCongNo = 'CHUA_THANH_TOAN'";
                else if (selected == "ĐÃ CỌC")
                    debtFilter = "TrangThaiCongNo = 'DA_COC'";

                if (!string.IsNullOrWhiteSpace(filterText) && !string.IsNullOrWhiteSpace(debtFilter))
                    dv.RowFilter = "(" + filterText + ") AND (" + debtFilter + ")";
                else if (!string.IsNullOrWhiteSpace(filterText))
                    dv.RowFilter = filterText;
                else if (!string.IsNullOrWhiteSpace(debtFilter))
                    dv.RowFilter = debtFilter;
                else
                    dv.RowFilter = "";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    dv.RowFilter = "";
                }
                else
                {
                    dv.RowFilter =
                        $"TenTour LIKE '%{safe}%' OR HoTen LIKE '%{safe}%' OR TrangThai LIKE '%{safe}%' OR GhiChu LIKE '%{safe}%'";
                }
            }

            grid.DataSource = dv;
        }

        private void DoCreate()
        {
            if (cbDangKy.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Đăng ký.");
                return;
            }

            if (numSoTien.Value <= 0)
            {
                MessageBox.Show("Vui lòng nhập số tiền thanh toán lớn hơn 0.");
                return;
            }

            try
            {
                ThanhToanService.Create(
                    Convert.ToInt32(cbDangKy.SelectedValue),
                    Convert.ToDecimal(numSoTien.Value),
                    dtNgayThanhToan.Value.Date,
                    cbTrangThai.Text,
                    txtGhiChu.Text
                );

                Reload();

                if (_currentMode == "CONGNO")
                {
                    _dtAll = ThanhToanService.GetCongNoTongHop();
                    grid.DataSource = _dtAll;
                    ApplySearch();
                    SetHint("Tạo thanh toán thành công. Công nợ đã được cập nhật.", true);
                }
                else
                {
                    SetHint("Tạo thanh toán thành công.", true);
                }

                MessageBox.Show("Tạo thanh toán thành công.");
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, false);
                MessageBox.Show(ex.Message);
            }
        }

        private void DoUpdate()
        {
            if (_currentMode != "GIAODICH") return;

            if (_selectedId < 0)
            {
                MessageBox.Show("Chọn 1 thanh toán để cập nhật.");
                return;
            }

            if (cbDangKy.SelectedValue == null)
            {
                MessageBox.Show("Thiếu Đăng ký.");
                return;
            }

            if (numSoTien.Value <= 0)
            {
                MessageBox.Show("Số tiền thanh toán phải lớn hơn 0.");
                return;
            }

            try
            {
                ThanhToanService.Update(
                    _selectedId,
                    Convert.ToInt32(cbDangKy.SelectedValue),
                    Convert.ToDecimal(numSoTien.Value),
                    dtNgayThanhToan.Value.Date,
                    cbTrangThai.Text,
                    txtGhiChu.Text
                );

                Reload();
                SetHint("Cập nhật thanh toán thành công.", true);
                MessageBox.Show("Cập nhật thanh toán thành công.");
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, false);
                MessageBox.Show(ex.Message);
            }
        }

        private void DoDelete()
        {
            if (_currentMode != "GIAODICH") return;

            if (_selectedId < 0)
            {
                MessageBox.Show("Chọn 1 thanh toán để hủy.");
                return;
            }

            if (MessageBox.Show(
                "Hủy giao dịch thanh toán này?\nDữ liệu sẽ không bị xóa khỏi database, chỉ đánh dấu hủy.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                ThanhToanService.SoftDelete(_selectedId);
                Reload();
                SetHint("Hủy giao dịch thành công.", true);
                MessageBox.Show("Hủy giao dịch thành công.");
            }
            catch (Exception ex)
            {
                SetHint(ex.Message, false);
                MessageBox.Show(ex.Message);
            }
        }

        private void DoInvoice()
        {
            if (_currentMode != "GIAODICH") return;

            if (_selectedId < 0)
            {
                MessageBox.Show("Chọn 1 thanh toán để xuất hóa đơn.");
                return;
            }

            if ((cbTrangThai.Text ?? "").Trim() != "DA_THANH_TOAN")
            {
                MessageBox.Show("Chỉ được xuất hóa đơn khi Trạng thái = DA_THANH_TOAN.");
                return;
            }

            if (cbDangKy.SelectedValue == null)
            {
                MessageBox.Show("Thiếu Đăng ký.");
                return;
            }

            int dangKyId = Convert.ToInt32(cbDangKy.SelectedValue);

            var dt = DangKyService.GetInvoicePreview(dangKyId);
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu hóa đơn.");
                return;
            }

            PdfInvoice.ExportAndOpen(dt);
        }

        private void ClearForm()
        {
            if (cbDangKy.Items.Count > 0) cbDangKy.SelectedIndex = 0;
            numSoTien.Value = 0;
            dtNgayThanhToan.Value = DateTime.Today;
            cbTrangThai.SelectedIndex = 0;
            txtGhiChu.Text = "";

            _selectedId = -1;
            _isSelectingFromCongNo = false;

            if (_currentMode == "CONGNO")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                btnInvoice.Enabled = false;

                SetHint("Đang xem công nợ. Chọn 1 dòng để đổ xuống form rồi nhập thanh toán.", true);
            }
            else
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnInvoice.Enabled = false;

                SetHint("Chọn 1 thanh toán trong danh sách để cập nhật / hủy.", true);
            }
        }

        private void FillFormFromCongNoRow(DataGridViewRow row)
        {
            if (row == null) return;
            if (!grid.Columns.Contains("DangKyID")) return;
            if (row.Cells["DangKyID"]?.Value == null) return;

            _isSelectingFromCongNo = true;
            _selectedId = -1;

            int dangKyId = Convert.ToInt32(row.Cells["DangKyID"].Value);

            if (cbDangKy.DataSource != null)
                cbDangKy.SelectedValue = dangKyId;

            numSoTien.Value = 0;
            dtNgayThanhToan.Value = DateTime.Today;

            string trangThaiCongNo = "";
            if (grid.Columns.Contains("TrangThaiCongNo"))
                trangThaiCongNo = row.Cells["TrangThaiCongNo"]?.Value?.ToString() ?? "";

            decimal conNo = 0;
            if (grid.Columns.Contains("ConNo") && row.Cells["ConNo"]?.Value != null)
                decimal.TryParse(row.Cells["ConNo"].Value.ToString(), out conNo);

            if (trangThaiCongNo == "DA_THANH_TOAN")
            {
                cbTrangThai.Text = "DA_THANH_TOAN";
                txtGhiChu.Text = "Đăng ký này đã thanh toán đủ.";
                btnAdd.Enabled = false;
            }
            else
            {
                cbTrangThai.Text = "CHUA_DU";

                if (trangThaiCongNo == "CHUA_THANH_TOAN")
                    txtGhiChu.Text = "Thanh toán lần đầu";
                else if (trangThaiCongNo == "DA_COC")
                    txtGhiChu.Text = "Thanh toán phần còn lại";
                else
                    txtGhiChu.Text = "";

                btnAdd.Enabled = true;
            }

            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnInvoice.Enabled = false;

            SetHint($"Đã chọn đăng ký #{dangKyId}. Còn nợ: {conNo:n0}. Nhập số tiền và bấm Tạo thanh toán.", true);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            if (grid.CurrentRow.IsNewRow) return;
            if (grid.Columns.Count == 0) return;

            var row = grid.CurrentRow;

            if (_currentMode == "CONGNO")
            {
                FillFormFromCongNoRow(row);
                return;
            }

            if (!grid.Columns.Contains("ThanhToanID")) return;
            if (row.Cells["ThanhToanID"]?.Value == null) return;

            _isSelectingFromCongNo = false;
            _selectedId = Convert.ToInt32(row.Cells["ThanhToanID"].Value);

            if (grid.Columns.Contains("DangKyID") && row.Cells["DangKyID"]?.Value != null)
                cbDangKy.SelectedValue = Convert.ToInt32(row.Cells["DangKyID"].Value);

            if (grid.Columns.Contains("SoTien") && row.Cells["SoTien"]?.Value != null)
                numSoTien.Value = Convert.ToDecimal(row.Cells["SoTien"].Value);

            if (grid.Columns.Contains("NgayThanhToan") &&
                row.Cells["NgayThanhToan"]?.Value != null &&
                DateTime.TryParse(row.Cells["NgayThanhToan"].Value.ToString(), out var d))
            {
                dtNgayThanhToan.Value = d;
            }

            if (grid.Columns.Contains("TrangThai"))
                cbTrangThai.Text = row.Cells["TrangThai"]?.Value?.ToString() ?? "CHUA_DU";
            else
                cbTrangThai.Text = "CHUA_DU";

            if (grid.Columns.Contains("GhiChu"))
                txtGhiChu.Text = row.Cells["GhiChu"]?.Value?.ToString() ?? "";
            else
                txtGhiChu.Text = "";

            btnAdd.Enabled = true;
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;

            if (btnInvoice != null)
                btnInvoice.Enabled = ((cbTrangThai.Text ?? "").Trim() == "DA_THANH_TOAN");

            SetHint($"Đang chọn ThanhToanID = {_selectedId}.", true);
        }

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

            g.DataBindingComplete += (_, __) =>
            {
                foreach (DataGridViewRow row in g.Rows)
                {
                    if (_currentMode == "CONGNO" && g.Columns.Contains("TrangThaiCongNo") && row.Cells["TrangThaiCongNo"]?.Value != null)
                    {
                        string s = row.Cells["TrangThaiCongNo"].Value.ToString();
                        if (s == "CHUA_THANH_TOAN")
                            row.DefaultCellStyle.BackColor = Color.MistyRose;
                        else if (s == "DA_COC")
                            row.DefaultCellStyle.BackColor = Color.LemonChiffon;
                        else if (s == "DA_THANH_TOAN")
                            row.DefaultCellStyle.BackColor = Color.Honeydew;
                    }
                }
            };
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

        private void ApplyOutlineStyle(IconButton b, bool outline)
        {
            if (b == null) return;

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