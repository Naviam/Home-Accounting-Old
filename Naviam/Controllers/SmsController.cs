using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Naviam.Domain.Concrete;
using Naviam.Data;
using Naviam.DAL;
using Naviam.WebUI.Helpers;
using Naviam.WebUI.Resources;
using log4net;
using Naviam.Domain;

namespace Naviam.WebUI.Controllers
{
    public class SmsController : Controller
    {

        private readonly ModemsRepository _modemsRepository;
        private readonly TransactionsRepository _transRepository;
        private readonly AccountsRepository _accountsRepository;
        private readonly CurrenciesRepository _currenciesRepository;
        private readonly CategoriesRepository _categoriesRepository;

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
        }

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
";
        [HttpPost]
        public ActionResult RecieveMessage(string key, string gateway, string from, string to, string message)
        {
            message = testMessage;
            //gateway = "GETWAY1";

            if (key != "givemeaccesstotoyou") return Json("error");

            Modem modem = _modemsRepository.GetModemByGateway(gateway);
            ILog log = LogManager.GetLogger("navSite");
            log.Debug(String.Format("gateway:{0}, from:{1}, message:{2}", gateway, from, message));
            
            //TODO: get bank_id by "from" param
            int id_bank = 15; //BelSwissBank

            try
            {
                SmsBase sms = new BelSwissSms(message);

                //TODO: check sms.Result????

                var account = _accountsRepository.GetAccountBySms(sms.CardNumber, modem.Id, id_bank);
                var tran = 
                    new Transaction
                    {
                        Amount = sms.Amount,
                        CategoryId = _categoriesRepository.FindCategoryForMerchant(account.Id, sms.Merchant.Trim()),
                        //autosearch category by merchant
                        // 20 - Uncategorized
                        CurrencyId = _currenciesRepository.GetCurrencyByShortName(sms.ShortCurrency).Id,
                        Date = DateTime.UtcNow,
                        Description = DisplayNames.SMSAlertServiceBank,
                        Direction = sms.Direction,
                        IncludeInTax = false,
                        Notes = "",
                        TransactionType = TransactionTypes.Cash,
                        Merchant = sms.Merchant,
                        AccountId = account.Id
                    };
                _transRepository.Insert(tran, account.CompanyId);
                var val = tran.Amount.HasValue ? tran.Amount.Value : 0;
                _accountsRepository.ChangeBalance(account.Id, account.CompanyId, val * (tran.Direction == TransactionDirections.Expense ? -1 : 1));
                //TODO: we don't have UserProfile here !!!!
                //EmailHelper.SendSmsAlert(from, SessionHelper.UserProfile.Name, message);
            }
            catch (Exception e)
            {
                throw e;
            }
            //throw new Exception("ddd");
            return Json("ok");
        }

    }
}
