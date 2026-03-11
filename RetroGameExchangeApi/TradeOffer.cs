public class TradeOffer
{
    public int Id { get; set; }
    public int OfferedByUserId { get; set; }
    public int OfferedGameId { get; set; }
    public int RequestedGameId { get; set; }
    public int RequestedFromUserId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}