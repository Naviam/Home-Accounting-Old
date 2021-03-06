﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using Naviam.WebUI.Helpers.Parsers;
using Naviam.DAL;
using Naviam.Data;
using Naviam.Domain.Concrete;
using Naviam.WebUI.Models;

namespace Naviam.WebUI.Controllers
{
    public class AttachmentsController : BaseController
    {

        [HttpPost]
        public string UploadStatement(int? accId)
        {
            //throw new NotImplementedException();
            var result = "error";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files["fileToUpload"];
                if (file != null && !String.IsNullOrEmpty(file.FileName))
                {
                    string fileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                    fileName = fileName + Path.GetExtension(file.FileName);
                    string fileNameToSave = string.Format("{0}{1}", System.IO.Path.GetTempPath(), fileName);
                    file.SaveAs(fileNameToSave);
                    var statRes = ParsersFactory.ParseFile(fileNameToSave);
                    System.IO.File.Delete(fileNameToSave);
                    if (statRes == null)
                        throw new Exception("Invalid file format");
                    var companyId = CurrentUser.CurrentCompany;
                    var reps = new TransactionsRepository();
                    var repCat = new CategoriesRepository();
                    var repRule = new RulesRepository(); 
                    var account = AccountsRepository.GetAccount(accId, companyId);
                    decimal sumAmount = 0;
                    var dbTransList = new List<Transaction>();
                    foreach (var trans in statRes.Transactions)
                    {
                        var categoryId = repCat.FindCategoryMerchant(CurrentUser.Id, trans.Place.Trim());
                        if (trans.AccountAmount != 0)
                        {
                            var dbTrans = new Transaction()
                            {
                                Amount = Math.Abs(trans.AccountAmount),
                                Date = trans.TransactionDate,
                                Description = repRule.FindDescriptionMerchant(CurrentUser.Id, trans.Place.Trim()), //trans.Place,
                                Direction = trans.AccountAmount > 0 ? TransactionDirections.Income : TransactionDirections.Expense,
                                Merchant = trans.Place,
                                TransactionType = TransactionTypes.Statement,
                                AccountId = accId,
                                IncludeInTax = false,
                                Notes = trans.OperationDescription,
                                //CategoryId = 20
                                CategoryId = categoryId,
                                CurrencyId = account.CurrencyId
                            };
                            dbTransList.Add(dbTrans);
                            sumAmount += trans.AccountAmount;
                        }
                    }
                    //Add to DB
                    var res = reps.BatchInsert(dbTransList, companyId);
                    if (res == 0)
                    {
                        new AccountsRepository().ChangeBalance(accId, companyId, sumAmount);
                        //reset redis
                        //reps.ResetCache(companyId);
                        result = "ok";
                    }
                    if (Request.UrlReferrer != null) Response.Redirect(Request.UrlReferrer.PathAndQuery);
                }
            }
            return result;
        }

    }
}
