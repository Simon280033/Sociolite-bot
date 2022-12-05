namespace REST.Model.ExchangeClasses
{
    public class ActivityRequestObject
    {
        public bool IsActive { get; set; }
        public string RecurranceString { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
