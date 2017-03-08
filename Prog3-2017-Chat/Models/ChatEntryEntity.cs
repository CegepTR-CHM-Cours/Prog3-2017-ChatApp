using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class ChatEntryEntity : TableEntity
    {
        public string Username { get; set; }

        public string Message { get; set; }
    }
}
