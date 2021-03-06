﻿using System;
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
    public class SmsControllerTest
    {
        static string testMessage = @"
4..0692 
Service payment from card 
Uspeshno 
2011-09-01 00:00:00 
Summa: 4828 BYR 
Ostatok: 56552 BYR 
Na vremya: 11:11:46 
//RBS Balance loader
";

        [TestMethod]
        public void RecieveSmsWithWrongKey()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = controller.RecieveMessage("givemeaccesstotoyou11", "GETWAY1", "BelSwissBank", "to", testMessage) as JsonResult;
            dynamic data = result.Data;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("error", data);
        }

        [TestMethod]
        public void RecieveSmsFromBelSwissBank()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = controller.RecieveMessage("givemeaccesstotoyou", "GETWAY1", "BelSwissBank", "to", testMessage) as JsonResult;
            dynamic data = result.Data;
            //int result = 0;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ok", data);
        }

        private static Modem GetTestModem(string gateway)
        {
            var res = new Modem();
            res.Id = 1;
            return res;
        }

        private static Account GetTestAccountBySms(string cardNumber, int? id_modem, int? id_bank)
        {
            var res = new Account();
            res.Id = 1;
            return res;
        }

        private static Currency GetTestCurrencyByShortName(string shortName)
        {
            var res = new Currency();
            res.Id = 1;
            return res;
        }


        private static SmsController GetController()
        {
            var modems_rep = new Mock<ModemsRepository>();
            modems_rep.Setup(m => m.GetModemByGateway(It.IsAny<string>())).Returns<string>(p => GetTestModem(p));
            var accs_rep = new Mock<AccountsRepository>();
            //Account account = null;
            accs_rep.Setup(m => m.GetAccountBySms(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>())).Returns<string, int?, int?>((p1, p2, p3)=> GetTestAccountBySms(p1, p2, p3));
            var trans_rep = new Mock<TransactionsRepository>();
            var currency_rep = new Mock<CurrenciesRepository>();
            currency_rep.Setup(m => m.GetCurrencyByShortName(It.IsAny<string>())).Returns<string>(p => GetTestCurrencyByShortName(p));
            var cats_rep = new Mock<CategoriesRepository>();

            var controller = new SmsController(modems_rep.Object, trans_rep.Object, accs_rep.Object, currency_rep.Object, cats_rep.Object);
            return controller;
        }
    }
}
