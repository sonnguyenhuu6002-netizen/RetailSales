using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;
using static RetailSales.DAL.DbHelper;

namespace RetailSales.DAL
{
    public class SanPhamDAL
    {
        static SanPhamDTO Map(SqlDataReader r) => new SanPhamDTO
        {
            MaSP = r.GetString(0), TenSP = r.GetString(1),
            GiaBan = r.GetDecimal(2), SoLuongTon = r.GetInt32(3)
        };

        const string Select = "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham ";

        /// <summary>Tìm theo tên hoặc mã vạch, ký tự đại diện % nằm trong giá trị tham số.</summary>
        public List<SanPhamDTO> Search(string keyword) =>
            Query(Select + "WHERE TenSP LIKE @kw OR MaSP LIKE @kw ORDER BY TenSP", Map,
                  P("@kw", "%" + keyword + "%"));

        public SanPhamDTO GetById(string maSP)
        {
            var l = Query(Select + "WHERE MaSP = @ma", Map, P("@ma", maSP));
            return l.Count > 0 ? l[0] : null;
        }

        public int Insert(SanPhamDTO s) => Exec(
            "INSERT INTO SanPham(MaSP,TenSP,GiaBan,SoLuongTon) VALUES(@ma,@ten,@gia,@ton)",
            P("@ma", s.MaSP), P("@ten", s.TenSP), P("@gia", s.GiaBan), P("@ton", s.SoLuongTon));

        public int Update(SanPhamDTO s) => Exec(
            "UPDATE SanPham SET TenSP=@ten, GiaBan=@gia, SoLuongTon=@ton WHERE MaSP=@ma",
            P("@ma", s.MaSP), P("@ten", s.TenSP), P("@gia", s.GiaBan), P("@ton", s.SoLuongTon));

        public int Delete(string maSP) => Exec("DELETE FROM SanPham WHERE MaSP=@ma", P("@ma", maSP));
    }

    public class KhachHangDAL
    {
        static KhachHangDTO Map(SqlDataReader r) => new KhachHangDTO
        {
            MaKH = r.GetInt32(0), TenKH = r.GetString(1),
            DienThoai = Str(r, 2), DiemTichLuy = r.GetInt32(3)
        };

        public List<KhachHangDTO> GetAll() =>
            Query("SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang ORDER BY TenKH", Map);

        public int Insert(KhachHangDTO k) => Exec(
            "INSERT INTO KhachHang(TenKH,DienThoai) VALUES(@ten,@sdt)",
            P("@ten", k.TenKH), P("@sdt", k.DienThoai));

        public int Update(KhachHangDTO k) => Exec(
            "UPDATE KhachHang SET TenKH=@ten, DienThoai=@sdt WHERE MaKH=@ma",
            P("@ma", k.MaKH), P("@ten", k.TenKH), P("@sdt", k.DienThoai));

        public int Delete(int maKH) => Exec("DELETE FROM KhachHang WHERE MaKH=@ma", P("@ma", maKH));
    }

    public class NhanVienDAL
    {
        static NhanVienDTO Map(SqlDataReader r) => new NhanVienDTO
        {
            MaNV = r.GetInt32(0), TenNV = r.GetString(1),
            SoDienThoai = Str(r, 2), ChucVu = Str(r, 3)
        };

        public List<NhanVienDTO> GetAll() =>
            Query("SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien ORDER BY TenNV", Map);

        public int Insert(NhanVienDTO n) => Exec(
            "INSERT INTO NhanVien(TenNV,SoDienThoai,ChucVu) VALUES(@ten,@sdt,@cv)",
            P("@ten", n.TenNV), P("@sdt", n.SoDienThoai), P("@cv", n.ChucVu));

        public int Update(NhanVienDTO n) => Exec(
            "UPDATE NhanVien SET TenNV=@ten, SoDienThoai=@sdt, ChucVu=@cv WHERE MaNV=@ma",
            P("@ma", n.MaNV), P("@ten", n.TenNV), P("@sdt", n.SoDienThoai), P("@cv", n.ChucVu));

        public int Delete(int maNV) => Exec("DELETE FROM NhanVien WHERE MaNV=@ma", P("@ma", maNV));
    }
}
