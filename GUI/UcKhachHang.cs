using System;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class UcKhachHang : UserControl
    {
        readonly KhachHangBUS bus = new KhachHangBUS();
        readonly DataGridView dgv = UI.Grid();
        readonly TextBox txtTen, txtSdt;
        int maKH = 0;

        public UcKhachHang()
        {
            Dock = DockStyle.Fill;
            var bot = UI.Bar(DockStyle.Bottom);
            txtTen = UI.Field(bot, "Tên KH:", 200);
            txtSdt = UI.Field(bot, "Điện thoại:", 120);
            UI.Btn(bot, "Thêm", (s, e) => Do(() => bus.Them(Read())));
            UI.Btn(bot, "Sửa", (s, e) => Do(() => maKH == 0 ? "Chọn khách hàng cần sửa." : bus.Sua(Read())));
            UI.Btn(bot, "Xóa", (s, e) => Xoa());
            UI.Btn(bot, "Làm mới", (s, e) => { maKH = 0; txtTen.Clear(); txtSdt.Clear(); LoadData(); });
            Controls.Add(dgv); Controls.Add(bot);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.DataBoundItem is KhachHangDTO k)
                { maKH = k.MaKH; txtTen.Text = k.TenKH; txtSdt.Text = k.DienThoai; }
            };
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (!DesignMode) LoadData(); }
        void LoadData() { try { dgv.DataSource = bus.GetAll(); } catch (Exception ex) { UI.Error(ex.Message); } }
        KhachHangDTO Read() => new KhachHangDTO { MaKH = maKH, TenKH = txtTen.Text, DienThoai = txtSdt.Text };

        void Do(Func<string> act)
        {
            string err = act();
            if (err != null) UI.Error(err); else { UI.Info("Thành công."); LoadData(); }
        }

        void Xoa()
        {
            if (maKH == 0) { UI.Error("Chọn khách hàng cần xóa."); return; }
            if (!UI.Confirm("Xóa khách hàng này?")) return;
            string err = bus.Xoa(maKH);
            if (err != null) UI.Error(err); else { maKH = 0; txtTen.Clear(); txtSdt.Clear(); LoadData(); }
        }
    }
}
