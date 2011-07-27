using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Naviam.Data
{

    [Serializable]
    public class Transaction
    {
        public string Description { get; set; }
        public string Category { get; set; }
        public int Amount { get; set; }
    }
}
