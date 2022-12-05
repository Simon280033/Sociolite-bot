using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Properties
{
    public partial class CustomPollProperty
    {
        public int Id { get; set; }

        public int TeamId { get; set; }

        public string CreatedBy { get; set; }

        public string? Question { get; set; }
        public string? PollOptions { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string>  getOptionsAsList()
        {
            List<string> options = new List<string>();

            if (PollOptions != null)
            {
                options = JsonConvert.DeserializeObject<List<string>>(PollOptions);
            }

            return options;
        }


    }

}


