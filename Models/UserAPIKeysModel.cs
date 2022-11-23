using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MechaChat.API.Models
{
    public class UserAPIKeysModel
    {
        [Description("id")]
        public int Id { get; set; }

        [Description("user_id")]
        public int UserId { get; set; }

        [Description("api_type")]
        public string APIType { get; set; }

        [Description("api_key")]
        public string APIKey { get; set; }
    }
}
