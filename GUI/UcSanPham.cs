using System;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class UcSanPham : UserControl
    {
        readonly SanPhamBUS bus = new SanPhamBUS();
        readonly DataGridView dgv = UI.Grid();
        readonly TextBox txtTim, txtMa, txtTen, txtGia, txtTon;

        public UcSanPham()
        {
            Dock = DockStyle.Fill;
            var top = UI.Bar(DockStyle.Top);
            txtTim = UI.Field(top, "Tìm theo tên / mã vạch:", 220);
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
            UI.Btn(top, "Tìm", (s, e) => LoadData());

            var bot = UI.Bar(DockStyle.Bottom);
            txtMa = UI.Field(bot, "Mã SP:", 130);
            txtTen = UI.Field(bot, "Tên SP:", 200);
            txtGia = UI.Field(bot, "Giá bán:", 90);
            txtTon = UI.Field(bot, "Tồn kho:", 70);
            UI.Btn(bot, "Thêm", (s, e) => Do(bus.Them));
            UI.Btn(bot, "Sửa", (s, e) => Do(bus.Sua));
            UI.Btn(bot, "Xóa", (s, e) => Xoa());
            UI.Btn(bot, "Làm mới", (s, e) => { txtTim.Clear(); Clear(); LoadData(); });

            Controls.Add(dgv); Controls.Add(top); Controls.Add(bot);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.DataBoundItem is SanPhamDTO p)
                { txtMa.Text = p.MaSP; txtTen.Text = p.TenSP; txtGia.Text = p.GiaBan.ToString("0.##"); txtTon.Text = p.SoLuongTon.ToString(); }
            };
        }

        protected override void OnLoad(EventArgs e) { base.OnLoad(e); if (!DesignMode) LoadData(); }

        void LoadData() { try { dgv.DataSource = bus.Tim(txtTim.Text.Trim()); } catch (Exception ex) { UI.Error(ex.Message); } }
        void Clear() { txtMa.Clear(); txtTen.Clear(); txtGia.Clear(); txtTon.Clear(); }

        SanPhamDTO Read()
        {
            decimal.TryParse(txtGia.Text, out var gia);
            if (!int.TryParse(txtTon.Text, out var ton)) ton = -1;
            return new SanPhamDTO { MaSP = txtMa.Text, TenSP = txtTen.Text.Trim(), GiaBan = gia, SoLuongTon = ton };
        }

        void Do(Func<SanPhamDTO, string> act)
        {
            string err = act(Read());
            if (err != null) UI.Error(err); else { UI.Info("Thành công."); LoadData(); }
        }

        void Xoa()
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { UI.Error("Chọn sản phẩm cần xóa."); return; }
            if (!UI.Confirm($"Xóa sản phẩm {txtMa.Text}?")) return;
            string err = bus.Xoa(txtMa.Text.Trim());
            if (err != null) UI.Error(err); else { Clear(); LoadData(); }
        }
    }
}
