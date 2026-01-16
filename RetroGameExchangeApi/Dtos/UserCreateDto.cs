namespace RetroGameExchangeApi.Dtos
{
    public record UserCreateDto(
    string Name,
    string Email,
    string Password,
    string StreetAddress
);
}
