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
                    decimal sumAmount = 0;
                    foreach (var trans in statRes.Transactions)
                    {
                        //Add to DB
                        if (trans.AccountAmount != 0)
                        {
                            var dbTrans = new Transaction()
                            {
                                Amount = Math.Abs(trans.AccountAmount),
                                Date = trans.TransactionDate,
                                Description = trans.OperationDescription,
                                Direction = trans.AccountAmount > 0 ? TransactionDirections.Income : TransactionDirections.Expense,
                                Merchant = trans.Place,
                                TransactionType = TransactionTypes.Cash,
                                AccountId = accId,
                                IncludeInTax = false,
                                //TODO: assign null and resolve on db side
                                CategoryId = 20
                            };
                            var res = reps.Insert(dbTrans, companyId);
                            if (res == 0)
                                sumAmount += trans.AccountAmount;
                        }
                    }
                    AccountsRepository.ChangeBalance(accId, companyId, sumAmount);
                    //reset redis
                    //TransactionsDataAdapter.ResetCache(CurrentUser.CurrentCompany);
                    result = "ok";
                    if (Request.UrlReferrer != null) Response.Redirect(Request.UrlReferrer.PathAndQuery);
                }
            }
            return result;
        }

    }
}
