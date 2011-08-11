using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
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
        //    mockPrincipal.Setup(m=>m).Returns(IPrincipal)
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

            var result = controller.LogOff() as ViewResult;

            // check if user has been signed out
            Assert.IsNull(controller.User);

            Assert.IsNotNull(result);
            // User should see the logon page
            Assert.AreEqual("LogOn", result.ViewName);
        }
    }
}
