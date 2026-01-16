namespace RetroGameExchangeApi
{
    public class Game
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Publisher { get; set; } = "";
        public int YearPublished { get; set; }
        public string System { get; set; } = "";
        public string Condition { get; set; } = "";
        public int? PreviousOwners { get; set; }

        public int OwnerId { get; set; }
        public User? Owner { get; set; }
    }
}
