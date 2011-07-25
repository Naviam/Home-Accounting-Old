using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Naviam.Data
{

    /// <summary>
    /// User Profile object
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public UserProfile(IDataRecord record)
        {
            Id = record["userId"] as int?;
            Name = record["userName"] as string;
            Password = record["userPassword"] as string;
        }
    }
}
