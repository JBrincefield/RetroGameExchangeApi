namespace RetroGameExchangeApi.Dtos
{
    public record GameCreateDto(
        string Name,
        string Publisher,
        int YearPublished,
        string System,
        string Condition,
        int? PreviousOwners,
        int OwnerId
    );
}