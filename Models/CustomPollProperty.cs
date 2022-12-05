﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Properties
{
    [Table("Custom_Poll")]
    public partial class CustomPollProperty
    {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("SocioliteTeamProperty")]
        public int TeamId { get; set; }

        [ForeignKey("UserProperty")]
        public string CreatedBy { get; set; }


        [Required]
        public string? Question { get; set; }

        [Required]
        public string? PollOptions { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }




    }

}


