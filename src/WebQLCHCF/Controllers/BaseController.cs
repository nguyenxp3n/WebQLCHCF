using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebQLCHCF.Controllers;

[WebQLCHCF.Security.RoleAuthorize]
public abstract class BaseController : Controller
{
    protected static string FriendlyControllerError(
        Exception ex,
        string fallback = "Không thể thực hiện thao tác. Vui lòng kiểm tra dữ liệu liên quan."
    )
    {
        Console.WriteLine($"[CONTROLLER ERROR] {ex.GetType().FullName}: {ex.Message} | Inner: {ex.InnerException?.Message}");

        var message = ex.Message;
        if (string.IsNullOrWhiteSpace(message))
            return fallback;

        if (ex is InvalidOperationException)
            return message;

        if (message.Contains("Mật khẩu không đúng", StringComparison.OrdinalIgnoreCase))
            return "Mật khẩu không đúng.";

        if (message.Contains("Bạn cần check-in", StringComparison.OrdinalIgnoreCase))
            return "Bạn cần check-in trước khi order.";

        if (message.Contains("Hiện không còn bàn trống", StringComparison.OrdinalIgnoreCase))
            return "Hiện không còn bàn trống.";

        if (message.Contains("Chưa có bàn Mang về", StringComparison.OrdinalIgnoreCase))
            return "Chưa có bàn Mang về trong dữ liệu.";

        if (message.Contains("Sản phẩm hiện không ở trạng thái Đang bán", StringComparison.OrdinalIgnoreCase)
            || message.Contains("hiện không ở trạng thái Đang bán", StringComparison.OrdinalIgnoreCase))
        {
            return "Sản phẩm hiện không ở trạng thái Đang bán.";
        }

        if (message.Contains("kho không đủ", StringComparison.OrdinalIgnoreCase)
            || message.Contains("hoàn tác", StringComparison.OrdinalIgnoreCase)
            || message.Contains("nguyên liệu đã được sử dụng", StringComparison.OrdinalIgnoreCase))
        {
            return "Không thể xóa vì nguyên liệu đã được sử dụng, kho không đủ để hoàn tác.";
        }

        if (message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
            || message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase))
        {
            return "Dữ liệu đang được sử dụng nên không thể xóa.";
        }

        if (message.Contains("out of range", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Arithmetic overflow", StringComparison.OrdinalIgnoreCase)
            || message.Contains("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase))
        {
            return "Dữ liệu nhập không hợp lệ hoặc vượt quá giới hạn cho phép.";
        }

        return fallback;
    }

    public override void OnActionExecuting(ActionExecutingContext c)
    {
        var hasAllowAnonymous =
            c.ActionDescriptor.EndpointMetadata?.OfType<IAllowAnonymous>().Any() == true
            || c.HttpContext.GetEndpoint()?.Metadata.OfType<IAllowAnonymous>().Any() == true
            || c.Filters.Any(f => f is IAllowAnonymous)
            || (c.ActionDescriptor is ControllerActionDescriptor cad && cad.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any());

        if (hasAllowAnonymous)
        {
            base.OnActionExecuting(c);
            return;
        }

        if (string.IsNullOrEmpty(HttpContext.Session.GetString("MaTK")))
        {
            c.Result = RedirectToAction("Login", "Account");
            return;
        }
        base.OnActionExecuting(c);
    }
}
