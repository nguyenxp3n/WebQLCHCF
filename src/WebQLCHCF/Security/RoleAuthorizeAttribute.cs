using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebQLCHCF.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RoleAuthorizeAttribute : ActionFilterAttribute
{
    private readonly HashSet<string> _allowedRoles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _allowedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var maTK = session.GetString("MaTK");
        if (string.IsNullOrWhiteSpace(maTK))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (_allowedRoles.Count == 0)
        {
            base.OnActionExecuting(context);
            return;
        }

        var vaiTro = session.GetString("VaiTro");
        if (string.IsNullOrWhiteSpace(vaiTro) || !_allowedRoles.Contains(vaiTro.Trim()))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}
