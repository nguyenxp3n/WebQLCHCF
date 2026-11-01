using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Data;
using WebQLCHCF.Models;
using WebQLCHCF.Tests.TestHarness;
using Xunit;

namespace WebQLCHCF.Tests.Domain;

public class EntityPersistenceIntegrationTests
{
    [Fact]
    public void AppDbContext_ModelBuilding_ContainsAll17Entities()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var model = context.Model;

        var expectedEntities = new[]
        {
            typeof(AuditLog),
            typeof(Ban),
            typeof(CTHD),
            typeof(CTPN),
            typeof(ChamCong),
            typeof(CongThuc),
            typeof(HoaDon),
            typeof(KhachHang),
            typeof(KhuyenMai),
            typeof(LoaiSP),
            typeof(Luong),
            typeof(NguyenLieu),
            typeof(NhanVien),
            typeof(PhieuNhap),
            typeof(SanPham),
            typeof(TaiKhoan),
            typeof(ThanhToan)
        };

        foreach (var entityType in expectedEntities)
        {
            var entity = model.FindEntityType(entityType);
            Assert.NotNull(entity);
        }
    }

    [Fact]
    public void AppDbContext_CompositeKeys_AreConfiguredCorrectly()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var model = context.Model;

        // CTHD: { MaHD, MaSP }
        var cthdEntity = model.FindEntityType(typeof(CTHD))!;
        var cthdPk = cthdEntity.FindPrimaryKey()!;
        Assert.Equal(2, cthdPk.Properties.Count);
        Assert.Contains(cthdPk.Properties, p => p.Name == nameof(CTHD.MaHD));
        Assert.Contains(cthdPk.Properties, p => p.Name == nameof(CTHD.MaSP));

        // CTPN: { MaPN, MaNL }
        var ctpnEntity = model.FindEntityType(typeof(CTPN))!;
        var ctpnPk = ctpnEntity.FindPrimaryKey()!;
        Assert.Equal(2, ctpnPk.Properties.Count);
        Assert.Contains(ctpnPk.Properties, p => p.Name == nameof(CTPN.MaPN));
        Assert.Contains(ctpnPk.Properties, p => p.Name == nameof(CTPN.MaNL));

        // CongThuc: { MaSP, MaNL }
        var congThucEntity = model.FindEntityType(typeof(CongThuc))!;
        var congThucPk = congThucEntity.FindPrimaryKey()!;
        Assert.Equal(2, congThucPk.Properties.Count);
        Assert.Contains(congThucPk.Properties, p => p.Name == nameof(CongThuc.MaSP));
        Assert.Contains(congThucPk.Properties, p => p.Name == nameof(CongThuc.MaNL));
    }

    [Fact]
    public void AppDbContext_SingleKeys_AreConfiguredCorrectly()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var model = context.Model;

        Assert.Equal(nameof(AuditLog.MaLog), model.FindEntityType(typeof(AuditLog))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(Ban.MaBan), model.FindEntityType(typeof(Ban))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(ChamCong.MaCC), model.FindEntityType(typeof(ChamCong))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(HoaDon.MaHD), model.FindEntityType(typeof(HoaDon))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(KhachHang.MaKH), model.FindEntityType(typeof(KhachHang))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(KhuyenMai.MaKM), model.FindEntityType(typeof(KhuyenMai))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(LoaiSP.MaLoaiSP), model.FindEntityType(typeof(LoaiSP))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(Luong.MaLuong), model.FindEntityType(typeof(Luong))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(NguyenLieu.MaNL), model.FindEntityType(typeof(NguyenLieu))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(NhanVien.MaNV), model.FindEntityType(typeof(NhanVien))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(PhieuNhap.MaPN), model.FindEntityType(typeof(PhieuNhap))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(SanPham.MaSP), model.FindEntityType(typeof(SanPham))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(TaiKhoan.MaTK), model.FindEntityType(typeof(TaiKhoan))!.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(nameof(ThanhToan.MaThanhToan), model.FindEntityType(typeof(ThanhToan))!.FindPrimaryKey()!.Properties.Single().Name);
    }

    [Fact]
    public void AppDbContext_ForeignKeys_AreConfiguredCorrectly()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();
        var model = context.Model;

        var sanPhamFks = model.FindEntityType(typeof(SanPham))!.GetForeignKeys();
        Assert.Contains(sanPhamFks, fk => fk.PrincipalEntityType.ClrType == typeof(LoaiSP));

        var nhanVienFks = model.FindEntityType(typeof(NhanVien))!.GetForeignKeys();
        Assert.Contains(nhanVienFks, fk => fk.PrincipalEntityType.ClrType == typeof(Luong));

        var taiKhoanFks = model.FindEntityType(typeof(TaiKhoan))!.GetForeignKeys();
        Assert.Contains(taiKhoanFks, fk => fk.PrincipalEntityType.ClrType == typeof(NhanVien));

        var hoaDonFks = model.FindEntityType(typeof(HoaDon))!.GetForeignKeys();
        Assert.Contains(hoaDonFks, fk => fk.PrincipalEntityType.ClrType == typeof(NhanVien));
        Assert.Contains(hoaDonFks, fk => fk.PrincipalEntityType.ClrType == typeof(Ban));
        Assert.Contains(hoaDonFks, fk => fk.PrincipalEntityType.ClrType == typeof(KhachHang));

        var cthdFks = model.FindEntityType(typeof(CTHD))!.GetForeignKeys();
        Assert.Contains(cthdFks, fk => fk.PrincipalEntityType.ClrType == typeof(HoaDon));
        Assert.Contains(cthdFks, fk => fk.PrincipalEntityType.ClrType == typeof(SanPham));

        var ctpnFks = model.FindEntityType(typeof(CTPN))!.GetForeignKeys();
        Assert.Contains(ctpnFks, fk => fk.PrincipalEntityType.ClrType == typeof(PhieuNhap));
        Assert.Contains(ctpnFks, fk => fk.PrincipalEntityType.ClrType == typeof(NguyenLieu));

        var congThucFks = model.FindEntityType(typeof(CongThuc))!.GetForeignKeys();
        Assert.Contains(congThucFks, fk => fk.PrincipalEntityType.ClrType == typeof(SanPham));
        Assert.Contains(congThucFks, fk => fk.PrincipalEntityType.ClrType == typeof(NguyenLieu));
    }

    [Fact]
    public async Task AppDbContext_CrudPersistence_SucceedsForRootAndCompositeEntities()
    {
        using var context = TestControllerContextFactory.CreateInMemoryDbContext();

        // 1. Create root master data
        var luong = new Luong { MaLuong = 10, Thang = 10, Nam = 2026, LuongCoBan = 12000000 };
        var nhanVien = new NhanVien { MaNV = "NV99", TenNV = "Nguyễn Văn Test", GioiTinh = "Nam", SDT = "0909999999", ChucVu = "Phục Vụ", MaLuong = 10 };
        var loai = new LoaiSP { MaLoaiSP = "L99", TenLoaiSP = "Đá Xay" };
        var sanPham = new SanPham { MaSP = "SP99", TenSP = "Caramel Freeze", DonGia = 45000, MaLoaiSP = "L99", TrangThai = "Đang bán" };
        var nguyenLieu = new NguyenLieu { MaNL = "NL99", TenNL = "Syrup Caramel", DonViTinh = "Chai", GiaNhap = 180000, MucTonToiThieu = 2, NgayCapNhat = DateTime.Today };
        var ban = new Ban { MaBan = "B99", TenBan = "Bàn 99", KhuVuc = "Tầng trệt", SoBan = 99, TrangThai = "Trống" };
        var khachHang = new KhachHang { MaKH = "KH99", TenKH = "VIP Khách Hàng", SDT = "0988888888", DiemTichLuy = 100 };
        var khuyenMai = new KhuyenMai { MaKM = "KM99", TenKM = "Tri Ân", LoaiKM = "Phần trăm", GiaTri = 20, NgayBatDau = DateTime.Today, NgayKetThuc = DateTime.Today.AddDays(10), DieuKienApDung = "Mọi đơn", TrangThai = "Đang áp dụng" };

        context.Luongs.Add(luong);
        context.NhanViens.Add(nhanVien);
        context.LoaiSPs.Add(loai);
        context.SanPhams.Add(sanPham);
        context.NguyenLieus.Add(nguyenLieu);
        context.Bans.Add(ban);
        context.KhachHangs.Add(khachHang);
        context.KhuyenMais.Add(khuyenMai);
        await context.SaveChangesAsync();

        // 2. Create composite entity: CongThuc
        var congThuc = new CongThuc { MaSP = "SP99", MaNL = "NL99", DinhLuong = 30 };
        context.CongThucs.Add(congThuc);
        await context.SaveChangesAsync();

        // Query CongThuc by composite key
        var savedRecipe = await context.CongThucs.FindAsync("SP99", "NL99");
        Assert.NotNull(savedRecipe);
        Assert.Equal(30, savedRecipe.DinhLuong);

        // 3. Create invoice and composite entity: CTHD
        var hoaDon = new HoaDon
        {
            MaHD = "HD99999",
            NgayLap = DateTime.Today,
            MaNV = "NV99",
            MaBan = "B99",
            MaKH = "KH99",
            TrangThaiOrder = "ChoThanhToan",
            TrangThaiThanhToan = "ChuaThanhToan",
            ThoiGianTao = DateTime.Now
        };
        context.HoaDons.Add(hoaDon);
        await context.SaveChangesAsync();

        var cthd = new CTHD
        {
            MaHD = "HD99999",
            MaSP = "SP99",
            SoLuong = 3,
            DonGia = 45000,
            MaKM = "KM99"
        };
        context.CTHDs.Add(cthd);
        await context.SaveChangesAsync();

        // Query CTHD by composite key
        var savedCthd = await context.CTHDs.FindAsync("HD99999", "SP99");
        Assert.NotNull(savedCthd);
        Assert.Equal(3, savedCthd.SoLuong);

        // 4. Update CTHD
        savedCthd.SoLuong = 5;
        await context.SaveChangesAsync();

        var updatedCthd = await context.CTHDs.FindAsync("HD99999", "SP99");
        Assert.Equal(5, updatedCthd!.SoLuong);

        // 5. Delete CTHD
        context.CTHDs.Remove(updatedCthd);
        await context.SaveChangesAsync();

        var deletedCthd = await context.CTHDs.FindAsync("HD99999", "SP99");
        Assert.Null(deletedCthd);
    }
}
