using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Data.SqlClient;

namespace Naviam.Data
{

    [Serializable]
    public class Transaction : DbEntity
    {
        public Transaction() { }
        public Transaction(SqlDataReader reader) 
        { 
         //TODO: read fields
        }

        public DateTime? Date { get; set; }
        public string FormattedDate {
            get
            {
                if (Date.HasValue)
                    return Date.Value.ToString(Thread.CurrentThread.CurrentUICulture);
                return null;
            }
        }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }
}
