using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Models;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Ban> Bans => Set<Ban>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
    public DbSet<LoaiSP> LoaiSPs => Set<LoaiSP>();
    public DbSet<Luong> Luongs => Set<Luong>();
    public DbSet<NguyenLieu> NguyenLieus => Set<NguyenLieu>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PhieuNhap> PhieuNhaps => Set<PhieuNhap>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<CTHD> CTHDs => Set<CTHD>();
    public DbSet<CTPN> CTPNs => Set<CTPN>();
    public DbSet<CongThuc> CongThucs => Set<CongThuc>();
    public DbSet<ChamCong> ChamCongs => Set<ChamCong>();
    public DbSet<LoginResultViewModel> LoginResults => Set<LoginResultViewModel>();
    public DbSet<SanPhamViewModel> SanPhamResults => Set<SanPhamViewModel>();
    public DbSet<NhanVienViewModel> NhanVienResults => Set<NhanVienViewModel>();
    public DbSet<TaiKhoanListViewModel> TaiKhoanListResults => Set<TaiKhoanListViewModel>();
    public DbSet<TaiKhoanDetailViewModel> TaiKhoanDetailResults => Set<TaiKhoanDetailViewModel>();
    public DbSet<HoaDonViewModel> HoaDonResults => Set<HoaDonViewModel>();
    public DbSet<CTHDViewModel> CTHDResults => Set<CTHDViewModel>();
    public DbSet<PhieuNhapViewModel> PhieuNhapResults => Set<PhieuNhapViewModel>();
    public DbSet<CTPNViewModel> CTPNResults => Set<CTPNViewModel>();
    public DbSet<CongThucViewModel> CongThucResults => Set<CongThucViewModel>();
    public DbSet<DoanhThuTheoNgayViewModel> DoanhThuTheoNgayResults =>
        Set<DoanhThuTheoNgayViewModel>();
    public DbSet<SanPhamBanChayViewModel> SanPhamBanChayResults => Set<SanPhamBanChayViewModel>();
    public DbSet<NhapHangTheoThangViewModel> NhapHangTheoThangResults =>
        Set<NhapHangTheoThangViewModel>();
    public DbSet<OrderChamCongStatusViewModel> OrderChamCongStatusResults =>
        Set<OrderChamCongStatusViewModel>();
    public DbSet<ChamCongDangLamViewModel> ChamCongDangLamResults =>
        Set<ChamCongDangLamViewModel>();
    public DbSet<ChamCongCheckoutResultViewModel> ChamCongCheckoutResults =>
        Set<ChamCongCheckoutResultViewModel>();
    public DbSet<ChamCongTongLuongTatCaThangViewModel> ChamCongTongLuongTatCaThangResults =>
        Set<ChamCongTongLuongTatCaThangViewModel>();
    public DbSet<ChamCongTongLuongNhanVienThangViewModel> ChamCongTongLuongNhanVienThangResults =>
        Set<ChamCongTongLuongNhanVienThangViewModel>();
    public DbSet<ChamCongCaLamThangViewModel> ChamCongCaLamThangResults =>
        Set<ChamCongCaLamThangViewModel>();
    public DbSet<AuditLogViewModel> AuditLogResults => Set<AuditLogViewModel>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ban>().ToTable("Ban");
        b.Entity<KhachHang>().ToTable("KhachHang");
        b.Entity<KhuyenMai>().ToTable("KhuyenMai");
        b.Entity<LoaiSP>().ToTable("LoaiSP");
        b.Entity<Luong>().ToTable("Luong");
        b.Entity<NguyenLieu>().ToTable("NguyenLieu");
        b.Entity<NhanVien>().ToTable("NhanVien");
        b.Entity<SanPham>().ToTable("SanPham");
        b.Entity<SanPham>().Property(x => x.TrangThai).HasMaxLength(30);
        b.Entity<HoaDon>().ToTable("HoaDon");
        b.Entity<HoaDon>().Property(x => x.TongTien).HasColumnType("money");
        b.Entity<HoaDon>().Property(x => x.TrangThaiOrder).HasMaxLength(30);
        b.Entity<HoaDon>().Property(x => x.TrangThaiThanhToan).HasMaxLength(30);
        b.Entity<HoaDon>().Property(x => x.PhuongThucThanhToan).HasMaxLength(30);
        b.Entity<HoaDon>().Property(x => x.ClientRequestId).HasMaxLength(36).IsUnicode(false);
        b.Entity<HoaDon>().HasIndex(x => x.ClientRequestId).IsUnique();
        b.Entity<ThanhToan>().ToTable("ThanhToan");
        b.Entity<ThanhToan>().Property(x => x.SoTien).HasColumnType("money");
        b.Entity<ThanhToan>().Property(x => x.MaPhienThanhToan).HasMaxLength(50);
        b.Entity<ThanhToan>().Property(x => x.PhuongThuc).HasMaxLength(30);
        b.Entity<ThanhToan>().Property(x => x.TrangThai).HasMaxLength(30);
        b.Entity<AuditLog>().ToTable("AuditLog");
        b.Entity<AuditLog>().Property(x => x.MaNV).HasMaxLength(50);
        b.Entity<AuditLog>().Property(x => x.TenDangNhap).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.VaiTro).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.ChucNang).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.HanhDong).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.DoiTuong).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.MaDoiTuong).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.NoiDung).HasMaxLength(1000);
        b.Entity<AuditLog>().Property(x => x.DiaChiIP).HasMaxLength(100);
        b.Entity<AuditLog>().Property(x => x.UserAgent).HasMaxLength(500);
        b.Entity<PhieuNhap>().ToTable("PhieuNhap");
        b.Entity<TaiKhoan>().ToTable("TaiKhoan");
        b.Entity<CTHD>().ToTable("CTHD");
        b.Entity<CTPN>().ToTable("CTPN");
        b.Entity<CongThuc>().ToTable("CongThuc");
        b.Entity<ChamCong>().ToTable("ChamCong");
        b.Entity<CTHD>().HasKey(x => new { x.MaHD, x.MaSP });
        b.Entity<CTPN>().HasKey(x => new { x.MaPN, x.MaNL });
        b.Entity<CongThuc>().HasKey(x => new { x.MaSP, x.MaNL });
        b.Entity<NhanVien>().Property(x => x.LuongTheoGio).HasColumnType("money");
        b.Entity<Luong>()
            .Property(x => x.TongLuong)
            .HasColumnType("money")
            .ValueGeneratedOnAddOrUpdate()
            .HasComputedColumnSql(
                "[LuongCoBan] + ISNULL([PhuCap], 0) + ISNULL([Thuong], 0) - ISNULL([Phat], 0)",
                true
            );
        b.Entity<CTHD>()
            .Property(x => x.ThanhTien)
            .HasColumnType("money")
            .ValueGeneratedOnAddOrUpdate()
            .HasComputedColumnSql("[SoLuong] * [DonGia]", true);
        b.Entity<CTPN>()
            .Property(x => x.ThanhTien)
            .HasColumnType("money")
            .ValueGeneratedOnAddOrUpdate()
            .HasComputedColumnSql("[SoLuong] * [DonGia]", true);
        b.Entity<ChamCong>().Property(x => x.DonGiaGio).HasColumnType("money");
        b.Entity<ChamCong>().Property(x => x.TongGio).HasColumnType("decimal(10,2)");
        b.Entity<ChamCong>().Property(x => x.TienCong).HasColumnType("money");
        b.Entity<OrderChamCongStatusViewModel>().Property(x => x.DonGiaGio).HasColumnType("money");
        b.Entity<OrderChamCongStatusViewModel>()
            .Property(x => x.TongGioTamTinh)
            .HasColumnType("decimal(10,2)");
        b.Entity<OrderChamCongStatusViewModel>()
            .Property(x => x.TienCongTamTinh)
            .HasColumnType("money");
        foreach (
            var e in new[]
            {
                typeof(LoginResultViewModel),
                typeof(SanPhamViewModel),
                typeof(NhanVienViewModel),
                typeof(TaiKhoanListViewModel),
                typeof(TaiKhoanDetailViewModel),
                typeof(HoaDonViewModel),
                typeof(CTHDViewModel),
                typeof(PhieuNhapViewModel),
                typeof(CTPNViewModel),
                typeof(CongThucViewModel),
                typeof(DoanhThuTheoNgayViewModel),
                typeof(SanPhamBanChayViewModel),
                typeof(NhapHangTheoThangViewModel),
                typeof(OrderChamCongStatusViewModel),
                typeof(ChamCongDangLamViewModel),
                typeof(ChamCongCheckoutResultViewModel),
                typeof(ChamCongTongLuongTatCaThangViewModel),
                typeof(ChamCongTongLuongNhanVienThangViewModel),
                typeof(ChamCongCaLamThangViewModel),
                typeof(AuditLogViewModel),
            }
        )
            b.Entity(e).HasNoKey();
        b.Entity<NhanVien>().HasOne<Luong>().WithMany().HasForeignKey(x => x.MaLuong);
        b.Entity<SanPham>().HasOne<LoaiSP>().WithMany().HasForeignKey(x => x.MaLoaiSP);
        b.Entity<HoaDon>().HasOne<NhanVien>().WithMany().HasForeignKey(x => x.MaNV);
        b.Entity<HoaDon>()
            .HasOne<KhachHang>()
            .WithMany()
            .HasForeignKey(x => x.MaKH)
            .IsRequired(false);
        b.Entity<HoaDon>().HasOne<Ban>().WithMany().HasForeignKey(x => x.MaBan);
        b.Entity<ThanhToan>().HasOne<HoaDon>().WithMany().HasForeignKey(x => x.MaHD).IsRequired(false);
        b.Entity<PhieuNhap>().HasOne<NhanVien>().WithMany().HasForeignKey(x => x.MaNV);
        b.Entity<TaiKhoan>().HasOne<NhanVien>().WithMany().HasForeignKey(x => x.MaNV);
        b.Entity<CTHD>().HasOne<HoaDon>().WithMany().HasForeignKey(x => x.MaHD);
        b.Entity<CTHD>().HasOne<SanPham>().WithMany().HasForeignKey(x => x.MaSP);
        b.Entity<CTHD>()
            .HasOne<KhuyenMai>()
            .WithMany()
            .HasForeignKey(x => x.MaKM)
            .IsRequired(false);
        b.Entity<CTPN>().HasOne<PhieuNhap>().WithMany().HasForeignKey(x => x.MaPN);
        b.Entity<CTPN>().HasOne<NguyenLieu>().WithMany().HasForeignKey(x => x.MaNL);
        b.Entity<CongThuc>().HasOne<SanPham>().WithMany().HasForeignKey(x => x.MaSP);
        b.Entity<CongThuc>().HasOne<NguyenLieu>().WithMany().HasForeignKey(x => x.MaNL);
        b.Entity<ChamCong>().HasOne<NhanVien>().WithMany().HasForeignKey(x => x.MaNV);
    }
}
