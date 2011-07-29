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
    public class UserProfile : DbEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
        
        public UserProfile()  {}

        public UserProfile(IDataRecord record)
        {
            Id = record["id"] as int?;
            Name = record["email"] as string;
            Password = record["password"] as string;
        }
    }
}
