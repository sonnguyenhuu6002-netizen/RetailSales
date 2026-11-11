using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class UcBanHang : UserControl
    {
        readonly SanPhamBUS spBus = new SanPhamBUS();
        readonly NhanVienBUS nvBus = new NhanVienBUS();
        readonly KhachHangBUS khBus = new KhachHangBUS();
        readonly HoaDonBUS hdBus = new HoaDonBUS();

        readonly BindingList<ChiTietHoaDonDTO> gio = new BindingList<ChiTietHoaDonDTO>();
        readonly DataGridView dgv = UI.Grid();
        readonly ComboBox cboNV = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        readonly ComboBox cboKH = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        readonly TextBox txtMaSP;
        readonly NumericUpDown nudSL = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Width = 70 };
        readonly Label lblTong = new Label { AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold), Margin = new Padding(8, 6, 20, 0) };
        readonly Label lblDiem = new Label { AutoSize = true, Margin = new Padding(0, 12, 20, 0) };

        public UcBanHang()
        {
            Dock = DockStyle.Fill;
            var top = UI.Bar(DockStyle.Top);
            top.Controls.Add(new Label { Text = "Nhân viên:", AutoSize = true, Margin = new Padding(6, 8, 0, 0) });
            top.Controls.Add(cboNV);
            top.Controls.Add(new Label { Text = "Khách hàng:", AutoSize = true, Margin = new Padding(12, 8, 0, 0) });
            top.Controls.Add(cboKH);
            cboKH.SelectedIndexChanged += (s, e) => Tinh();

            var add = UI.Bar(DockStyle.Top);
            txtMaSP = UI.Field(add, "Mã SP / quét mã vạch:", 200);
            txtMaSP.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ThemVaoGio(); } };
            add.Controls.Add(new Label { Text = "SL:", AutoSize = true, Margin = new Padding(8, 8, 0, 0) });
            add.Controls.Add(nudSL);
            UI.Btn(add, "Thêm vào giỏ", (s, e) => ThemVaoGio());
            UI.Btn(add, "Xóa dòng", (s, e) => XoaDong());

            var bot = UI.Bar(DockStyle.Bottom);
            bot.Controls.Add(lblTong);
            bot.Controls.Add(lblDiem);
            UI.Btn(bot, "THANH TOÁN", (s, e) => ThanhToan());
            UI.Btn(bot, "Hủy giỏ", (s, e) => { gio.Clear(); Tinh(); });

            dgv.DataSource = gio;
            if (dgv.Columns["MaHD"] != null) dgv.Columns["MaHD"].Visible = false;

            Controls.Add(dgv);
            Controls.Add(add);   // add trước top => top nằm trên cùng
            Controls.Add(top);
            Controls.Add(bot);
            Tinh();
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (!DesignMode) LoadLookups(); }

        /// <summary>Nạp lại danh sách nhân viên & khách hàng (gọi khi chuyển sang tab này).</summary>
        public void LoadLookups()
        {
            try
            {
                cboNV.DataSource = nvBus.GetAll();
                cboNV.DisplayMember = "TenNV"; cboNV.ValueMember = "MaNV";

                var ds = new List<KhachHangDTO> { new KhachHangDTO { MaKH = 0, TenKH = "Khách lẻ" } };
                ds.AddRange(khBus.GetAll());
                cboKH.DataSource = ds;
                cboKH.DisplayMember = "TenKH"; cboKH.ValueMember = "MaKH";
            }
            catch (Exception ex) { UI.Error(ex.Message); }
        }

        int? MaKH => cboKH.SelectedValue is int k && k > 0 ? k : (int?)null;

        void ThemVaoGio()
        {
            string ma = txtMaSP.Text.Trim();
            if (ma.Length == 0) return;
            try
            {
                var sp = spBus.GetById(ma);
                if (sp == null) { UI.Error("Không tìm thấy sản phẩm có mã: " + ma); return; }

                int sl = (int)nudSL.Value;
                var cu = gio.FirstOrDefault(x => string.Equals(x.MaSP, sp.MaSP, StringComparison.OrdinalIgnoreCase));
                int daCo = cu?.SoLuong ?? 0;
                if (daCo + sl > sp.SoLuongTon) { UI.Error($"Không đủ tồn kho. '{sp.TenSP}' chỉ còn {sp.SoLuongTon}."); return; }

                if (cu != null) cu.SoLuong += sl;
                else gio.Add(new ChiTietHoaDonDTO { MaSP = sp.MaSP, TenSP = sp.TenSP, SoLuong = sl, DonGia = sp.GiaBan });

                gio.ResetBindings();
                txtMaSP.Clear(); nudSL.Value = 1; txtMaSP.Focus();
                Tinh();
            }
            catch (Exception ex) { UI.Error(ex.Message); }
        }

        void XoaDong()
        {
            if (dgv.CurrentRow?.DataBoundItem is ChiTietHoaDonDTO ct) { gio.Remove(ct); Tinh(); }
        }

        void Tinh()
        {
            decimal tong = gio.Sum(x => x.ThanhTien);
            lblTong.Text = $"Tổng tiền: {tong:N0} đ";
            lblDiem.Text = MaKH.HasValue ? $"Điểm tích lũy được cộng: {(int)(tong / 100000)}" : "Khách lẻ: không tích điểm";
        }

        void ThanhToan()
        {
            if (!(cboNV.SelectedValue is int maNV)) { UI.Error("Chưa có nhân viên. Hãy thêm nhân viên trước."); return; }
            var hd = new HoaDonDTO { MaNV = maNV, MaKH = MaKH, ChiTiet = gio.ToList() };

            string err = hdBus.ThanhToan(hd, out int maHD, out decimal tong);
            if (err != null) { UI.Error("Thanh toán thất bại, giao dịch đã được hoàn tác (Rollback):\n" + err); return; }

            UI.Info($"Thanh toán thành công!\nHóa đơn #{maHD}\nTổng tiền: {tong:N0} đ");
            gio.Clear(); Tinh();
            LoadLookups();   // cập nhật điểm tích lũy
        }
    }
}
