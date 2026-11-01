using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using WebQLCHCF.Controllers;
using WebQLCHCF.Data;
using WebQLCHCF.Security;

namespace WebQLCHCF.Tests.TestHarness;

public static class TestControllerContextFactory
{
    private class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    public static AppDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? ("TestDb_" + Guid.NewGuid().ToString("N")))
            .Options;

        return new AppDbContext(options);
    }

    public static TController SetupController<TController>(
        TController controller,
        HttpContext? httpContext = null,
        RouteData? routeData = null) where TController : Controller
    {
        httpContext ??= TestHttpContextFactory.CreateAnonymous();
        routeData ??= new RouteData();

        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = typeof(TController).Name.Replace("Controller", ""),
            ActionName = "Index",
            ControllerTypeInfo = typeof(TController).GetTypeInfo()
        };

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        controller.ControllerContext = new ControllerContext(actionContext);
        controller.TempData = new TempDataDictionary(httpContext, new EmptyTempDataProvider());

        return controller;
    }

    public static IActionResult? ExecuteSecurityPipeline<TController>(
        TController controller,
        string actionName,
        params object?[] args) where TController : Controller
    {
        var controllerType = controller.GetType();
        var httpContext = controller.HttpContext ?? TestHttpContextFactory.CreateAnonymous();
        var routeData = controller.RouteData ?? new RouteData();

        var method = controllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase) &&
                                 m.GetParameters().Length == args.Length)
            ?? controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase))
            ?? controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public).First();

        var endpointMetadata = new List<object>();
        if (method != null)
        {
            endpointMetadata.AddRange(method.GetCustomAttributes(true));
        }
        endpointMetadata.AddRange(controllerType.GetCustomAttributes(true));

        var actionDescriptor = new ControllerActionDescriptor
        {
            ControllerName = controllerType.Name.Replace("Controller", ""),
            ActionName = actionName,
            MethodInfo = method!,
            ControllerTypeInfo = controllerType.GetTypeInfo(),
            EndpointMetadata = endpointMetadata
        };

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
        controller.ControllerContext = new ControllerContext(actionContext);

        var executingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller);

        // 1. RoleAuthorizeAttribute check (Action or Class level)
        var hasAllowAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
        if (!hasAllowAnonymous)
        {
            // Action level role filter
            var actionRoleAttr = method?.GetCustomAttribute<RoleAuthorizeAttribute>(true);
            if (actionRoleAttr != null)
            {
                actionRoleAttr.OnActionExecuting(executingContext);
                if (executingContext.Result != null)
                    return executingContext.Result;
            }

            // Controller level role filter
            var classRoleAttr = controllerType.GetCustomAttribute<RoleAuthorizeAttribute>(true);
            if (classRoleAttr != null)
            {
                classRoleAttr.OnActionExecuting(executingContext);
                if (executingContext.Result != null)
                    return executingContext.Result;
            }
        }

        // 2. BaseController OnActionExecuting check
        if (controller is BaseController)
        {
            var baseOnActionExecuting = typeof(BaseController).GetMethod(
                "OnActionExecuting",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            baseOnActionExecuting?.Invoke(controller, new object[] { executingContext });
            if (executingContext.Result != null)
                return executingContext.Result;
        }

        return executingContext.Result;
    }
}
