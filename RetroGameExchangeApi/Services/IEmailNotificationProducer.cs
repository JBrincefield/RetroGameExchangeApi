using RetroGameExchangeApi.Messages;

namespace RetroGameExchangeApi.Services
{
    public interface IEmailNotificationProducer
    {
        Task SendNotificationAsync(EmailNotificationMessage message);
        Task SendPasswordChangedNotificationAsync(User user);
        Task SendTradeOfferCreatedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame);
        Task SendTradeOfferAcceptedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame);
        Task SendTradeOfferRejectedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame);
    }
}