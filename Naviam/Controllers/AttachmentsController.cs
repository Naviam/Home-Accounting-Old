using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using Naviam.WebUI.Helpers.Parsers;
using Naviam.DAL;
using Naviam.Data;

namespace Naviam.WebUI.Controllers
{
    public class AttachmentsController : BaseController
    {

        [HttpPost]
        public string UploadStatement()
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
                    //TODO: check that user have account, bank...
                    foreach (var trans in statRes.Transactions)
                    {
                        //Add to DB
                        if (trans.AccountAmount != 0)
                        {
                            var dbTrans = new Transaction()
                            {
                                AccountNumber = statRes.Account,
                                Amount = Math.Abs(trans.AccountAmount),
                                Date = trans.TransactionDate,
                                Description = trans.OperationDescription,
                                Direction = trans.AccountAmount > 0 ? Transaction.TransactionDirections.Income : Transaction.TransactionDirections.Expence,
                                Merchant = trans.Place,
                                TransactionType = Transaction.TransactionTypes.Cash
                            };
                            TransactionsDataAdapter.InsertTransaction(dbTrans, CurrentUser.CurrentCompany, CurrentUser.LanguageId);
                        }
                    }
                    result = "ok";
                    Response.Redirect(Request.UrlReferrer.PathAndQuery);
                }
            }
            return result;
        }

    }
}
