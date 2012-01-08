using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Naviam.InternetBank.Entities;

namespace Naviam.InternetBank
{
    public class BankClient : IDisposable
    {
        private readonly SbsiBankRequests _bankRequests;
        private const string BanksXmlFileName = "InternetBanks.xml";

        /// <summary>
        /// Current Internet bank 
        /// </summary>
        public InetBank InetBank { get; private set; }
        
        /// <summary>
        /// Internet Bank settings (rules, custom text) that helps to export and parse user's transactions
        /// </summary>
        public InetBankSettings Settings { get; private set; }

        protected static InetBankSettings LoadSettings(string sbsibanksettingsXml)
        {
            var serializer = new XmlSerializer(typeof(InetBankSettings));
            if (File.Exists(sbsibanksettingsXml))
            {
                using (var streamReader = File.OpenText(sbsibanksettingsXml))
                {
                    return serializer.Deserialize(streamReader) as InetBankSettings;
                }
            }
            return null;
        }

        protected static IEnumerable<InetBank> LoadBanks(string fileName)
        {
            var serializer = new XmlSerializer(typeof(InternetBanks));
            if (File.Exists(fileName))
            {
                using (var streamReader = File.OpenText(fileName))
                {
                    var internetBanks = serializer.Deserialize(streamReader) as InternetBanks;
                    if (internetBanks != null)
                        return internetBanks.Banks;
                }
            }
            return null;
        }

        /// <summary>
        /// Initialize the internet bank client with specific settings of a given bank.
        /// </summary>
        /// <param name="bankId">Internal Bank Id that is used to get internet bank settings</param>
        public BankClient(int bankId)
        {
            // deserialize xml bank settings to object 
            // and read internet bank settings for specified bank id 
            var banks = LoadBanks(BanksXmlFileName);
            InetBank = (from b in banks
                       where b.NaviamId == bankId.ToString(CultureInfo.InvariantCulture)
                       select b).FirstOrDefault();

            if (InetBank != null) Settings = LoadSettings(InetBank.BankSettings);

            // init helper class to make requests
            _bankRequests = new SbsiBankRequests(Settings);
        }

        /// <summary>
        /// Login to the internet bank web site with provided inet bank credentials
        /// </summary>
        /// <param name="userName">IBank Username</param>
        /// <param name="password">IBank Password</param>
        /// <returns>Login response</returns>
        public LoginResponse Login(string userName, string password)
        {
            var responseCode = _bankRequests.GetLoginPage();
            if (responseCode == 0)
            {
                responseCode = _bankRequests.Authenticate(userName, password, InetBank.BankId);
            }
            return new LoginResponse
                       {
                           ErrorCode = responseCode,
                           IsAuthenticated = responseCode == 0
                       };
        }

        /// <summary>
        /// Obtain the list of user's payment cards from internet bank web site.
        /// </summary>
        public IEnumerable<PaymentCard> GetPaymentCards()
        {
            var cardList = _bankRequests.GetCardList();
            var resultCardList = new List<PaymentCard>();

            foreach (var paymentCard in cardList)
            {
                var card = paymentCard;
                if (paymentCard == null) continue;
                _bankRequests.UpdateCardInfo(ref card);
                resultCardList.Add(card);
            }
            return resultCardList;
        }

        /// <summary>
        /// Obtain the list of transactions for payment card starting from specified date.
        /// </summary>
        /// <param name="card">Payment Card to get transactions for</param>
        /// <param name="startDate">Start date to obtain transactions from</param>
        public IEnumerable<AccountTransaction> GetTransactions(PaymentCard card, DateTime startDate)
        {
            if (card == null)
                throw new ArgumentNullException("card");
            // verify start data is in the past and not a today date
            if (startDate >= DateTime.UtcNow)
                throw new ArgumentOutOfRangeException("startDate", startDate, "Start Date must be before today date.");
            
            // set card with parameter id active
            var cardChanged = _bankRequests.ChangeCurrentCard(card.Id);
            // get 20 latest transactions
            if (cardChanged)
            {
                var latestTransactions = _bankRequests.GetLatestCardTransactions().ToList();
                
                // check if there is a date in these transactions older than start date
                if (latestTransactions.Any(t => t.OperationDate.Date < startDate))
                {
                    // if  true return the list of transactions
                    return latestTransactions.Where(t => t.OperationDate.Date >= startDate);
                }
                // if false get report periods
                var reports = _bankRequests.GetListOfUsedStatements(startDate, card.RegisterDate);
                
                // run reports and create them when necessary
                foreach (var reportRow in reports.Where(reportRow => !reportRow.IsCreated))
                {
                    _bankRequests.CreateReport(reportRow.StartDate, reportRow.EndDate);
                }
            }
            
            return new List<AccountTransaction>();
        }

        /// <summary>
        /// Logout from internet bank
        /// </summary>
        /// <param name="cleanAuthCookies">Indicates whether it is required to clean internet bank cookies on logout</param>
        public void Logout(bool cleanAuthCookies)
        {
            
        }

        public void Dispose()
        {
            Logout(true);
        }
    }
}