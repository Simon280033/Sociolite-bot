using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Properties
{
    [Table("Custom_Discussion")]
    public partial class CustomDiscussionProperty
    {

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("SocioliteTeamProperty")]
        public int TeamId { get; set; }

        [ForeignKey("UserProperty")]
        public string CreatedBy { get; set; }


        [Required]
        public string? TopicText { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }

}


