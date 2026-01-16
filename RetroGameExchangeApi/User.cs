namespace RetroGameExchangeApi
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string StreetAddress { get; set; } = "";

        public List<Game> Games { get; set; } = new();
    }
}
