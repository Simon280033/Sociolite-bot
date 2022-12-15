namespace MyTeamsApp2.Models
{
    public partial class RESTSociolitePoll
    {
        public int Id { get; set; }

        public int TeamId { get; set; }

        public string CreatedBy { get; set; }

        public string? Question { get; set; }

        public string? PollOptions { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}


