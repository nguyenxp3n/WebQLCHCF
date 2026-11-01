using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebQLCHCF.Data;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public class OrderService : IOrderService
{
    private const string TrangThaiDangBan = "Đang bán";
    private const string TrangThaiKhuyenMaiDangApDung = "Đang áp dụng";
    private const string LoaiKhuyenMaiTienMat = "Tiền mặt";
    private const string LoaiKhuyenMaiPhanTram = "Phần trăm";
    private const string VietQrBankCode = "MB";
    private const string VietQrAccountNo = "0332135258";
    private const string VietQrAccountName = "HOANG CHIEM NGUYEN";

    private readonly AppDbContext _context;

    public OrderService(AppDbContext context) => _context = context;

    public async Task<OrderCreateViewModel> GetCreateModelAsync()
    {
        return await GetCreateModelAsync(new OrderCreateViewModel());
    }

    public async Task<OrderCreateViewModel> GetCreateModelForEditAsync(string maHD)
    {
        var hoaDon = await _context.HoaDons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHD == maHD);

        if (hoaDon == null)
            throw new InvalidOperationException("Không tìm thấy hóa đơn cần sửa.");

        if (!TableWorkflowManager.CanEditOrder(hoaDon.TrangThaiOrder, hoaDon.TrangThaiThanhToan, out var reason))
            throw new InvalidOperationException(reason);

        var customer = string.IsNullOrWhiteSpace(hoaDon.MaKH)
            ? null
            : await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(x => x.MaKH == hoaDon.MaKH);

        var model = await GetCreateModelAsync(new OrderCreateViewModel
        {
            EditMaHD = hoaDon.MaHD,
            HinhThuc = hoaDon.MaBan == OrderWorkflowConstants.MaBanMangVe
                ? OrderWorkflowConstants.HinhThucMangVe
                : OrderWorkflowConstants.HinhThucTaiQuan,
            MaKH = hoaDon.MaKH,
            TenKH = customer?.TenKH,
            SDTKhachHang = customer?.SDT,
            GhiChu = hoaDon.GhiChu,
        });

        var detailMap = await _context.CTHDResults
            .FromSqlInterpolated($"EXEC CTHD_DSTheoMaHD {maHD}")
            .AsNoTracking()
            .ToDictionaryAsync(x => x.MaSP);

        foreach (var item in model.Items)
        {
            if (!detailMap.TryGetValue(item.MaSP, out var detail))
                continue;

            item.SoLuong = detail.SoLuong;
            item.MaKM = detail.MaKM;
            item.TenKM = detail.TenKM;
            item.ThanhTien = item.DonGia * item.SoLuong;
        }

        var currentTable = await _context.Bans.AsNoTracking()
            .Where(x => x.MaBan == hoaDon.MaBan)
            .Select(x => new OrderTableViewModel
            {
                MaBan = x.MaBan,
                TenBan = x.TenBan,
                KhuVuc = x.KhuVuc,
                TrangThai = x.TrangThai,
            })
            .FirstOrDefaultAsync();

        if (currentTable != null && currentTable.MaBan != OrderWorkflowConstants.MaBanMangVe)
            model.KhuVuc = currentTable.KhuVuc;

        model.BanTrongTiepTheo = currentTable ?? model.BanTrongTiepTheo;
        return model;
    }

    public async Task<OrderCreateViewModel> GetCreateModelAsync(OrderCreateViewModel model)
    {
        model.HinhThuc = string.IsNullOrWhiteSpace(model.HinhThuc)
            ? OrderWorkflowConstants.HinhThucTaiQuan
            : model.HinhThuc;
        model.KhuVuc = NormalizeTableArea(model.KhuVuc);

        model.DanhSachKhachHang = await _context
            .KhachHangs.AsNoTracking()
            .OrderBy(x => x.TenKH)
            .Select(x => new OrderCustomerViewModel
            {
                MaKH = x.MaKH,
                TenKH = x.TenKH,
                SDT = x.SDT,
                DiemTichLuy = x.DiemTichLuy,
            })
            .ToListAsync();

        var products = await _context
            .SanPhamResults.FromSqlInterpolated($"EXEC SanPham_DS")
            .AsNoTracking()
            .ToListAsync();

        model.DanhSachSanPhamDangBan = products
            .Where(x => x.TrangThai == TrangThaiDangBan)
            .OrderBy(x => x.TenSP)
            .Select(x => new OrderAvailableProductViewModel
            {
                MaSP = x.MaSP,
                TenSP = x.TenSP,
                DonGia = x.DonGia,
                MaLoaiSP = x.MaLoaiSP,
                TenLoaiSP = x.TenLoaiSP,
                TrangThai = x.TrangThai,
            })
            .ToList();

        var activePromotions = await _context
            .KhuyenMais.FromSqlInterpolated($"EXEC KhuyenMai_DangApDung {DateTime.Today}")
            .AsNoTracking()
            .ToListAsync();

        model.DanhSachKhuyenMai = activePromotions
            .OrderBy(x => x.TenKM)
            .Select(x => new OrderPromotionViewModel
            {
                MaKM = x.MaKM,
                TenKM = x.TenKM,
                LoaiKM = x.LoaiKM,
                GiaTri = x.GiaTri,
                TrangThai = x.TrangThai,
            })
            .ToList();

        model.BanTrongDauTienTheoKhuVuc = await GetFirstAvailableTablesByAreaAsync();
        model.BanTrongTiepTheo = ResolvePreviewTable(model);

        if (model.Items.Count == 0)
        {
            model.Items = model
                .DanhSachSanPhamDangBan.Select(x => new OrderItemViewModel
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    DonGia = x.DonGia,
                })
                .ToList();
        }
        else
        {
            var productMap = model.DanhSachSanPhamDangBan.ToDictionary(x => x.MaSP);
            foreach (var item in model.Items)
            {
                if (string.IsNullOrWhiteSpace(item.MaSP))
                    continue;

                if (!productMap.TryGetValue(item.MaSP, out var product))
                    continue;

                item.TenSP = product.TenSP;
                item.DonGia = product.DonGia;
                item.ThanhTien = product.DonGia * item.SoLuong;
                item.TenKM = model.DanhSachKhuyenMai.FirstOrDefault(x => x.MaKM == item.MaKM)?.TenKM;
            }
        }

        return model;
    }

    public async Task<string> CreateOrderAsync(OrderCreateViewModel model, string maNV)
    {
        if (!string.IsNullOrWhiteSpace(model.EditMaHD))
            return await UpdateOrderAsync(model, maNV);

        if (string.IsNullOrWhiteSpace(maNV))
            throw new InvalidOperationException("Phiên đăng nhập không hợp lệ.");

        if (!await IsEmployeeCheckedInAsync(maNV))
            throw new InvalidOperationException("Bạn cần check-in trước khi order.");

        var orderItems = model.Items.Where(x => x.SoLuong > 0).ToList();
        if (!orderItems.Any())
            throw new InvalidOperationException("Vui lòng chọn ít nhất 1 món.");

        string? maKh = null;
        if (!string.IsNullOrWhiteSpace(model.MaKH))
        {
            var customerExists = await _context.KhachHangs.AsNoTracking()
                .AnyAsync(x => x.MaKH == model.MaKH);
            maKh = customerExists ? model.MaKH : null;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var maBan = await ResolveTableAsync(model);
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC HoaDon_Them {(string?)null}, {DateTime.Today}, {maNV}, {maKh}, {model.GhiChu}, {maBan}"
                );

                var maHD = await _context
                    .HoaDons.AsNoTracking()
                    .Where(x => x.MaNV == maNV && x.NgayLap == DateTime.Today && x.MaBan == maBan)
                    .OrderByDescending(x => x.MaHD)
                    .Select(x => x.MaHD)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(maHD))
                    throw new InvalidOperationException("Không tìm thấy hóa đơn vừa tạo.");

                foreach (var item in orderItems)
                {
                    var product = await _context
                        .SanPhams.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MaSP == item.MaSP);

                    if (product == null)
                        throw new InvalidOperationException($"Sản phẩm {item.MaSP} không tồn tại.");

                    if (product.TrangThai != TrangThaiDangBan)
                        throw new InvalidOperationException(
                            $"Sản phẩm {product.TenSP} hiện không ở trạng thái Đang bán."
                        );

                    var maKM = await ResolveActivePromotionAsync(item.MaKM);

                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC CTHD_Them {maHD}, {product.MaSP}, {item.SoLuong}, {product.DonGia}, {maKM}"
                    );
                }

                if (model.HinhThuc == OrderWorkflowConstants.HinhThucTaiQuan)
                    await UpdateTableStatusAsync(maBan, OrderWorkflowConstants.TrangThaiBanDangPhucVu);

                await tx.CommitAsync();
                return maHD;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<string> UpdateOrderAsync(OrderCreateViewModel model, string maNV)
    {
        if (string.IsNullOrWhiteSpace(maNV))
            throw new InvalidOperationException("Phiên đăng nhập không hợp lệ.");

        if (string.IsNullOrWhiteSpace(model.EditMaHD))
            throw new InvalidOperationException("Thiếu mã hóa đơn cần sửa.");

        if (!await IsEmployeeCheckedInAsync(maNV))
            throw new InvalidOperationException("Bạn cần check-in trước khi sửa order.");

        var orderItems = model.Items.Where(x => x.SoLuong > 0).ToList();
        if (!orderItems.Any())
            throw new InvalidOperationException("Vui lòng chọn ít nhất 1 món.");

        var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(x => x.MaHD == model.EditMaHD);
        if (hoaDon == null)
            throw new InvalidOperationException("Không tìm thấy hóa đơn cần sửa.");

        if (!TableWorkflowManager.CanEditOrder(hoaDon.TrangThaiOrder, hoaDon.TrangThaiThanhToan, out var reason))
            throw new InvalidOperationException(reason);

        string? maKh = null;
        if (!string.IsNullOrWhiteSpace(model.MaKH))
        {
            var customerExists = await _context.KhachHangs.AsNoTracking()
                .AnyAsync(x => x.MaKH == model.MaKH);
            maKh = customerExists ? model.MaKH : null;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC HoaDon_Sua {hoaDon.MaHD}, {hoaDon.NgayLap}, {hoaDon.MaNV}, {maKh}, {model.GhiChu}, {hoaDon.MaBan}"
                );

                var currentDetails = await _context.CTHDs.AsNoTracking()
                    .Where(x => x.MaHD == hoaDon.MaHD)
                    .Select(x => x.MaSP)
                    .ToListAsync();

                foreach (var maSP in currentDetails)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC CTHD_Xoa {hoaDon.MaHD}, {maSP}"
                    );
                }

                foreach (var item in orderItems)
                {
                    var product = await _context.SanPhams.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MaSP == item.MaSP);

                    if (product == null)
                        throw new InvalidOperationException($"Sản phẩm {item.MaSP} không tồn tại.");

                    if (product.TrangThai != TrangThaiDangBan)
                        throw new InvalidOperationException(
                            $"Sản phẩm {product.TenSP} hiện không ở trạng thái Đang bán."
                        );

                    var maKM = await ResolveActivePromotionAsync(item.MaKM);

                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC CTHD_Them {hoaDon.MaHD}, {product.MaSP}, {item.SoLuong}, {product.DonGia}, {maKM}"
                    );
                }

                if (hoaDon.MaBan != OrderWorkflowConstants.MaBanMangVe)
                    await UpdateTableStatusAsync(hoaDon.MaBan, OrderWorkflowConstants.TrangThaiBanDangPhucVu);

                await tx.CommitAsync();
                return hoaDon.MaHD;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<OrderReceiptViewModel?> GetReceiptAsync(string maHD)
    {
        var header = (
            await _context
                .HoaDonResults.FromSqlInterpolated($"EXEC HoaDon_CT {maHD}")
                .AsNoTracking()
                .ToListAsync()
        ).FirstOrDefault();

        if (header == null)
            return null;

        var details = await _context
            .CTHDResults.FromSqlInterpolated($"EXEC CTHD_DSTheoMaHD {maHD}")
            .AsNoTracking()
            .ToListAsync();

        var customer = string.IsNullOrWhiteSpace(header.MaKH)
            ? null
            : await _context.KhachHangs.AsNoTracking().FirstOrDefaultAsync(x => x.MaKH == header.MaKH);

        var hoaDon = await _context.HoaDons.AsNoTracking().FirstOrDefaultAsync(x => x.MaHD == maHD);
        var ban = await _context.Bans.AsNoTracking().FirstOrDefaultAsync(x => x.MaBan == header.MaBan);
        var payments = await _context.ThanhToans.AsNoTracking()
            .Where(x => x.MaHD == maHD)
            .OrderByDescending(x => x.MaThanhToan)
            .Select(x => new PaymentInfoViewModel
            {
                MaThanhToan = x.MaThanhToan,
                MaHD = x.MaHD,
                MaPhienThanhToan = x.MaPhienThanhToan,
                PhuongThuc = x.PhuongThuc,
                SoTien = x.SoTien,
                NoiDungChuyenKhoan = x.NoiDungChuyenKhoan,
                MaGiaoDich = x.MaGiaoDich,
                TrangThai = x.TrangThai,
                ThoiGianTao = x.ThoiGianTao,
                ThoiGianHetHan = x.ThoiGianHetHan,
                ThoiGianThanhToan = x.ThoiGianThanhToan,
                GhiChu = x.GhiChu,
            })
            .ToListAsync();

        foreach (var payment in payments)
        {
            if (payment.PhuongThuc == OrderWorkflowConstants.PhuongThucQr
                && !string.IsNullOrWhiteSpace(payment.MaPhienThanhToan))
            {
                payment.VietQrUrl = BuildVietQrUrl(payment.SoTien, payment.MaPhienThanhToan);
            }
        }

        var trangThaiOrder = hoaDon?.TrangThaiOrder ?? OrderWorkflowConstants.TrangThaiOrderChoThanhToan;
        var trangThaiThanhToan =
            hoaDon?.TrangThaiThanhToan ?? OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan;
        var isPayable = IsPayable(trangThaiOrder, trangThaiThanhToan);
        var pendingQr = payments.FirstOrDefault(x =>
            x.PhuongThuc == OrderWorkflowConstants.PhuongThucQr
            && x.TrangThai == OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr);
        var canChoosePayment = isPayable && pendingQr == null;

        return new OrderReceiptViewModel
        {
            MaHD = header.MaHD,
            NgayLap = header.NgayLap,
            GioIn = DateTime.Now,
            MaNV = header.MaNV,
            TenNV = header.TenNV,
            MaKH = header.MaKH,
            TenKH = header.TenKH,
            DiemTichLuy = customer?.DiemTichLuy,
            MaBan = header.MaBan,
            TenBan = header.TenBan,
            HinhThuc = header.MaBan == OrderWorkflowConstants.MaBanMangVe
                ? OrderWorkflowConstants.HinhThucMangVe
                : OrderWorkflowConstants.HinhThucTaiQuan,
            TongTien = header.TongTien,
            GhiChu = header.GhiChu,
            DiemDaCong = customer == null
                ? null
                : (trangThaiThanhToan == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
                    ? (int)Math.Floor(header.TongTien / 10000m)
                    : 0),
            TrangThaiOrder = trangThaiOrder,
            TrangThaiThanhToan = trangThaiThanhToan,
            PhuongThucThanhToan = hoaDon?.PhuongThucThanhToan,
            ThoiGianTao = hoaDon?.ThoiGianTao ?? default,
            ThoiGianThanhToan = hoaDon?.ThoiGianThanhToan,
            TrangThaiBan = ban?.TrangThai,
            CanPay = canChoosePayment,
            CanCreateQr = canChoosePayment,
            CanEditOrder = TableWorkflowManager.CanEditOrder(trangThaiOrder, trangThaiThanhToan, out _),
            CanCancelOrder = trangThaiThanhToan != OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
                && trangThaiOrder != OrderWorkflowConstants.TrangThaiOrderDaHuy,
            CanCompleteTable = header.MaBan != OrderWorkflowConstants.MaBanMangVe
                && trangThaiThanhToan == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan
                && ban?.TrangThai == OrderWorkflowConstants.TrangThaiBanDangPhucVu,
            PendingQr = pendingQr,
            Payments = payments,
            Items = details
                .Select(x => new OrderItemViewModel
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    SoLuong = x.SoLuong,
                    DonGia = x.DonGia,
                    ThanhTien = x.ThanhTien,
                    MaKM = x.MaKM,
                    TenKM = x.TenKM,
                })
                .ToList(),
        };
    }

    public async Task<bool> IsEmployeeCheckedInAsync(string maNV)
    {
        try
        {
            return (
                await _context
                    .OrderChamCongStatusResults.FromSqlInterpolated(
                        $"EXEC ChamCong_KiemTraDangLam {maNV}"
                    )
                    .AsNoTracking()
                    .ToListAsync()
            ).Any();
        }
        catch
        {
            return false;
        }
    }

    public async Task CompleteTableAsync(string maHD)
    {
        var receipt = await GetReceiptAsync(maHD);
        if (receipt == null)
            throw new InvalidOperationException("Không tìm thấy hóa đơn.");

        if (receipt.MaBan != OrderWorkflowConstants.MaBanMangVe)
            await CompletePhysicalTableAsync(receipt.MaBan);
    }

    public async Task<PaymentCommandResultViewModel> PayCashAsync(string maHD, string? ghiChu)
    {
        var result = await ExecutePaymentProcedureAsync(
            "ThanhToan_TienMat",
            ("@MaHD", maHD),
            ("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim())
        );

        if (result.Success)
        {
            await DeductInventoryForInvoiceAsync(maHD);

            var invoice = await _context.HoaDons.AsNoTracking()
                .Where(x => x.MaHD == maHD)
                .Select(x => new { x.MaKH })
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(invoice?.MaKH))
            {
                await AddCustomerPointsAsync(maHD, invoice.MaKH);
            }
        }

        return result;
    }

    public async Task<PaymentCommandResultViewModel> CreateQrPaymentAsync(string maHD)
    {
        var result = await ExecutePaymentProcedureAsync("ThanhToan_TaoQR", ("@MaHD", maHD));
        if (!string.IsNullOrWhiteSpace(result.MaPhienThanhToan) && result.SoTien.HasValue)
            result.VietQrUrl = BuildVietQrUrl(result.SoTien.Value, result.MaPhienThanhToan);

        return result;
    }

    public async Task<PaymentCommandResultViewModel> ConfirmQrDemoAsync(
        string maHD,
        string maPhienThanhToan
    )
    {
        var payment = await _context.ThanhToans.AsNoTracking()
            .Where(x =>
                x.MaHD == maHD
                && x.PhuongThuc == OrderWorkflowConstants.PhuongThucQr
                && x.MaPhienThanhToan == maPhienThanhToan
            )
            .OrderByDescending(x => x.MaThanhToan)
            .FirstOrDefaultAsync();

        if (payment == null)
            throw new InvalidOperationException("Không tìm thấy phiên QR cần xác nhận.");

        var transactionCode = $"DEMO-{DateTime.Now:yyyyMMddHHmmssfff}-{payment.MaThanhToan}";
        var transferContent = payment.NoiDungChuyenKhoan ?? payment.MaPhienThanhToan ?? maPhienThanhToan;

        var result = await ExecutePaymentProcedureAsync(
            "ThanhToan_XacNhanQRTheoPhien",
            ("@MaGiaoDich", transactionCode),
            ("@MaPhienThanhToan", maPhienThanhToan),
            ("@SoTien", payment.SoTien),
            ("@NoiDungChuyenKhoan", transferContent)
        );

        if (result.Success)
        {
            await DeductInventoryForInvoiceAsync(maHD);

            var invoice = await _context.HoaDons.AsNoTracking()
                .Where(x => x.MaHD == maHD)
                .Select(x => new { x.MaKH })
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(invoice?.MaKH))
            {
                await AddCustomerPointsAsync(maHD, invoice.MaKH);
            }
        }

        return result;
    }

    public async Task<PaymentCommandResultViewModel> ExpireQrAsync(string maHD)
    {
        return await ExecutePaymentProcedureAsync("ThanhToan_QRHetHan", ("@MaHD", maHD));
    }

    public async Task<PaymentCommandResultViewModel> CancelOrderAsync(string maHD, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new InvalidOperationException("Vui lòng nhập lý do hủy order.");

        var invoice = await _context.HoaDons.AsNoTracking()
            .Where(x => x.MaHD == maHD)
            .Select(x => new { x.MaKH, x.TrangThaiThanhToan })
            .FirstOrDefaultAsync();

        var result = await ExecutePaymentProcedureAsync(
            "ThanhToan_HuyOrder",
            ("@MaHD", maHD),
            ("@LyDo", lyDo.Trim())
        );

        if (result.Success && !string.IsNullOrWhiteSpace(invoice?.MaKH))
        {
            if (invoice.TrangThaiThanhToan == OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan)
            {
                await ReverseCustomerPointsAsync(maHD, invoice.MaKH);
            }
        }

        return result;
    }

    public async Task<PaymentCommandResultViewModel> EditOrderAsync(string maHD)
    {
        return await ExecutePaymentProcedureAsync("ThanhToan_SuaOrder", ("@MaHD", maHD));
    }

    public async Task<PaymentCommandResultViewModel> ProcessExpiredQrAsync()
    {
        return await ExecutePaymentProcedureAsync("ThanhToan_XuLyTatCaQRHetHan");
    }

    public async Task<List<OrderCustomerViewModel>> SearchCustomersAsync(string keyword)
    {
        keyword = (keyword ?? string.Empty).Trim();
        if (keyword.Length < 3)
            return new List<OrderCustomerViewModel>();

        return await _context.KhachHangs.AsNoTracking()
            .Where(x => x.SDT.Contains(keyword))
            .OrderByDescending(x => x.SDT.StartsWith(keyword))
            .ThenByDescending(x => x.SDT.EndsWith(keyword))
            .ThenBy(x => x.TenKH)
            .Take(8)
            .Select(x => new OrderCustomerViewModel
            {
                MaKH = x.MaKH,
                TenKH = x.TenKH,
                SDT = x.SDT,
                DiemTichLuy = x.DiemTichLuy,
                GiamGiaCoTheApDung = null,
            })
            .ToListAsync();
    }

    public async Task<OrderCustomerViewModel> CreateQuickCustomerAsync(
        OrderQuickCustomerCreateViewModel model
    )
    {
        var tenKh = (model.TenKH ?? string.Empty).Trim();
        var sdt = (model.SDT ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(tenKh))
            throw new InvalidOperationException("Vui lòng nhập tên khách hàng.");

        if (string.IsNullOrWhiteSpace(sdt) || sdt.Length > 10)
            throw new InvalidOperationException("Số điện thoại không hợp lệ.");

        var duplicated = await _context.KhachHangs.AsNoTracking().AnyAsync(x => x.SDT == sdt);
        if (duplicated)
            throw new InvalidOperationException("Số điện thoại đã tồn tại.");

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC KhachHang_Them {(string?)null}, {tenKh}, {sdt}, {0}"
        );

        var customer = await _context.KhachHangs.AsNoTracking()
            .Where(x => x.SDT == sdt)
            .OrderByDescending(x => x.MaKH)
            .Select(x => new OrderCustomerViewModel
            {
                MaKH = x.MaKH,
                TenKH = x.TenKH,
                SDT = x.SDT,
                DiemTichLuy = x.DiemTichLuy,
                GiamGiaCoTheApDung = null,
            })
            .FirstOrDefaultAsync();

        return customer
            ?? throw new InvalidOperationException("Không thể tạo khách hàng nhanh.");
    }

    private async Task<PaymentCommandResultViewModel> ExecutePaymentProcedureAsync(
        string procedureName,
        params (string Name, object? Value)[] parameters
    )
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;

            var currentTransaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            if (currentTransaction != null)
                command.Transaction = currentTransaction;

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return new PaymentCommandResultViewModel { Success = true };

            return new PaymentCommandResultViewModel
            {
                Success = ReadBool(reader, "Success"),
                Message = ReadString(reader, "Message") ?? string.Empty,
                MaHD = ReadString(reader, "MaHD"),
                MaBan = ReadString(reader, "MaBan"),
                MaPhienThanhToan = ReadString(reader, "MaPhienThanhToan"),
                SoTien = ReadDecimal(reader, "SoTien"),
                NoiDungChuyenKhoan = ReadString(reader, "NoiDungChuyenKhoan"),
                ThoiGianHetHan = ReadDateTime(reader, "ThoiGianHetHan"),
                SoHoaDonXuLy = ReadInt(reader, "SoHoaDonXuLy"),
            };
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private async Task CompletePhysicalTableAsync(string maBan)
    {
        if (maBan == OrderWorkflowConstants.MaBanMangVe)
            return;

        var hasUnpaidOrder = await _context.HoaDons.AsNoTracking()
            .AnyAsync(x =>
                x.MaBan == maBan
                && x.TrangThaiOrder != OrderWorkflowConstants.TrangThaiOrderDaHuy
                && x.TrangThaiThanhToan != OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan);

        if (hasUnpaidOrder)
            throw new InvalidOperationException("Bàn còn order chưa thanh toán, không thể trả bàn.");

        var result = await ExecutePaymentProcedureAsync("Ban_KhachDi", ("@MaBan", maBan));
        if (!result.Success && !string.IsNullOrWhiteSpace(result.Message))
            throw new InvalidOperationException(result.Message);
    }

    private static bool IsPayable(string trangThaiOrder, string trangThaiThanhToan) =>
        TableWorkflowManager.IsPayable(trangThaiOrder, trangThaiThanhToan);

    private static string BuildVietQrUrl(decimal amount, string maPhienThanhToan)
    {
        return CafeOrderCalculator.SinhUrlVietQr(
            amount,
            maPhienThanhToan,
            VietQrBankCode,
            VietQrAccountNo,
            VietQrAccountName
        );
    }

    private static bool HasColumn(IDataRecord record, string name)
    {
        for (var i = 0; i < record.FieldCount; i++)
        {
            if (string.Equals(record.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string? ReadString(IDataRecord record, string name)
    {
        if (!HasColumn(record, name))
            return null;
        var value = record[name];
        return value == DBNull.Value ? null : Convert.ToString(value);
    }

    private static bool ReadBool(IDataRecord record, string name)
    {
        if (!HasColumn(record, name) || record[name] == DBNull.Value)
            return false;
        return Convert.ToBoolean(record[name]);
    }

    private static decimal? ReadDecimal(IDataRecord record, string name)
    {
        if (!HasColumn(record, name) || record[name] == DBNull.Value)
            return null;
        return Convert.ToDecimal(record[name]);
    }

    private static DateTime? ReadDateTime(IDataRecord record, string name)
    {
        if (!HasColumn(record, name) || record[name] == DBNull.Value)
            return null;
        return Convert.ToDateTime(record[name]);
    }

    private static int? ReadInt(IDataRecord record, string name)
    {
        if (!HasColumn(record, name) || record[name] == DBNull.Value)
            return null;
        return Convert.ToInt32(record[name]);
    }

    private async Task<string?> ResolveActivePromotionAsync(string? maKM)
    {
        maKM = (maKM ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(maKM))
            return null;

        var today = DateTime.Today;
        var promotion = await _context.KhuyenMais.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKM == maKM);

        if (promotion == null)
            throw new InvalidOperationException("Khuyến mãi không tồn tại.");

        var isActive =
            promotion.TrangThai == TrangThaiKhuyenMaiDangApDung
            && today >= promotion.NgayBatDau.Date
            && today <= promotion.NgayKetThuc.Date
            && promotion.GiaTri >= 0
            && (
                promotion.LoaiKM == LoaiKhuyenMaiTienMat
                || (promotion.LoaiKM == LoaiKhuyenMaiPhanTram && promotion.GiaTri <= 100)
            );

        if (!isActive)
            throw new InvalidOperationException("Khuyến mãi không còn hiệu lực hoặc không hợp lệ.");

        return promotion.MaKM;
    }

    public async Task AddCustomerPointsAsync(string maHD, string maKH)
    {
        var total = await _context.HoaDons.AsNoTracking()
            .Where(x => x.MaHD == maHD)
            .Select(x => x.TongTien)
            .FirstOrDefaultAsync();

        var points = CafeOrderCalculator.TinhDiemTichLuy(total);
        if (points <= 0)
            return;

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE dbo.KhachHang SET DiemTichLuy = ISNULL(DiemTichLuy, 0) + {points} WHERE MaKH = {maKH}"
        );
    }

    public async Task ReverseCustomerPointsAsync(string maHD, string maKH)
    {
        var total = await _context.HoaDons.AsNoTracking()
            .Where(x => x.MaHD == maHD)
            .Select(x => x.TongTien)
            .FirstOrDefaultAsync();

        var points = CafeOrderCalculator.TinhDiemTichLuy(total);
        if (points <= 0)
            return;

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE dbo.KhachHang SET DiemTichLuy = CASE WHEN ISNULL(DiemTichLuy, 0) >= {points} THEN DiemTichLuy - {points} ELSE 0 END WHERE MaKH = {maKH}"
        );
    }

    private async Task<string> ResolveTableAsync(OrderCreateViewModel model)
    {
        if (model.HinhThuc == OrderWorkflowConstants.HinhThucMangVe)
        {
            var mveExists = await _context.Bans.AnyAsync(x => x.MaBan == OrderWorkflowConstants.MaBanMangVe);
            if (!mveExists)
                throw new InvalidOperationException("Chưa có bàn Mang về trong dữ liệu.");

            return OrderWorkflowConstants.MaBanMangVe;
        }

        var khuVuc = NormalizeTableArea(model.KhuVuc);
        var table = await GetNextAvailableTableAsync(khuVuc);

        if (table == null && khuVuc != OrderWorkflowConstants.KhuVucTangTret)
            table = await GetNextAvailableTableAsync(OrderWorkflowConstants.KhuVucTangTret);

        if (table == null)
        {
            var anyAvailableTable = await _context.Bans.AsNoTracking().AnyAsync(x =>
                x.MaBan != OrderWorkflowConstants.MaBanMangVe
                && x.TrangThai == OrderWorkflowConstants.TrangThaiBanTrong);

            throw new InvalidOperationException(
                anyAvailableTable
                    ? "Khu vực này đã hết bàn trống."
                    : "Hiện không còn bàn trống tại quán."
            );
        }

        return table.MaBan;
    }

    private async Task<OrderTableViewModel?> GetNextAvailableTableAsync(string khuVuc)
    {
        var table = await _context.Bans
            .FromSqlInterpolated($"SELECT TOP 1 * FROM dbo.Ban WITH (UPDLOCK, HOLDLOCK) WHERE MaBan <> {OrderWorkflowConstants.MaBanMangVe} AND KhuVuc = {khuVuc} AND TrangThai = {OrderWorkflowConstants.TrangThaiBanTrong} ORDER BY SoBan, MaBan, TenBan")
            .FirstOrDefaultAsync();

        if (table == null)
            return null;

        return new OrderTableViewModel
        {
            MaBan = table.MaBan,
            TenBan = table.TenBan,
            KhuVuc = table.KhuVuc,
            TrangThai = table.TrangThai,
        };
    }

    private async Task<List<OrderTableViewModel>> GetFirstAvailableTablesByAreaAsync()
    {
        var availableTables = await _context.Bans.AsNoTracking()
            .Where(x =>
                x.MaBan != OrderWorkflowConstants.MaBanMangVe
                && x.TrangThai == OrderWorkflowConstants.TrangThaiBanTrong)
            .OrderBy(x => x.SoBan)
            .ThenBy(x => x.MaBan)
            .ThenBy(x => x.TenBan)
            .Select(x => new OrderTableViewModel
            {
                MaBan = x.MaBan,
                TenBan = x.TenBan,
                KhuVuc = x.KhuVuc,
                TrangThai = x.TrangThai,
            })
            .ToListAsync();

        return availableTables
            .GroupBy(x => x.KhuVuc)
            .Select(x => x.First())
            .ToList();
    }

    private static string NormalizeTableArea(string? khuVuc)
    {
        return khuVuc == OrderWorkflowConstants.KhuVucTangLung
            ? OrderWorkflowConstants.KhuVucTangLung
            : OrderWorkflowConstants.KhuVucTangTret;
    }

    private static OrderTableViewModel? ResolvePreviewTable(OrderCreateViewModel model)
    {
        var selectedTable = model.BanTrongDauTienTheoKhuVuc
            .FirstOrDefault(x => x.KhuVuc == model.KhuVuc);

        if (selectedTable != null)
            return selectedTable;

        model.ThongBaoBan = "Khu vực này đã hết bàn trống.";
        model.KhuVuc = OrderWorkflowConstants.KhuVucTangTret;

        var defaultTable = model.BanTrongDauTienTheoKhuVuc
            .FirstOrDefault(x => x.KhuVuc == OrderWorkflowConstants.KhuVucTangTret);
        if (defaultTable != null)
            return defaultTable;

        model.ThongBaoBan = "Hiện không còn bàn trống tại quán.";
        return null;
    }

    private async Task UpdateTableStatusAsync(string maBan, string trangThai)
    {
        var table = await _context.Bans.FirstOrDefaultAsync(x => x.MaBan == maBan);
        if (table == null)
            throw new InvalidOperationException("Không tìm thấy bàn.");

        table.TrangThai = trangThai;
        await _context.SaveChangesAsync();
    }
    private async Task DeductInventoryForInvoiceAsync(string maHD)
    {
        try
        {
            var lineItems = await _context.CTHDs.AsNoTracking()
                .Where(x => x.MaHD == maHD)
                .Select(x => new { x.MaSP, x.SoLuong })
                .ToListAsync();

            if (!lineItems.Any())
                return;

            var productIds = lineItems.Select(x => x.MaSP).Distinct().ToList();
            var recipes = await _context.CongThucs.AsNoTracking()
                .Where(x => productIds.Contains(x.MaSP))
                .ToListAsync();

            if (!recipes.Any())
                return;

            var deductions = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in lineItems)
            {
                var itemRecipes = recipes.Where(r => string.Equals(r.MaSP, item.MaSP, StringComparison.OrdinalIgnoreCase));
                foreach (var r in itemRecipes)
                {
                    var totalDeduct = r.DinhLuong * item.SoLuong;
                    if (deductions.ContainsKey(r.MaNL))
                        deductions[r.MaNL] += totalDeduct;
                    else
                        deductions[r.MaNL] = totalDeduct;
                }
            }

            var ingredientIds = deductions.Keys.ToList();
            var ingredients = await _context.NguyenLieus
                .Where(x => ingredientIds.Contains(x.MaNL))
                .ToListAsync();

            foreach (var ing in ingredients)
            {
                if (deductions.TryGetValue(ing.MaNL, out var deductAmount))
                {
                    var currentStock = ing.SoLuongTon ?? 0;
                    var newStock = currentStock - (int)Math.Round(deductAmount, MidpointRounding.AwayFromZero);
                    ing.SoLuongTon = Math.Max(0, newStock);
                    ing.NgayCapNhat = DateTime.Today;
                }
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            // Trừ kho nguyên liệu hỗ trợ; không làm crash giao dịch thanh toán chính
        }
    }
}