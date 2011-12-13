using System;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Naviam.Data;
using Naviam.DAL;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Resources;
using log4net;
using Naviam.Domain;
using Naviam.NotificationCenter;

namespace Naviam.WebUI.Controllers
{
    public class SmsController : Controller
    {
        private readonly ModemsRepository _modemsRepository;
        private readonly TransactionsRepository _transRepository;
        private readonly AccountsRepository _accountsRepository;
        private readonly CurrenciesRepository _currenciesRepository;
        private readonly CategoriesRepository _categoriesRepository;
        private readonly MembershipRepository _membershipRepository;
        private readonly RulesRepository _rulesRepository;

        public SmsController()
            : this(null, null, null, null, null)
        {
        }

        public SmsController(ModemsRepository modemsRepository, TransactionsRepository transRepository, AccountsRepository accountsRepository, CurrenciesRepository currenciesRepository, CategoriesRepository categoriesRepository)
        {
            _modemsRepository = modemsRepository ?? new ModemsRepository();
            _transRepository = transRepository ?? new TransactionsRepository();
            _accountsRepository = accountsRepository ?? new AccountsRepository();
            _currenciesRepository = currenciesRepository ?? new CurrenciesRepository();
            _categoriesRepository = categoriesRepository ?? new CategoriesRepository();
            _membershipRepository = _membershipRepository ?? new MembershipRepository();
            _rulesRepository = _rulesRepository ?? new RulesRepository();
        }

        [HttpPost]
        public ActionResult RecieveMessage(string key, string gateway, string from, string to, string message)
        {
            //message = testMessage;
            //gateway = "GETWAY1";
            ILog log = LogManager.GetLogger("navSite");

            if (key != "givemeaccesstotoyou")
            {
                log.Error("session key does not match with the original!");
                return Json("error");
            }

            Modem modem = _modemsRepository.GetModemByGateway(gateway);

            log.Debug(String.Format("gateway:{0}, from:{1}, message:{2}", gateway, from, message));

            FinanceInstitution bank = FinanceInstitutionDataAdapter.GetByIdentifier(from);
            if (bank == null)
            {
                log.Warn(String.Format("can't find bank"));
                return Json("ok");
            }
            try
            {
                SmsBase sms;
                
                //try find bank
                switch (bank.Id)
                {
                    case 15: sms = new BelSwissSms(message);
                        break;
                    default:
                        sms = new BelSwissSms(message);
                        break;
                }
                
                //try find account
                var account = _accountsRepository.GetAccountBySms(sms.CardNumber, modem.Id, bank.Id);
                if (account == null)
                {
                    log.Warn(String.Format("can't find account"));
                    return Json("ok");
                }
                
                //if result is not Uspeshno send sms and not add transaction to db
                if (string.IsNullOrEmpty(sms.Result) || !sms.Result.Equals("Uspeshno", StringComparison.InvariantCultureIgnoreCase))
                {
                    log.Warn(String.Format("sms resault is {0}", sms.Result));
                    NotificationManager.Instance.SendSmsMail(account.SmsUser, message);
                    return Json("ok");
                }

                //get category id
                var categoryId = _categoriesRepository.FindCategoryMerchant(account.UserId, sms.Merchant.Trim());
                var tran =
                    new Transaction
                    {
                        Amount = sms.Amount,
                        CategoryId = categoryId,
                        //autosearch category by merchant, default 20 - Uncategorized
                        CurrencyId = _currenciesRepository.GetCurrencyByShortName(sms.ShortCurrency).Id,
                        Date = sms.Date,
                        //autosearch description by merchant, default description = mechant
                        Description = _rulesRepository.FindDescriptionMerchant(account.UserId, sms.Merchant.Trim()),
                        Direction = sms.Direction,
                        IncludeInTax = false,
                        Notes = "",
                        TransactionType = TransactionTypes.SMS,
                        Merchant = sms.Merchant.Trim(),
                        AccountId = account.Id
                    };
                _transRepository.Insert(tran, account.CompanyId);
                var val = tran.Amount.HasValue ? tran.Amount.Value : 0;
                _accountsRepository.ChangeBalance(account.Id, account.CompanyId, val * (tran.Direction == TransactionDirections.Expense ? -1 : 1));
                NotificationManager.Instance.SendSmsMail(account.SmsUser, sms.HtmlText);
            }
            catch (Exception ex)
            {
                log.Error(String.Format("sms error"), ex);
                Response.StatusCode = 500;
                return Json(new { Text = ex.Message, stackTrace = ex.StackTrace });
            }
            return Json("ok");
        }


        static string testMessage = @"
4..7983
Retail
Uspeshno
2011-10-27 17:25:44
Summa: 257900 BYR
Ostatok: 1264348 BYR
Na vremya: 20:26:12
BLR/MINSK REG./KRAVT SHOP (AKVABEL)
";
        static string other = @"
4..0692 
Service payment from card 
Uspeshno 
2011-09-01 00:00:00 
Summa: 4828 BYR 
Ostatok: 56552 BYR 
Na vremya: 11:11:46 
//RBS Balance loader

4..0692 
Service payment to card 
Uspeshno 
2011-09-01 00:00:00 
Summa: 999999 BYR 
Ostatok: 1056551 BYR 
Na vremya: 16:26:28 
//RBS Balance loader

4..7983
Cash
Uspeshno
2011-10-13 11:54:07
Summa: 400000 BYR
Ostatok: 1200962 BYR
Na vremya: 11:54:12
BLR/MINSK/EUROSET RKC 3

4..0692
Retail
Uspeshno
2011-08-22 09:32:37
Summa: 50000 BYR
Ostatok: 141380 BYR
Na vremya: 09:32:43
BLR/MINSK/BELCEL I-BANK

4..7983
Retail
Uspeshno
2011-10-27 17:25:44
Summa: 257900 BYR
Ostatok: 1264348 BYR
Na vremya: 20:26:12
BLR/MINSK REG./KRAVT SHOP (AKVABEL)
";

    }
}
