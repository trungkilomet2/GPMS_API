using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Security.Claims;

namespace GPMS.TEST.TestCommon;

internal static class ControllerTestHelper
{
    public static void AttachHttpContext(ControllerBase controller, ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "http";
        if (user is not null)
        {
            httpContext.User = user;
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var mockUrl = new Mock<IUrlHelper>();
        mockUrl.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/fake-url");
        controller.Url = mockUrl.Object;
    }

    public static ClaimsPrincipal BuildUserWithId(int id)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Name, "tester")
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }
}