using WebQLCHCF.ViewModels;

namespace WebQLCHCF.Services;

public interface IOrderService
{
    Task<OrderCreateViewModel> GetCreateModelAsync();
    Task<OrderCreateViewModel> GetCreateModelForEditAsync(string maHD);
    Task<OrderCreateViewModel> GetCreateModelAsync(OrderCreateViewModel model);
    Task<string> CreateOrderAsync(OrderCreateViewModel model, string maNV);
    Task<string> UpdateOrderAsync(OrderCreateViewModel model, string maNV);
    Task<OrderReceiptViewModel?> GetReceiptAsync(string maHD);
    Task<bool> IsEmployeeCheckedInAsync(string maNV);
    Task CompleteTableAsync(string maHD);
    Task<PaymentCommandResultViewModel> PayCashAsync(string maHD, string? ghiChu);
    Task<PaymentCommandResultViewModel> CreateQrPaymentAsync(string maHD);
    Task<PaymentCommandResultViewModel> ConfirmQrDemoAsync(string maHD, string maPhienThanhToan);
    Task<PaymentCommandResultViewModel> ExpireQrAsync(string maHD);
    Task<PaymentCommandResultViewModel> CancelOrderAsync(string maHD, string lyDo);
    Task<PaymentCommandResultViewModel> EditOrderAsync(string maHD);
    Task<PaymentCommandResultViewModel> ProcessExpiredQrAsync();
    Task<List<OrderCustomerViewModel>> SearchCustomersAsync(string keyword);
    Task<OrderCustomerViewModel> CreateQuickCustomerAsync(OrderQuickCustomerCreateViewModel model);
}
