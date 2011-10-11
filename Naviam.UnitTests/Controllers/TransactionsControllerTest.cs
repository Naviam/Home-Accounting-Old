using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Naviam.WebUI.Controllers;
using System.Web.Mvc;
using Moq;
using Naviam.Domain.Concrete;
using Naviam.Data;
using Naviam.WebUI.Models;
using System.Web.Script.Serialization;

namespace Naviam.UnitTests.Controllers
{
    [TestClass]
    public class TransactionsControllerTest
    {
        [TestMethod]
        public void DefaultGetTransactions()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(5, trans.Count);
        }

        [TestMethod]
        public void TransactionsFilterByAccountId()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            pageContext.AccountId = 2;

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;
            var tr = trans.FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, trans.Count);
            Assert.AreEqual(2, tr.AccountId);
        }

        [TestMethod]
        public void TransactionsFilterByTagId()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() {Name = "TagId", Value = "20"});
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);
            
            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;
            var tr = trans.FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, trans.Count);
            Assert.AreEqual(3, tr.AccountId);
        }

        [TestMethod]
        public void TransactionsFilterByMerchant()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "Merchant", Value = "find Merchant" });
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;
            var tr = trans.FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, trans.Count);
            Assert.AreEqual("find Merchant", tr.Merchant);
        }

        [TestMethod]
        public void TransactionsFilterByTagName()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "TagName", Value = "test1 Tag" });
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;
            var tr = trans.FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(2, trans.Count);
            Assert.AreEqual(1, tr.TagIds.Count);
            Assert.AreEqual("20", tr.TagIds.FirstOrDefault());
        }

        [TestMethod]
        public void TransactionsFilterByCategoryId()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "CategoryId", Value = "200", Type = "int" });
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;
            var tr = trans.FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, trans.Count);
            Assert.AreEqual(200, tr.CategoryId);
        }

        [TestMethod]
        public void TransactionsFilterByString()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "ByString", Value = "test1" });
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(2, trans.Count);
            Assert.IsNotNull(trans.FirstOrDefault(s => s.Merchant == "test1 Merchant"));
            Assert.IsNotNull(trans.FirstOrDefault(s => s.CategoryId == 200));

            // arrange again
            fltrs.Clear();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "ByString", Value = "test2" });
            js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);
            // act
            result = controller.GetTransactions(paging, pageContext) as JsonResult;
            data = result.Data;
            trans = data.items;
            // Assert
            Assert.AreEqual(1, trans.Count);
            Assert.IsNotNull(trans.FirstOrDefault(s => s.Merchant == "test2 Merchant"));
            Assert.IsNotNull(trans.FirstOrDefault(s => s.CategoryId == 210));
        }

        [TestMethod]
        public void TransactionsFilterByStringCategoryMerchant()
        {
            // arrange
            var controller = GetController();
            TransactionsController.Paging paging = new TransactionsController.Paging();
            PageContext pageContext = new PageContext();
            var fltrs = new List<TransactionsController.FilterHolder>();
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "ByString", Value = "Merchant" });
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "Merchant", Value = "test1 Merchant" });
            fltrs.Add(new TransactionsController.FilterHolder() { Name = "Category", Value = "test Cat3" });
            JavaScriptSerializer js = new JavaScriptSerializer();
            paging.Filter = js.Serialize(fltrs);

            // act
            var result = controller.GetTransactions(paging, pageContext) as JsonResult;
            dynamic data = result.Data;
            List<Transaction> trans = data.items;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, trans.Count);
            Assert.IsNotNull(trans.FirstOrDefault(s => s.Merchant == "test1 Merchant"));
            Assert.IsNotNull(trans.FirstOrDefault(s => s.CategoryId == 221));
        }


        private static IEnumerable<Transaction> GetTestTrans(int? companyId)
        {
            var res = new List<Transaction>();
            var trans = new Transaction()
            {
                AccountId = 1,
                Amount = 100,
                CategoryId = 221,
                CurrencyId = 1,
                Date = new DateTime(),
                Description = "test Description",
                Direction = TransactionDirections.Expense,
                Id = 1,
                IncludeInTax = false,
                Merchant = "test1 Merchant",
                Notes = "test Notes"
            };
            trans.TagIds.Add("1");
            res.Add(trans);
            
            trans = trans.Clone();
            trans.Id = 2;
            trans.Merchant = "find Merchant";
            res.Add(trans);
            
            trans = trans.Clone();
            trans.Id = 3;
            trans.CategoryId = 200;
            trans.Description = "find Category";
            trans.Merchant = "new Merchant";
            res.Add(trans);
            
            trans = trans.Clone();
            trans.Id = 4;
            trans.AccountId = 2;
            trans.CategoryId = 210;
            trans.Merchant = "test2 Merchant";
            trans.Description = "find Account";
            trans.TagIds = new List<string>();
            trans.TagIds.Add("20");
            res.Add(trans);

            trans = trans.Clone();
            trans.Id = 5;
            trans.AccountId = 3;
            trans.CategoryId = 1;
            trans.Description = "find Tag";
            trans.TagIds = new List<string>();
            trans.Merchant = null;
            trans.TagIds.Add("20");
            trans.TagIds.Add("21");
            res.Add(trans);
         
            return res;
        }

        private static List<Category> GetTestCats(int? userId)
        {
            var res = new List<Category>();
            res.Add(new Category() { Id = 200, Name = "test1 Cat" });
            res.Add(new Category() { Id = 210, Name = "test2 Cat2" });
            res.Add(new Category() { Id = 221, Name = "test Cat3" });
            return res;
        }

        private static List<Tag> GetTestTags(int? userId)
        {
            var res = new List<Tag>();
            res.Add(new Tag() { Id = 20, Name = "test1 Tag" });
            res.Add(new Tag() { Id = 21, Name = "test2 Tag2" });
            return res;
        }

        private static TransactionsController GetController()
        {
            var reps = new Mock<TransactionsRepository>();
            reps.Setup(m => m.GetTransactions(It.IsAny<int?>())).Returns<int?>(p => GetTestTrans(p));
            var car_rep = new Mock<CategoriesRepository>();
            car_rep.Setup(m => m.GetAll(It.IsAny<int?>())).Returns<int?>(p => GetTestCats(p));
            var tag_rep = new Mock<TagsRepository>();
            tag_rep.Setup(m => m.GetAll(It.IsAny<int?>())).Returns<int?>(p => GetTestTags(p));

            var controller = new TransactionsController(reps.Object, car_rep.Object, tag_rep.Object);
            //set user and def company
            var user = new UserProfile();
            user.DefaultCompany = 1;
            var comps = new List<Company>();
            comps.Add(new Company() { Id = 1 });
            user.Companies = comps;
            controller.CurrentUser = user;

            return controller;
        }

    }
}
