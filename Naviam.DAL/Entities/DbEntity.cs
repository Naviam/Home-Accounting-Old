using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naviam.Data
{
    public enum DbActionType { Select, Insert, Update, Delete }

    [Serializable]
    public abstract class DbEntity
    {
        public int? Id { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return ((DbEntity)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            //return Id != null ? Id.Value : base.GetHashCode();
            return base.GetHashCode();
        }
    }
}
