using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class ChatEntryDto
    {
        public string Username { get; set; }

        public string Message { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
