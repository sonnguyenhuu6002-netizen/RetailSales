using System;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;
using static RetailSales.DAL.DbHelper;

namespace RetailSales.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Lập hóa đơn trong MỘT SqlTransaction (ACID):
        ///  1) INSERT HoaDon  2) INSERT ChiTietHoaDon  3) Trừ tồn kho (không đủ -> Rollback)
        ///  4) Cộng điểm tích lũy  5) Commit.
        /// Đơn giá lấy từ CSDL (không tin dữ liệu từ giao diện).
        /// </summary>
        public int TaoHoaDon(HoaDonDTO hd, out decimal tongTien)
        {
            using (var cn = Open())
            using (var trans = cn.BeginTransaction())
            {
                try
                {
                    // 1. Hóa đơn (tổng tiền cập nhật sau khi có chi tiết)
                    int maHD;
                    using (var cmd = new SqlCommand(
                        "INSERT INTO HoaDon(NgayLap,MaNV,MaKH,TongTien) VALUES(GETDATE(),@nv,@kh,0); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS int);", cn, trans))
                    {
                        cmd.Parameters.Add(P("@nv", hd.MaNV));
                        cmd.Parameters.Add(P("@kh", hd.MaKH));
                        maHD = (int)cmd.ExecuteScalar();
                    }

                    foreach (var ct in hd.ChiTiet)
                    {
                        // 2. Chi tiết hóa đơn (DonGia = GiaBan hiện tại trong DB)
                        int rows;
                        using (var cmd = new SqlCommand(
                            "INSERT INTO ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) " +
                            "SELECT @hd, MaSP, @sl, GiaBan FROM SanPham WHERE MaSP=@ma", cn, trans))
                        {
                            cmd.Parameters.Add(P("@hd", maHD));
                            cmd.Parameters.Add(P("@ma", ct.MaSP));
                            cmd.Parameters.Add(P("@sl", ct.SoLuong));
                            rows = cmd.ExecuteNonQuery();
                        }
                        if (rows == 0)
                            throw new InvalidOperationException($"Sản phẩm '{ct.MaSP}' không tồn tại.");

                        // 3. Trừ tồn kho: chỉ trừ khi SoLuongTon >= SoLuong (an toàn cả khi 2 máy bán cùng lúc)
                        using (var cmd = new SqlCommand(
                            "UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl " +
                            "WHERE MaSP=@ma AND SoLuongTon >= @sl", cn, trans))
                        {
                            cmd.Parameters.Add(P("@ma", ct.MaSP));
                            cmd.Parameters.Add(P("@sl", ct.SoLuong));
                            if (cmd.ExecuteNonQuery() == 0)
                                throw new InvalidOperationException(
                                    $"Sản phẩm '{ct.MaSP}' không đủ tồn kho để bán {ct.SoLuong}.");
                        }
                    }

                    // Tổng tiền tính lại từ chi tiết
                    using (var cmd = new SqlCommand(
                        "UPDATE HoaDon SET TongTien = (SELECT SUM(SoLuong*DonGia) FROM ChiTietHoaDon WHERE MaHD=@hd) " +
                        "WHERE MaHD=@hd; SELECT TongTien FROM HoaDon WHERE MaHD=@hd;", cn, trans))
                    {
                        cmd.Parameters.Add(P("@hd", maHD));
                        tongTien = (decimal)cmd.ExecuteScalar();
                    }

                    // 4. Cộng điểm tích lũy: DiemTichLuy += TongTien / 100000 (lấy phần nguyên)
                    if (hd.MaKH.HasValue)
                    {
                        using (var cmd = new SqlCommand(
                            "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + CAST(FLOOR(@tong/100000) AS int) " +
                            "WHERE MaKH=@kh", cn, trans))
                        {
                            cmd.Parameters.Add(P("@tong", tongTien));
                            cmd.Parameters.Add(P("@kh", hd.MaKH.Value));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 5. Hoàn tất
                    trans.Commit();
                    return maHD;
                }
                catch
                {
                    try { trans.Rollback(); } catch { /* giao dịch đã bị hủy */ }
                    throw;
                }
            }
        }
    }
}
