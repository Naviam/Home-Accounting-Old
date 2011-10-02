using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Naviam.Data
{
    public abstract class SmsBase
    {
        protected string _sms;

        public SmsBase(string sms)
        {
            _sms = sms;
            CardNumber = GetCardNumber();
            Operation = GetOperation();
            Result = GetResult();
            Date = GetDate();
            Amount = GetAmount();
            ShortCurrency = GetShortCurrency();
            Merchant = GetMerchant();
            Direction = GetDirection();
        }

        public string CardNumber { get; set; }
        public string Operation { get; set; }
        public string Result { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string ShortCurrency { get; set; }
        public string Merchant { get; set; }
        public TransactionDirections Direction { get; set; }

        public virtual string GetCardNumber()
        {
            return string.Empty;
        }

        public virtual string GetOperation()
        {
            return string.Empty;
        }

        public virtual string GetResult()
        {
            return string.Empty;
        }

        public virtual DateTime GetDate()
        {
            return DateTime.MinValue;
        }

        public virtual decimal GetAmount()
        {
            return 0;
        }

        public virtual string GetShortCurrency()
        {
            return string.Empty;
        }

        public virtual string GetMerchant()
        {
            return string.Empty;
        }

        public virtual TransactionDirections GetDirection()
        {
            return TransactionDirections.Expense;
        }
    }
}