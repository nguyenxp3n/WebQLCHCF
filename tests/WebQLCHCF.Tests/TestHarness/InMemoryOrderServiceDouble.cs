using WebQLCHCF.Models;
using WebQLCHCF.Services;
using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Tests.TestHarness;

public class InMemoryOrderServiceDouble : IOrderService
{
    public List<Ban> Tables { get; } = new();
    public List<SanPham> Products { get; } = new();
    public List<KhuyenMai> Promotions { get; } = new();
    public List<KhachHang> Customers { get; } = new();
    public Dictionary<string, HoaDon> Invoices { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<CTHD> OrderDetails { get; } = new();
    public List<ThanhToan> Payments { get; } = new();
    public HashSet<string> CheckedInStaff { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> PointsAwardedByInvoice { get; } = new(StringComparer.OrdinalIgnoreCase);

    private int _invoiceCounter = 1;
    private int _paymentCounter = 1;

    public InMemoryOrderServiceDouble()
    {
        SeedDefaultData();
    }

    private void SeedDefaultData()
    {
        Tables.AddRange(new[]
        {
            new Ban { MaBan = "B01", TenBan = "Bàn 01", KhuVuc = "Tầng trệt", SoBan = 1, TrangThai = "Trống" },
            new Ban { MaBan = "B02", TenBan = "Bàn 02", KhuVuc = "Tầng trệt", SoBan = 2, TrangThai = "Trống" },
            new Ban { MaBan = "B03", TenBan = "Bàn 03", KhuVuc = "Tầng lửng", SoBan = 3, TrangThai = "Trống" },
            new Ban { MaBan = "MVE", TenBan = "Mang về", KhuVuc = "Mang về", SoBan = null, TrangThai = "Trống" },
        });

        Products.AddRange(new[]
        {
            new SanPham { MaSP = "SP01", TenSP = "Cà phê đen", DonGia = 25000, MaLoaiSP = "L01", TrangThai = "Đang bán" },
            new SanPham { MaSP = "SP02", TenSP = "Cà phê sữa", DonGia = 30000, MaLoaiSP = "L01", TrangThai = "Đang bán" },
            new SanPham { MaSP = "SP03", TenSP = "Trà đào cam sả", DonGia = 35000, MaLoaiSP = "L02", TrangThai = "Đang bán" },
            new SanPham { MaSP = "SP04", TenSP = "Bánh Tiramisu", DonGia = 45000, MaLoaiSP = "L03", TrangThai = "Ngừng bán" }
        });

        Promotions.AddRange(new[]
        {
            new KhuyenMai
            {
                MaKM = "KM01",
                TenKM = "Giảm 10%",
                LoaiKM = "Phần trăm",
                GiaTri = 10,
                NgayBatDau = DateTime.Today.AddDays(-5),
                NgayKetThuc = DateTime.Today.AddDays(30),
                DieuKienApDung = "Áp dụng cho mọi hóa đơn",
                TrangThai = "Đang áp dụng"
            },
            new KhuyenMai
            {
                MaKM = "KM02",
                TenKM = "Giảm 10K",
                LoaiKM = "Tiền mặt",
                GiaTri = 10000,
                NgayBatDau = DateTime.Today.AddDays(-5),
                NgayKetThuc = DateTime.Today.AddDays(30),
                DieuKienApDung = "Hóa đơn trên 50K",
                TrangThai = "Đang áp dụng"
            }
        });

        Customers.AddRange(new[]
        {
            new KhachHang { MaKH = "KH01", TenKH = "Nguyễn Văn A", SDT = "0901234567", DiemTichLuy = 50 },
            new KhachHang { MaKH = "KH02", TenKH = "Trần Thị B", SDT = "0912345678", DiemTichLuy = 5 }
        });

        CheckedInStaff.Add("NV01");
        CheckedInStaff.Add("NV02");
        CheckedInStaff.Add("NV03");
    }

    public Task<bool> IsEmployeeCheckedInAsync(string maNV)
    {
        return Task.FromResult(CheckedInStaff.Contains(maNV));
    }

    public Task<OrderCreateViewModel> GetCreateModelAsync()
    {
        var model = new OrderCreateViewModel
        {
            DanhSachSanPhamDangBan = Products
                .Where(p => p.TrangThai == "Đang bán")
                .Select(p => new OrderAvailableProductViewModel
                {
                    MaSP = p.MaSP,
                    TenSP = p.TenSP,
                    DonGia = p.DonGia,
                    MaLoaiSP = p.MaLoaiSP,
                    TrangThai = p.TrangThai
                }).ToList(),
            DanhSachKhuyenMai = Promotions
                .Where(k => k.TrangThai == "Đang áp dụng")
                .Select(k => new OrderPromotionViewModel
                {
                    MaKM = k.MaKM,
                    TenKM = k.TenKM,
                    LoaiKM = k.LoaiKM,
                    GiaTri = k.GiaTri,
                    TrangThai = k.TrangThai
                }).ToList(),
            DanhSachKhachHang = Customers
                .Select(c => new OrderCustomerViewModel
                {
                    MaKH = c.MaKH,
                    TenKH = c.TenKH,
                    SDT = c.SDT,
                    DiemTichLuy = c.DiemTichLuy
                }).ToList(),
            BanTrongDauTienTheoKhuVuc = Tables
                .Where(t => t.TrangThai == "Trống" && t.MaBan != "MVE")
                .Select(t => new OrderTableViewModel
                {
                    MaBan = t.MaBan,
                    TenBan = t.TenBan,
                    KhuVuc = t.KhuVuc,
                    TrangThai = t.TrangThai
                }).ToList()
        };

        return Task.FromResult(model);
    }

    public Task<OrderCreateViewModel> GetCreateModelForEditAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            throw new InvalidOperationException($"Không tìm thấy hóa đơn {maHD}");

        var model = new OrderCreateViewModel
        {
            EditMaHD = maHD,
            GhiChu = hd.GhiChu,
            MaKH = hd.MaKH,
            Items = OrderDetails
                .Where(d => d.MaHD == maHD)
                .Select(d =>
                {
                    var sp = Products.FirstOrDefault(p => p.MaSP == d.MaSP);
                    return new OrderItemViewModel
                    {
                        MaSP = d.MaSP,
                        TenSP = sp?.TenSP ?? d.MaSP,
                        DonGia = d.DonGia,
                        SoLuong = d.SoLuong,
                        MaKM = d.MaKM,
                        ThanhTien = d.SoLuong * d.DonGia
                    };
                }).ToList()
        };

        return Task.FromResult(model);
    }

    public Task<OrderCreateViewModel> GetCreateModelAsync(OrderCreateViewModel model)
    {
        return Task.FromResult(model);
    }

    public Task<string> CreateOrderAsync(OrderCreateViewModel model, string maNV)
    {
        if (!CheckedInStaff.Contains(maNV))
            throw new InvalidOperationException("Bạn cần check-in trước khi order.");

        var validItems = model.Items.Where(i => i.SoLuong > 0).ToList();
        if (validItems.Count == 0)
            throw new InvalidOperationException("Vui lòng chọn ít nhất 1 món.");

        foreach (var item in validItems)
        {
            var sp = Products.FirstOrDefault(p => p.MaSP == item.MaSP);
            if (sp == null || sp.TrangThai != "Đang bán")
                throw new InvalidOperationException($"Sản phẩm {item.MaSP} hiện không ở trạng thái Đang bán.");
        }

        Ban? table;
        if (model.HinhThuc == "MangVe")
        {
            table = Tables.FirstOrDefault(t => t.MaBan == "MVE");
        }
        else
        {
            table = Tables.FirstOrDefault(t => t.KhuVuc == model.KhuVuc && t.TrangThai == "Trống" && t.MaBan != "MVE");
            if (table == null)
                throw new InvalidOperationException("Hiện không còn bàn trống tại khu vực này.");
        }

        if (table != null && table.MaBan != "MVE")
        {
            table.TrangThai = "Đang phục vụ";
        }

        var maHD = $"HD{_invoiceCounter++:D5}";

        decimal total = 0;
        foreach (var item in validItems)
        {
            var sp = Products.First(p => p.MaSP == item.MaSP);
            string? loaiKM = null;
            decimal? giaTriKM = null;

            if (!string.IsNullOrEmpty(item.MaKM))
            {
                var km = Promotions.FirstOrDefault(k => k.MaKM == item.MaKM);
                if (km != null)
                {
                    loaiKM = km.LoaiKM;
                    giaTriKM = km.GiaTri;
                }
            }

            var lineTotal = CafeOrderCalculator.TinhThanhTienMon(sp.DonGia, item.SoLuong, loaiKM, giaTriKM);
            total += lineTotal;

            var cthd = new CTHD
            {
                MaHD = maHD,
                MaSP = item.MaSP,
                SoLuong = item.SoLuong,
                DonGia = sp.DonGia,
                MaKM = item.MaKM
            };
            OrderDetails.Add(cthd);
        }

        var hoaDon = new HoaDon
        {
            MaHD = maHD,
            NgayLap = DateTime.Today,
            MaNV = maNV,
            MaKH = model.MaKH,
            MaBan = table?.MaBan ?? "MVE",
            TrangThaiOrder = "ChoThanhToan",
            TrangThaiThanhToan = "ChuaThanhToan",
            ThoiGianTao = DateTime.Now,
            GhiChu = model.GhiChu
        };

        typeof(HoaDon).GetProperty(nameof(HoaDon.TongTien))?.SetValue(hoaDon, total);
        Invoices[maHD] = hoaDon;

        return Task.FromResult(maHD);
    }

    public Task<string> UpdateOrderAsync(OrderCreateViewModel model, string maNV)
    {
        if (string.IsNullOrEmpty(model.EditMaHD) || !Invoices.TryGetValue(model.EditMaHD, out var hd))
            throw new InvalidOperationException("Hóa đơn không tồn tại.");

        if (!TableWorkflowManager.CanEditOrder(hd.TrangThaiOrder, hd.TrangThaiThanhToan, out var reason))
            throw new InvalidOperationException(reason);

        var validItems = model.Items.Where(i => i.SoLuong > 0).ToList();
        if (validItems.Count == 0)
            throw new InvalidOperationException("Vui lòng chọn ít nhất 1 món.");

        OrderDetails.RemoveAll(d => d.MaHD == model.EditMaHD);

        decimal total = 0;
        foreach (var item in validItems)
        {
            var sp = Products.First(p => p.MaSP == item.MaSP);
            string? loaiKM = null;
            decimal? giaTriKM = null;

            if (!string.IsNullOrEmpty(item.MaKM))
            {
                var km = Promotions.FirstOrDefault(k => k.MaKM == item.MaKM);
                if (km != null)
                {
                    loaiKM = km.LoaiKM;
                    giaTriKM = km.GiaTri;
                }
            }

            var lineTotal = CafeOrderCalculator.TinhThanhTienMon(sp.DonGia, item.SoLuong, loaiKM, giaTriKM);
            total += lineTotal;

            OrderDetails.Add(new CTHD
            {
                MaHD = model.EditMaHD,
                MaSP = item.MaSP,
                SoLuong = item.SoLuong,
                DonGia = sp.DonGia,
                MaKM = item.MaKM
            });
        }

        typeof(HoaDon).GetProperty(nameof(HoaDon.TongTien))?.SetValue(hd, total);
        hd.GhiChu = model.GhiChu;

        return Task.FromResult(model.EditMaHD);
    }

    public Task<OrderReceiptViewModel?> GetReceiptAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult<OrderReceiptViewModel?>(null);

        var table = Tables.FirstOrDefault(t => t.MaBan == hd.MaBan);
        var customer = Customers.FirstOrDefault(c => c.MaKH == hd.MaKH);

        var isPaid = TableWorkflowManager.IsPaid(hd.TrangThaiThanhToan);
        var isTableInService = table?.TrangThai == "Đang phục vụ";

        var receipt = new OrderReceiptViewModel
        {
            MaHD = hd.MaHD,
            NgayLap = hd.NgayLap,
            GioIn = DateTime.Now,
            MaNV = hd.MaNV,
            TenNV = "Nhân Viên " + hd.MaNV,
            MaKH = hd.MaKH,
            TenKH = customer?.TenKH,
            DiemTichLuy = customer?.DiemTichLuy,
            MaBan = hd.MaBan,
            TenBan = table?.TenBan ?? hd.MaBan,
            HinhThuc = hd.MaBan == "MVE" ? "Mang về" : "Tại quán",
            TongTien = hd.TongTien,
            GhiChu = hd.GhiChu,
            TrangThaiOrder = hd.TrangThaiOrder,
            TrangThaiThanhToan = hd.TrangThaiThanhToan,
            PhuongThucThanhToan = hd.PhuongThucThanhToan,
            ThoiGianTao = hd.ThoiGianTao,
            ThoiGianThanhToan = hd.ThoiGianThanhToan,
            TrangThaiBan = table?.TrangThai,
            CanPay = TableWorkflowManager.IsPayable(hd.TrangThaiOrder, hd.TrangThaiThanhToan),
            CanCreateQr = TableWorkflowManager.IsPayable(hd.TrangThaiOrder, hd.TrangThaiThanhToan) && hd.TrangThaiThanhToan != "ChoThanhToanQR",
            CanCompleteTable = hd.MaBan != "MVE" && isPaid && isTableInService,
            CanEditOrder = TableWorkflowManager.CanEditOrder(hd.TrangThaiOrder, hd.TrangThaiThanhToan, out _),
            CanCancelOrder = hd.TrangThaiOrder != "DaHuy",
            Items = OrderDetails
                .Where(d => d.MaHD == maHD)
                .Select(d =>
                {
                    var sp = Products.FirstOrDefault(p => p.MaSP == d.MaSP);
                    return new OrderItemViewModel
                    {
                        MaSP = d.MaSP,
                        TenSP = sp?.TenSP ?? d.MaSP,
                        DonGia = d.DonGia,
                        SoLuong = d.SoLuong,
                        MaKM = d.MaKM,
                        ThanhTien = d.SoLuong * d.DonGia
                    };
                }).ToList()
        };

        return Task.FromResult<OrderReceiptViewModel?>(receipt);
    }

    public Task CompleteTableAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            throw new InvalidOperationException($"Không tìm thấy hóa đơn {maHD}");

        if (hd.MaBan == "MVE")
            return Task.CompletedTask;

        var table = Tables.FirstOrDefault(t => t.MaBan == hd.MaBan);
        if (table == null)
            throw new InvalidOperationException("Bàn không tồn tại.");

        if (!TableWorkflowManager.IsPaid(hd.TrangThaiThanhToan))
            throw new InvalidOperationException("Hóa đơn chưa thanh toán, không thể hoàn tất bàn.");

        if (table.TrangThai != "Đang phục vụ")
            throw new InvalidOperationException("Bàn không ở trạng thái đang phục vụ.");

        table.TrangThai = "Trống";
        return Task.CompletedTask;
    }

    public Task<PaymentCommandResultViewModel> PayCashAsync(string maHD, string? ghiChu)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (!TableWorkflowManager.IsPayable(hd.TrangThaiOrder, hd.TrangThaiThanhToan))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không ở trạng thái có thể thanh toán." });

        hd.TrangThaiThanhToan = "DaThanhToan";
        hd.TrangThaiOrder = "HoanThanh";
        hd.PhuongThucThanhToan = "TienMat";
        hd.ThoiGianThanhToan = DateTime.Now;

        if (!string.IsNullOrEmpty(hd.MaKH))
        {
            var customer = Customers.FirstOrDefault(c => c.MaKH == hd.MaKH);
            if (customer != null)
            {
                var points = CafeOrderCalculator.TinhDiemTichLuy(hd.TongTien);
                customer.DiemTichLuy = (customer.DiemTichLuy ?? 0) + points;
                PointsAwardedByInvoice[maHD] = points;
            }
        }

        Payments.Add(new ThanhToan
        {
            MaThanhToan = _paymentCounter++,
            MaHD = maHD,
            PhuongThuc = "TienMat",
            SoTien = hd.TongTien,
            TrangThai = "DaThanhToan",
            ThoiGianTao = DateTime.Now,
            ThoiGianThanhToan = DateTime.Now,
            GhiChu = ghiChu
        });

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            MaBan = hd.MaBan,
            SoTien = hd.TongTien,
            Message = "Thanh toán tiền mặt thành công."
        });
    }

    public Task<PaymentCommandResultViewModel> CreateQrPaymentAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (!TableWorkflowManager.IsPayable(hd.TrangThaiOrder, hd.TrangThaiThanhToan))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không ở trạng thái có thể tạo mã QR." });

        var maPhien = "QR_" + Guid.NewGuid().ToString("N")[..8];
        hd.TrangThaiThanhToan = "ChoThanhToanQR";
        hd.PhuongThucThanhToan = "QR";

        Payments.Add(new ThanhToan
        {
            MaThanhToan = _paymentCounter++,
            MaHD = maHD,
            MaPhienThanhToan = maPhien,
            PhuongThuc = "QR",
            SoTien = hd.TongTien,
            TrangThai = "ChoThanhToanQR",
            ThoiGianTao = DateTime.Now,
            ThoiGianHetHan = DateTime.Now.AddMinutes(15)
        });

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            MaPhienThanhToan = maPhien,
            SoTien = hd.TongTien,
            Message = "Tạo phiên QR thành công."
        });
    }

    public Task<PaymentCommandResultViewModel> ConfirmQrDemoAsync(string maHD, string maPhienThanhToan)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        hd.TrangThaiThanhToan = "DaThanhToan";
        hd.TrangThaiOrder = "HoanThanh";
        hd.PhuongThucThanhToan = "QR";
        hd.ThoiGianThanhToan = DateTime.Now;

        if (!string.IsNullOrEmpty(hd.MaKH))
        {
            var customer = Customers.FirstOrDefault(c => c.MaKH == hd.MaKH);
            if (customer != null)
            {
                var points = CafeOrderCalculator.TinhDiemTichLuy(hd.TongTien);
                customer.DiemTichLuy = (customer.DiemTichLuy ?? 0) + points;
                PointsAwardedByInvoice[maHD] = points;
            }
        }

        var payment = Payments.LastOrDefault(p => p.MaHD == maHD && p.MaPhienThanhToan == maPhienThanhToan);
        if (payment != null)
        {
            payment.TrangThai = "DaThanhToan";
            payment.ThoiGianThanhToan = DateTime.Now;
        }

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            MaBan = hd.MaBan,
            SoTien = hd.TongTien,
            Message = "Xác nhận thanh toán QR thành công."
        });
    }

    public Task<PaymentCommandResultViewModel> ExpireQrAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (hd.TrangThaiThanhToan == "ChoThanhToanQR")
        {
            hd.TrangThaiThanhToan = "ChuaThanhToan";
        }

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            Message = "Hủy phiên QR thành công."
        });
    }

    public Task<PaymentCommandResultViewModel> CancelOrderAsync(string maHD, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new InvalidOperationException("Vui lòng nhập lý do hủy order.");

        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (hd.TrangThaiOrder == "DaHuy")
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn đã được hủy trước đó." });

        if (!string.IsNullOrEmpty(hd.MaKH) && PointsAwardedByInvoice.TryGetValue(maHD, out var points))
        {
            var customer = Customers.FirstOrDefault(c => c.MaKH == hd.MaKH);
            if (customer != null)
            {
                customer.DiemTichLuy = Math.Max(0, (customer.DiemTichLuy ?? 0) - points);
            }
        }

        hd.TrangThaiOrder = "DaHuy";
        hd.GhiChu = string.IsNullOrEmpty(hd.GhiChu) ? $"Hủy: {lyDo}" : $"{hd.GhiChu} | Hủy: {lyDo}";

        var table = Tables.FirstOrDefault(t => t.MaBan == hd.MaBan);
        if (table != null && table.MaBan != "MVE")
        {
            table.TrangThai = "Trống";
        }

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            MaBan = hd.MaBan,
            Message = "Hủy hóa đơn thành công."
        });
    }

    public Task<PaymentCommandResultViewModel> RefundOrderAsync(string maHD, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new InvalidOperationException("Vui lòng nhập lý do hoàn đơn / hủy thanh toán.");

        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (hd.TrangThaiThanhToan != "DaThanhToan")
            throw new InvalidOperationException("Chỉ hóa đơn đã thanh toán mới có thể thực hiện hoàn đơn.");

        if (!string.IsNullOrEmpty(hd.MaKH) && PointsAwardedByInvoice.TryGetValue(maHD, out var points))
        {
            var customer = Customers.FirstOrDefault(c => c.MaKH == hd.MaKH);
            if (customer != null)
            {
                customer.DiemTichLuy = Math.Max(0, (customer.DiemTichLuy ?? 0) - points);
            }
        }

        hd.TrangThaiOrder = "DaHuy";
        hd.TrangThaiThanhToan = "DaHuy";
        hd.GhiChu = string.IsNullOrEmpty(hd.GhiChu) ? $"Hoàn đơn: {lyDo}" : $"{hd.GhiChu} | Hoàn đơn: {lyDo}";

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            MaBan = hd.MaBan,
            Message = "Hoàn đơn và hủy thanh toán thành công."
        });
    }

    public Task<int> ReconcilePendingPostPaymentAsync(string? maHD = null)
    {
        return Task.FromResult(0);
    }

    public Task<PaymentCommandResultViewModel> EditOrderAsync(string maHD)
    {
        if (!Invoices.TryGetValue(maHD, out var hd))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = "Hóa đơn không tồn tại." });

        if (!TableWorkflowManager.CanEditOrder(hd.TrangThaiOrder, hd.TrangThaiThanhToan, out var reason))
            return Task.FromResult(new PaymentCommandResultViewModel { Success = false, Message = reason ?? "Không thể sửa hóa đơn." });

        hd.TrangThaiOrder = "DangOrder";

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            MaHD = maHD,
            Message = "Mở lại hóa đơn để sửa."
        });
    }

    public Task<PaymentCommandResultViewModel> ProcessExpiredQrAsync()
    {
        int count = 0;
        foreach (var hd in Invoices.Values.Where(h => h.TrangThaiThanhToan == "ChoThanhToanQR"))
        {
            hd.TrangThaiThanhToan = "ChuaThanhToan";
            count++;
        }

        return Task.FromResult(new PaymentCommandResultViewModel
        {
            Success = true,
            SoHoaDonXuLy = count,
            Message = $"Đã xử lý {count} hóa đơn QR hết hạn."
        });
    }

    public Task<List<OrderCustomerViewModel>> SearchCustomersAsync(string keyword)
    {
        var kw = (keyword ?? string.Empty).Trim();
        var result = Customers
            .Where(c => c.SDT.Contains(kw) || c.TenKH.Contains(kw, StringComparison.OrdinalIgnoreCase))
            .Select(c => new OrderCustomerViewModel
            {
                MaKH = c.MaKH,
                TenKH = c.TenKH,
                SDT = c.SDT,
                DiemTichLuy = c.DiemTichLuy
            }).ToList();

        return Task.FromResult(result);
    }

    public Task<OrderCustomerViewModel> CreateQuickCustomerAsync(OrderQuickCustomerCreateViewModel model)
    {
        var nextCode = $"KH{Customers.Count + 1:D2}";
        var newCustomer = new KhachHang
        {
            MaKH = nextCode,
            TenKH = model.TenKH,
            SDT = model.SDT,
            DiemTichLuy = 0
        };
        Customers.Add(newCustomer);

        return Task.FromResult(new OrderCustomerViewModel
        {
            MaKH = nextCode,
            TenKH = model.TenKH,
            SDT = model.SDT,
            DiemTichLuy = 0
        });
    }
}
