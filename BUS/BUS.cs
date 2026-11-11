using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    /// <summary>Chạy thao tác DAL, trả null nếu OK hoặc chuỗi lỗi thân thiện.</summary>
    static class BusHelper
    {
        public static string Run(Action act)
        {
            try { act(); return null; }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            { return "Dữ liệu bị trùng (mã hoặc số điện thoại đã tồn tại)."; }
            catch (SqlException ex) when (ex.Number == 547)
            { return "Không thể thực hiện: dữ liệu đang được sử dụng ở bảng khác (đã phát sinh giao dịch) hoặc vi phạm ràng buộc."; }
            catch (Exception ex) { return ex.Message; }
        }

        public static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public class SanPhamBUS
    {
        readonly SanPhamDAL dal = new SanPhamDAL();
        public List<SanPhamDTO> Tim(string kw) => dal.Search(kw ?? "");
        public SanPhamDTO GetById(string ma) => dal.GetById(ma.Trim());

        string Validate(SanPhamDTO s)
        {
            if (string.IsNullOrWhiteSpace(s.MaSP)) return "Mã sản phẩm không được để trống.";
            if (s.MaSP.Trim().Length > 20) return "Mã sản phẩm tối đa 20 ký tự.";
            if (string.IsNullOrWhiteSpace(s.TenSP)) return "Tên sản phẩm không được để trống.";
            if (s.GiaBan <= 0) return "Giá bán phải lớn hơn 0.";
            if (s.SoLuongTon < 0) return "Số lượng tồn phải >= 0.";
            return null;
        }

        public string Them(SanPhamDTO s) { s.MaSP = s.MaSP?.Trim(); return Validate(s) ?? BusHelper.Run(() => dal.Insert(s)); }
        public string Sua(SanPhamDTO s) { s.MaSP = s.MaSP?.Trim(); return Validate(s) ?? BusHelper.Run(() => dal.Update(s)); }
        public string Xoa(string ma) => BusHelper.Run(() => dal.Delete(ma));
    }

    public class KhachHangBUS
    {
        readonly KhachHangDAL dal = new KhachHangDAL();
        public List<KhachHangDTO> GetAll() => dal.GetAll();

        string Validate(KhachHangDTO k)
        {
            if (string.IsNullOrWhiteSpace(k.TenKH)) return "Tên khách hàng không được để trống.";
            k.TenKH = k.TenKH.Trim();
            k.DienThoai = BusHelper.NullIfEmpty(k.DienThoai);
            return null;
        }

        public string Them(KhachHangDTO k) => Validate(k) ?? BusHelper.Run(() => dal.Insert(k));
        public string Sua(KhachHangDTO k) => Validate(k) ?? BusHelper.Run(() => dal.Update(k));
        public string Xoa(int ma) => BusHelper.Run(() => dal.Delete(ma));
    }

    public class NhanVienBUS
    {
        readonly NhanVienDAL dal = new NhanVienDAL();
        public List<NhanVienDTO> GetAll() => dal.GetAll();

        string Validate(NhanVienDTO n)
        {
            if (string.IsNullOrWhiteSpace(n.TenNV)) return "Tên nhân viên không được để trống.";
            n.TenNV = n.TenNV.Trim();
            n.SoDienThoai = BusHelper.NullIfEmpty(n.SoDienThoai);
            n.ChucVu = BusHelper.NullIfEmpty(n.ChucVu);
            return null;
        }

        public string Them(NhanVienDTO n) => Validate(n) ?? BusHelper.Run(() => dal.Insert(n));
        public string Sua(NhanVienDTO n) => Validate(n) ?? BusHelper.Run(() => dal.Update(n));
        public string Xoa(int ma) => BusHelper.Run(() => dal.Delete(ma));
    }

    public class HoaDonBUS
    {
        readonly HoaDonDAL dal = new HoaDonDAL();

        /// <summary>Thanh toán. Trả null nếu thành công; ngược lại trả thông báo lỗi (giao dịch đã Rollback).</summary>
        public string ThanhToan(HoaDonDTO hd, out int maHD, out decimal tongTien)
        {
            maHD = 0; tongTien = 0;
            if (hd.MaNV <= 0) return "Vui lòng chọn nhân viên bán hàng.";
            if (hd.ChiTiet == null || hd.ChiTiet.Count == 0) return "Giỏ hàng đang trống.";
            if (hd.ChiTiet.Any(c => c.SoLuong <= 0)) return "Số lượng phải lớn hơn 0.";

            // Gộp dòng trùng mã (khóa chính ChiTietHoaDon = MaHD + MaSP)
            hd.ChiTiet = hd.ChiTiet.GroupBy(c => c.MaSP, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ChiTietHoaDonDTO { MaSP = g.First().MaSP, SoLuong = g.Sum(x => x.SoLuong) })
                .ToList();

            int id = 0; decimal tong = 0;
            string err = BusHelper.Run(() => id = dal.TaoHoaDon(hd, out tong));
            maHD = id; tongTien = tong;
            return err;
        }
    }

    public class ThongKeBUS
    {
        readonly ThongKeDAL dal = new ThongKeDAL();
        public decimal DoanhThuNgay(DateTime d) => dal.DoanhThu(d.Date, d.Date.AddDays(1));
        public decimal DoanhThuThang(DateTime d)
        {
            var from = new DateTime(d.Year, d.Month, 1);
            return dal.DoanhThu(from, from.AddMonths(1));
        }
        public List<TopSanPhamDTO> Top5() => dal.Top5();
    }
}
