using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Naviam.Domain.Abstract;
using Naviam.WebUI.Controllers;

namespace Naviam.UnitTests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        //public void BeforeTest()
        //{
        //    var mockPrincipal = new Mock<IPrincipal>();
        //    var mockIdentity = new Mock<IIdentity>();
        //    mockPrincipal.Setup(m => m).Returns(IPrincipal)
        //    mockIdentity.Setup(m => m.Name).Returns("s@s.s");
        //}

        [TestMethod]
        public void LogOn()
        {
            var mock = new Mock<IUserAccountRepository>();
            var controller = new AccountController(mock.Object);

            var result = controller.LogOn() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("LogOn", result.ViewName);
        }

        [TestMethod]
        public void LogOff()
        {
            var mock = new Mock<IUserAccountRepository>();
            var controller = new AccountController(mock.Object);

            var identity = new Mock<IIdentity>();

            var principal = new Mock<IPrincipal>();
            principal.Setup(p => p.Identity).Returns(identity.Object);
            // ... mock IPrincipal as you wish

            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(x => x.User).Returns(principal.Object);
            // ... mock other httpContext's properties, methods, as needed

            var reqContext = new RequestContext(httpContext.Object, new RouteData());

            controller.ControllerContext =
                new ControllerContext(reqContext, controller);

            var result = controller.LogOff() as ViewResult;

            // check if user has been signed out
            Assert.IsFalse(controller.User.Identity.IsAuthenticated);

            Assert.IsNotNull(result);
            // User should see the logon page
            Assert.AreEqual("LogOn", result.ViewName);
        }
    }
}
