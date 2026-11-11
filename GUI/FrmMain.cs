using System.Windows.Forms;

namespace RetailSales.GUI
{
    public class FrmMain : Form
    {
        public FrmMain()
        {
            Text = "Hệ thống quản lý bán hàng - Siêu thị mini";
            Width = 1100; Height = 700;
            StartPosition = FormStartPosition.CenterScreen;

            var banHang = new UcBanHang();
            var tabs = new TabControl { Dock = DockStyle.Fill };
            void AddTab(string title, Control c) { var t = new TabPage(title); t.Controls.Add(c); tabs.TabPages.Add(t); }

            AddTab("Bán hàng", banHang);
            AddTab("Sản phẩm", new UcSanPham());
            AddTab("Khách hàng", new UcKhachHang());
            AddTab("Nhân viên", new UcNhanVien());
            AddTab("Báo cáo - Thống kê", new UcBaoCao());
            tabs.SelectedIndexChanged += (s, e) => { if (tabs.SelectedIndex == 0) banHang.LoadLookups(); };
            Controls.Add(tabs);
        }
    }
}
