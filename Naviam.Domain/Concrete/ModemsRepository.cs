using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naviam.Data;
using Naviam.DAL;

namespace Naviam.Domain.Concrete
{
    public class ModemsRepository
    {
        public virtual Modem GetModemByGateway(string gateway)
        {
            return ModemsDataAdapter.GetModemByGateway(gateway);
        }

    }
}
