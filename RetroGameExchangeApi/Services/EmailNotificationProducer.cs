using RetroGameExchangeApi.Messages;
using System.Text.Json;

namespace RetroGameExchangeApi.Services
{
    public class EmailNotificationProducer : IEmailNotificationProducer
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<EmailNotificationProducer> _logger;
        private readonly string _topicName;

        public EmailNotificationProducer(IKafkaProducer kafkaProducer, IConfiguration configuration, ILogger<EmailNotificationProducer> logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
            _topicName = configuration["Kafka:Topics:EmailNotifications"] ?? "email-notifications";
        }

        public async Task SendNotificationAsync(EmailNotificationMessage message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                await _kafkaProducer.ProduceAsync(_topicName, json);
                _logger.LogInformation("Produced email notification for {Email}, Event: {EventType}",
                    message.ToEmail, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to produce email notification");
            }
        }

        public async Task SendPasswordChangedNotificationAsync(User user)
        {
            var message = new EmailNotificationMessage
            {
                ToEmail = user.Email,
                ToName = user.Name,
                Subject = "Your Password Has Been Changed",
                Body = $"Hello {user.Name},\n\nYour password for Retro Game Exchange has been changed.\n\nIf you did not make this change, please contact support immediately.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "PasswordChanged"
            };
            await SendNotificationAsync(message);
        }

        public async Task SendTradeOfferCreatedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame)
        {
            var offerorMessage = new EmailNotificationMessage
            {
                ToEmail = offeror.Email,
                ToName = offeror.Name,
                Subject = "Trade Offer Created",
                Body = $"Hello {offeror.Name},\n\nYou have successfully created a trade offer for \"{requestedGame.Name}\".\n\nWe'll notify you when {offeree.Name} responds.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferCreated"
            };
            await SendNotificationAsync(offerorMessage);

            var offereeMessage = new EmailNotificationMessage
            {
                ToEmail = offeree.Email,
                ToName = offeree.Name,
                Subject = "New Trade Offer Received",
                Body = $"Hello {offeree.Name},\n\n{offeror.Name} has made a trade offer for your game \"{requestedGame.Name}\".\n\nLog in to view and respond to this offer.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferCreated"
            };
            await SendNotificationAsync(offereeMessage);
        }

        public async Task SendTradeOfferAcceptedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame)
        {
            var offerorMessage = new EmailNotificationMessage
            {
                ToEmail = offeror.Email,
                ToName = offeror.Name,
                Subject = "Trade Offer Accepted!",
                Body = $"Hello {offeror.Name},\n\nGreat news! {offeree.Name} has accepted your trade offer for \"{requestedGame.Name}\".\n\nPlease coordinate with {offeree.Name} to complete the exchange.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferAccepted"
            };
            await SendNotificationAsync(offerorMessage);

            var offereeMessage = new EmailNotificationMessage
            {
                ToEmail = offeree.Email,
                ToName = offeree.Name,
                Subject = "Trade Offer Accepted",
                Body = $"Hello {offeree.Name},\n\nYou have accepted the trade offer from {offeror.Name} for your game \"{requestedGame.Name}\".\n\nPlease coordinate with {offeror.Name} to complete the exchange.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferAccepted"
            };
            await SendNotificationAsync(offereeMessage);
        }

        public async Task SendTradeOfferRejectedNotificationAsync(TradeOffer offer, User offeror, User offeree, Game requestedGame)
        {
            var offerorMessage = new EmailNotificationMessage
            {
                ToEmail = offeror.Email,
                ToName = offeror.Name,
                Subject = "Trade Offer Rejected",
                Body = $"Hello {offeror.Name},\n\nUnfortunately, {offeree.Name} has rejected your trade offer for \"{requestedGame.Name}\".\n\nDon't give up! Browse other games available for trade.\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferRejected"
            };
            await SendNotificationAsync(offerorMessage);

            var offereeMessage = new EmailNotificationMessage
            {
                ToEmail = offeree.Email,
                ToName = offeree.Name,
                Subject = "Trade Offer Rejected",
                Body = $"Hello {offeree.Name},\n\nYou have rejected the trade offer from {offeror.Name} for your game \"{requestedGame.Name}\".\n\nBest regards,\nRetro Game Exchange Team",
                EventType = "TradeOfferRejected"
            };
            await SendNotificationAsync(offereeMessage);
        }
    }
}