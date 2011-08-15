using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Naviam.Domain.Concrete;
using Naviam.Entities.User;
using Naviam.UnitTests.Mocks;
using Naviam.WebUI.Controllers;
using Naviam.WebUI.Models;

namespace Naviam.UnitTests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        [TestMethod]
        public void LoginGet()
        {
            // arrange
            var controller = GetAccountController();

            // act
            var result = controller.LogOn() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LoginPostRedirectsTransactionsIfLoginSuccessful()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (RedirectToRouteResult)controller.LogOn(new LogOnModel { UserName = "someUser", Password = "goodPass", RememberMe = true }, null);

            // Assert
            Assert.AreEqual("Transactions", result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void LogOff()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (RedirectToRouteResult)controller.LogOff();
            
            // Assert
            Assert.AreEqual("Account", result.RouteValues["controller"]);
            Assert.AreEqual("LogOn", result.RouteValues["action"]);
        }

        private static AccountController GetAccountController()
        {
            var membershipRepository = new Mock<MembershipRepository>();
            var user = new UserProfile();
            var cacheWrapper = new Mock<ICacheWrapper>();

            membershipRepository.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns(user);
            IFormsAuthentication formsAuth = new MockFormsAuthenticationService();
            var contextBase = MvcMockHelpers.FakeHttpContext(); // new MockHttpContext();

            var controller = new AccountController(formsAuth, cacheWrapper.Object, membershipRepository.Object);
            controller.ControllerContext = new ControllerContext(contextBase, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(contextBase, new RouteData()), new RouteCollection());
            return controller;
        }
    }
}
