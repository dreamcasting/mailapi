using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MailAPI.Models
{
    public class ApiKey
    {
        [Key]
        public int ApiKeyId { get; set; }
        public string Key { get; set; }
        public string Salt { get; set; }
        [Display(Name ="Application name")]
        public string AssociatedApplication { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool ActiveStatus { get; set; }
        public Int64 Uses { get; set; }

        public ApiKey()
        {
            //Use pre-existing value or get a new guid (raw key)
            Key = Key ?? _Key();

            //Use pre-existing value or get a new salt
            Salt = Salt ?? _Salt();
        }

        private string _Key()
        {
            return Guid.NewGuid().ToString();
        }

        private string _Salt()
        {
            byte[] saltBytes = new byte[128/8];
            RandomNumberGenerator CryptoProvider = RandomNumberGenerator.Create();
            CryptoProvider.GetBytes(saltBytes);
            string saltString = BitConverter.ToString(saltBytes).Replace("-","").ToLower();
            return saltString;
        }

    }
}
