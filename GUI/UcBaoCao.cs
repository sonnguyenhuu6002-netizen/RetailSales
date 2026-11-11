using System;
using System.Windows.Forms;
using RetailSales.BUS;

namespace RetailSales.GUI
{
    public class UcBaoCao : UserControl
    {
        readonly ThongKeBUS bus = new ThongKeBUS();
        readonly DateTimePicker dtp = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
        readonly Label lbl = new Label { AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold), Margin = new Padding(12, 6, 0, 0) };
        readonly DataGridView dgv = UI.Grid();

        public UcBaoCao()
        {
            Dock = DockStyle.Fill;
            var top = UI.Bar(DockStyle.Top);
            top.Controls.Add(new Label { Text = "Chọn ngày:", AutoSize = true, Margin = new Padding(6, 8, 0, 0) });
            top.Controls.Add(dtp);
            UI.Btn(top, "Doanh thu ngày", (s, e) => Run(() => lbl.Text = $"Doanh thu ngày {dtp.Value:dd/MM/yyyy}: {bus.DoanhThuNgay(dtp.Value):N0} đ"));
            UI.Btn(top, "Doanh thu tháng", (s, e) => Run(() => lbl.Text = $"Doanh thu tháng {dtp.Value:MM/yyyy}: {bus.DoanhThuThang(dtp.Value):N0} đ"));
            UI.Btn(top, "Top 5 bán chạy", (s, e) => Run(() => dgv.DataSource = bus.Top5()));
            top.Controls.Add(lbl);
            Controls.Add(dgv); Controls.Add(top);
        }

        static void Run(Action a) { try { a(); } catch (Exception ex) { UI.Error(ex.Message); } }
    }
}
