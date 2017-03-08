using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Models
{
    public class CreateChatEntryDto
    {
        [Required]
        [MaxLength(25)]
        public string Username { get; set; }

        [Required]
        [MaxLength(150)]
        public string Message { get; set; }
    }
}
