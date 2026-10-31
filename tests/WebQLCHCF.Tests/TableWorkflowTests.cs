using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class TableWorkflowTests
{
    [Fact]
    public void ResolveTableStatusOnOrder_TaiQuan_ChuyenSangDangPhucVu()
    {
        string newStatus = TableWorkflowManager.ResolveTableStatusOnOrder(
            OrderWorkflowConstants.HinhThucTaiQuan, "B01", OrderWorkflowConstants.TrangThaiBanTrong);

        Assert.Equal(OrderWorkflowConstants.TrangThaiBanDangPhucVu, newStatus);
    }

    [Fact]
    public void ResolveTableStatusOnOrder_MangVe_GiuNguyenTrangThai()
    {
        string newStatus = TableWorkflowManager.ResolveTableStatusOnOrder(
            OrderWorkflowConstants.HinhThucMangVe, "MVE", "Khác");

        Assert.Equal("Khác", newStatus);
    }

    [Theory]
    [InlineData("Trống", true)]
    [InlineData("  Trống  ", true)]
    [InlineData("Đang phục vụ", false)]
    [InlineData("Tạm ngưng", false)]
    [InlineData(null, false)]
    public void IsTableAvailable_KiemTraBanCoTheDat(string? status, bool expected)
    {
        bool isAvailable = TableWorkflowManager.IsTableAvailable(status);
        Assert.Equal(expected, isAvailable);
    }

    [Fact]
    public void CanEditOrder_HoaDonDaHuy_TraVeFalseKemLyDo()
    {
        bool canEdit = TableWorkflowManager.CanEditOrder(
            OrderWorkflowConstants.TrangThaiOrderDaHuy,
            OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan,
            out string? reason);

        Assert.False(canEdit);
        Assert.Equal("Hóa đơn đã hủy, không thể sửa.", reason);
    }

    [Fact]
    public void CanEditOrder_HoaDonDaThanhToan_TraVeFalseKemLyDo()
    {
        bool canEdit = TableWorkflowManager.CanEditOrder(
            OrderWorkflowConstants.TrangThaiOrderChoThanhToan,
            OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan,
            out string? reason);

        Assert.False(canEdit);
        Assert.Equal("Hóa đơn đã thanh toán, không thể sửa.", reason);
    }

    [Fact]
    public void CanEditOrder_HoaDonHopLe_TraVeTrueVaKhongCoLyDo()
    {
        bool canEdit = TableWorkflowManager.CanEditOrder(
            OrderWorkflowConstants.TrangThaiOrderChoThanhToan,
            OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan,
            out string? reason);

        Assert.True(canEdit);
        Assert.Null(reason);
    }

    [Theory]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderChoThanhToan, OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan, true)]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderChoThanhToan, OrderWorkflowConstants.TrangThaiThanhToanChoThanhToanQr, true)]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderChoThanhToan, OrderWorkflowConstants.TrangThaiThanhToanDaThanhToan, false)]
    [InlineData(OrderWorkflowConstants.TrangThaiOrderDaHuy, OrderWorkflowConstants.TrangThaiThanhToanChuaThanhToan, false)]
    public void IsPayable_XacDinhChinhXacHoaDonCoTheThanhToan(
        string trangThaiOrder, string trangThaiThanhToan, bool expected)
    {
        bool payable = TableWorkflowManager.IsPayable(trangThaiOrder, trangThaiThanhToan);
        Assert.Equal(expected, payable);
    }
}
