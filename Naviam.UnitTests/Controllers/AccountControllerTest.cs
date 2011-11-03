using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Naviam.Domain.Concrete;
using Naviam.UnitTests.Mocks;
using Naviam.WebUI.Controllers;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Helpers.Cookies;
using Naviam.WebUI.Models;
using Naviam.WebUI.Resources;
using Naviam.Data;

namespace Naviam.UnitTests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        #region LOGIN / LOGOFF TESTS
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
            var controller = GetAccountController(new UserProfile());

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
            var controller = GetAccountController(new UserProfile());

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
            var controller = GetAccountController(new UserProfile());
            var model = new LogOnModel { UserName = "someUser", Password = String.Empty, RememberMe = true };

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model, null), model);

            // Assert
            var resultModel = (LogOnModel)result.Model;
            Assert.AreEqual(true, resultModel.RememberMe);
            Assert.AreEqual(ValidationStrings.PasswordRequired, result.ViewData.ModelState["password"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void LoginPostReturnsViewIfUsernameNotSpecified()
        {
            // Arrange
            var controller = GetAccountController(new UserProfile());
            var model = new LogOnModel { UserName = "", Password = "1", RememberMe = false };

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model, null), model);

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
            var model = new LogOnModel { UserName = "s@s.s", Password = "badPass", RememberMe = true };

            // Act
            var result = (ViewResult)controller.CallWithModelValidation(m => m.LogOn(model, null), model);

            // Assert
            var resultModel = (LogOnModel)result.Model;
            Assert.AreEqual(true, resultModel.RememberMe);
            Assert.AreEqual(ValidationStrings.UsernameOrPasswordIsIncorrect, result.ViewData.ModelState[String.Empty].Errors[0].ErrorMessage);
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
        #endregion

        #region CONFRMATION TESTS
        [TestMethod]
        public void ConfirmationGet()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Confirmation("acc", "email");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Confirmation", result.ViewName);
        }

        //[TestMethod]
        //public void ConfirmationPostRedirectsHomeIfConfirmationSuccessful()
        //{
        //    // Arrange
        //    var controller = GetAccountController(new UserProfile());

        //    // Act
        //    var result = controller.Confirmation(
        //        new ConfirmationModel { ApproveCode = "someCode", Email="someEmail" });

        //    // Assert
        //    Assert.AreEqual("Transactions", result.RouteValues["controller"]);
        //    Assert.AreEqual("Index", result.RouteValues["action"]);
        //}
        #endregion

        #region REGISTER TESTS
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
            var model = new RegisterModel { UserName = "email", Password = "goodPass", ConfirmPassword = "goodPass" };

            // Act
            var result = (RedirectToRouteResult)controller.CallWithModelValidation(m => m.Register(model), model);

            // Assert
            Assert.AreEqual("Transactions", result.RouteValues["controller"]);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfEmailNotSpecified()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "", Password = "goodPass", ConfirmPassword = "goodPass" });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("You must specify an email address.", result.ViewData.ModelState["email"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfNewPasswordDoesNotMatchConfirmPassword()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "email", Password = "goodPass", ConfirmPassword = "goodPass2" });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("The new password and confirmation password do not match.", result.ViewData.ModelState["_FORM"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfPasswordIsNull()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "email", Password = null, ConfirmPassword = null });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("You must specify a password of 6 or more characters.", result.ViewData.ModelState["password"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfPasswordIsTooShort()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "email", Password = "12345", ConfirmPassword = "12345" });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("You must specify a password of 6 or more characters.", result.ViewData.ModelState["password"].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfRegistrationFails()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "DuplicateUserName", Password = "badPass", ConfirmPassword = "badPass" });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("Username already exists. Please enter a different user name.", result.ViewData.ModelState[""].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfUsernameNotSpecified()
        {
            // Arrange
            var controller = GetAccountController();

            // Act
            var result = (ViewResult)controller.Register(new RegisterModel { UserName = "email", Password = "badPass", ConfirmPassword = "badPass" });

            // Assert
            Assert.AreEqual(6, result.ViewData["PasswordLength"]);
            Assert.AreEqual("You must specify a username.", result.ViewData.ModelState["username"].Errors[0].ErrorMessage);
        }
        #endregion

        private static AccountController GetAccountController(UserProfile user = null)
        {
            var membershipRepository = new Mock<MembershipRepository>();

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
