using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace JackBotV2.Database
{
    public partial class JackBotQuotes
    {
        [Key] // set Key for the DB to use
        public string Quote { get; set; }
        public string User { get; set; }
    }
}
