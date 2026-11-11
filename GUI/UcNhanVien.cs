using System;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class UcNhanVien : UserControl
    {
        readonly NhanVienBUS bus = new NhanVienBUS();
        readonly DataGridView dgv = UI.Grid();
        readonly TextBox txtTen, txtSdt, txtChucVu;
        int maNV = 0;

        public UcNhanVien()
        {
            Dock = DockStyle.Fill;
            var bot = UI.Bar(DockStyle.Bottom);
            txtTen = UI.Field(bot, "Tên NV:", 200);
            txtSdt = UI.Field(bot, "Điện thoại:", 120);
            txtChucVu = UI.Field(bot, "Chức vụ:", 120);
            UI.Btn(bot, "Thêm", (s, e) => Do(() => bus.Them(Read())));
            UI.Btn(bot, "Sửa", (s, e) => Do(() => maNV == 0 ? "Chọn nhân viên cần sửa." : bus.Sua(Read())));
            UI.Btn(bot, "Xóa", (s, e) => Xoa());
            UI.Btn(bot, "Làm mới", (s, e) => { Clear(); LoadData(); });
            Controls.Add(dgv); Controls.Add(bot);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.DataBoundItem is NhanVienDTO n)
                { maNV = n.MaNV; txtTen.Text = n.TenNV; txtSdt.Text = n.SoDienThoai; txtChucVu.Text = n.ChucVu; }
            };
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (!DesignMode) LoadData(); }
        void LoadData() { try { dgv.DataSource = bus.GetAll(); } catch (Exception ex) { UI.Error(ex.Message); } }
        void Clear() { maNV = 0; txtTen.Clear(); txtSdt.Clear(); txtChucVu.Clear(); }
        NhanVienDTO Read() => new NhanVienDTO { MaNV = maNV, TenNV = txtTen.Text, SoDienThoai = txtSdt.Text, ChucVu = txtChucVu.Text };

        void Do(Func<string> act)
        {
            string err = act();
            if (err != null) UI.Error(err); else { UI.Info("Thành công."); LoadData(); }
        }

        void Xoa()
        {
            if (maNV == 0) { UI.Error("Chọn nhân viên cần xóa."); return; }
            if (!UI.Confirm("Xóa nhân viên này?")) return;
            string err = bus.Xoa(maNV);
            if (err != null) UI.Error(err); else { Clear(); LoadData(); }
        }
    }
}
