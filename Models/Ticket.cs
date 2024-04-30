namespace AeroFly.Models
{
    public class Ticket
    {
        public string ClientId { get; set; }
        public string Id { get; set; }
        public Races Races { get; set; }

        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string Plane { get; set; }
    }
}
