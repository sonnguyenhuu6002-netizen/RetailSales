using System;
using System.Collections.Generic;
using RetailSales.DTO;
using static RetailSales.DAL.DbHelper;

namespace RetailSales.DAL
{
    public class ThongKeDAL
    {
        /// <summary>Tổng doanh thu trong khoảng [from, to) bằng ExecuteScalar.</summary>
        public decimal DoanhThu(DateTime from, DateTime to)
        {
            object o = Scalar("SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE NgayLap >= @f AND NgayLap < @t",
                              P("@f", from), P("@t", to));
            return Convert.ToDecimal(o);
        }

        /// <summary>Top 5 sản phẩm bán chạy nhất bằng SqlDataReader.</summary>
        public List<TopSanPhamDTO> Top5() => Query(
            @"SELECT TOP 5 sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongSL
              FROM ChiTietHoaDon ct JOIN SanPham sp ON sp.MaSP = ct.MaSP
              GROUP BY sp.MaSP, sp.TenSP
              ORDER BY TongSL DESC",
            r => new TopSanPhamDTO { MaSP = r.GetString(0), TenSP = r.GetString(1), TongSoLuongBan = r.GetInt32(2) });
    }
}
