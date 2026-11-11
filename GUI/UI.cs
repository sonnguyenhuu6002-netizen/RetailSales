using System;
using System.Windows.Forms;

namespace RetailSales.GUI
{
    public static class UI
    {
        public static FlowLayoutPanel Bar(DockStyle dock) => new FlowLayoutPanel
        { Dock = dock, AutoSize = true, Padding = new Padding(4), WrapContents = true };

        public static TextBox Field(Control parent, string label, int width = 130)
        {
            parent.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(8, 8, 0, 0) });
            var t = new TextBox { Width = width, Margin = new Padding(3, 5, 0, 3) };
            parent.Controls.Add(t);
            return t;
        }

        public static Button Btn(Control parent, string text, EventHandler onClick)
        {
            var b = new Button { Text = text, AutoSize = true, Margin = new Padding(6, 3, 0, 3) };
            b.Click += onClick;
            parent.Controls.Add(b);
            return b;
        }

        public static DataGridView Grid() => new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, MultiSelect = false, RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoGenerateColumns = true, BackgroundColor = System.Drawing.Color.White
        };

        public static void Info(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        public static void Error(string m) => MessageBox.Show(m, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public static bool Confirm(string m) =>
            MessageBox.Show(m, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
