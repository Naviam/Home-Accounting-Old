using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Naviam.Domain.Concrete;
using Naviam.Entities.User;
using Naviam.UnitTests.Mocks;
using Naviam.WebUI.Controllers;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Helpers.Cookies;
using Naviam.WebUI.Models;
using Naviam.WebUI.Resources;

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
        public void LoginPostRedirectsHomeIfLoginSuccessfulButNoReturnUrlGiven()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (RedirectToRouteResult)controller.LogOn(
                new LogOnModel { UserName = "someUser", Password = "goodPass", RememberMe = true },
                null);

            // Assert
            Assert.AreEqual("Transactions", result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void LoginPostRedirectsToReturnUrlIfLoginSuccessfulAndReturnUrlGiven()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (RedirectResult)controller.LogOn(
                new LogOnModel { UserName = "someUser", Password = "goodPass", RememberMe = true }, 
                "/someUrl");

            // Assert
            Assert.AreEqual("/someUrl", result.Url);
        }

        [TestMethod]
        public void LoginPostReturnsViewIfPasswordNotSpecified()
        {
            // Arrange
            var controller = GetAccountController();
            LogOnModel[] model = {new LogOnModel {UserName = "someUser", Password = String.Empty, RememberMe = true}};

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model[0], null), model[0]);

            // Assert
            var resultModel = (LogOnModel)result.Model;
            Assert.AreEqual(true, resultModel.RememberMe);
            Assert.AreEqual(ValidationStrings.PasswordRequired, result.ViewData.ModelState["password"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void LoginPostReturnsViewIfUsernameNotSpecified()
        {
            // Arrange
            var controller = GetAccountController();
            LogOnModel[] model = { new LogOnModel { UserName = "", Password = "1", RememberMe = false } };

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model[0], null), model[0]);

            // Assert
            var resultModel = (LogOnModel)result.Model;
            Assert.AreEqual(false, resultModel.RememberMe);
            Assert.AreEqual(ValidationStrings.UserNameRequired, result.ViewData.ModelState["username"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void LoginPostReturnsViewIfUsernameOrPasswordIsIncorrect()
        {
            // Arrange
            var controller = GetAccountController();

            LogOnModel[] model = { new LogOnModel { UserName = "s@s.s", Password = "badPass", RememberMe = true } };

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model[0], null), model[0]);

            // Assert
            var resultModel = (LogOnModel)result.Model;
            Assert.AreEqual(true, resultModel.RememberMe);
            Assert.AreEqual(ValidationStrings.UsernameOrPasswordIsIncorrect, result.ViewData.ModelState["_FORM"].Errors[0].ErrorMessage);
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

        [TestMethod]
        public void RegisterGet()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Register", result.ViewName);
        }

        [TestMethod]
        public void RegisterPostRedirectsHomeIfRegistrationSuccessful()
        {
            // Arrange
            var controller = GetAccountController();
            var model = new RegisterModel {Email = "email", Password = "goodPass", ConfirmPassword = "goodPass"};

            // Act
            var result = (RedirectToRouteResult)controller.CallWithModelValidation(m => m.Register(model), model);

            // Assert
            Assert.AreEqual("Transactions", result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        private static AccountController GetAccountController()
        {
            var membershipRepository = new Mock<MembershipRepository>();
            var user = new UserProfile();

            var cookieContainer = new Mock<ICookieContainer>();
            cookieContainer.Setup(c => c.SetValue(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<DateTime>()));
            cookieContainer.Setup(c => c.SetAuthCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));

            membershipRepository.Setup(m => m.GetUser(It.IsAny<string>(), It.IsAny<string>())).Returns(user);

            IFormsAuthentication formsAuth = new MockFormsAuthenticationService();
            var contextBase = MvcMockHelpers.FakeHttpContext(); // new MockHttpContext();

            var controller = new AccountController(
                cookieContainer.Object, formsAuth, membershipRepository.Object);
            controller.ControllerContext = new ControllerContext(contextBase, new RouteData(), controller);
            controller.Url = new UrlHelper(new RequestContext(contextBase, new RouteData()), new RouteCollection());
            return controller;
        }
    }
}
