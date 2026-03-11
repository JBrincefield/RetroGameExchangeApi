using Microsoft.AspNetCore.Mvc;
using RetroGameExchangeApi;
using RetroGameExchangeApi.Services;

[ApiController]
[Route("tradeoffers")]
public class TradeOffersController : ControllerBase
{
    private readonly ExchangeDbContext _db;
    private readonly IEmailNotificationProducer _emailProducer;

    public TradeOffersController(ExchangeDbContext db, IEmailNotificationProducer emailProducer)
    {
        _db = db;
        _emailProducer = emailProducer;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOffer(TradeOffer offer)
    {
        // Validate the offering user exists
        var offeringUser = _db.Users.Find(offer.OfferedByUserId);
        if (offeringUser == null)
            return BadRequest(new { error = "Offering user does not exist" });

        // Validate the requested game exists
        var requestedGame = _db.Games.Find(offer.RequestedGameId);
        if (requestedGame == null)
            return BadRequest(new { error = "Requested game does not exist" });

        // Ensure user cannot request their own game
        if (requestedGame.OwnerId == offer.OfferedByUserId)
            return BadRequest(new { error = "Cannot create trade offer for your own game" });

        // Validate offered game if provided
        if (offer.OfferedGameId > 0)
        {
            var offeredGame = _db.Games.Find(offer.OfferedGameId);
            if (offeredGame == null)
                return BadRequest(new { error = "Offered game does not exist" });

            if (offeredGame.OwnerId != offer.OfferedByUserId)
                return BadRequest(new { error = "You can only offer games you own" });
        }

        offer.Status = "Pending";
        offer.RequestedFromUserId = requestedGame.OwnerId;

        _db.TradeOffers.Add(offer);
        _db.SaveChanges();

        // Send notification via Kafka
        var offeree = _db.Users.Find(requestedGame.OwnerId);
        if (offeree != null)
        {
            await _emailProducer.SendTradeOfferCreatedNotificationAsync(offer, offeringUser, offeree, requestedGame);
        }

        return CreatedAtAction(nameof(GetOffer), new { id = offer.Id }, offer);
    }

    [HttpGet("{id}")]
    public IActionResult GetOffer(int id)
    {
        var offer = _db.TradeOffers.Find(id);
        return offer == null ? NotFound() : Ok(offer);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_db.TradeOffers.ToList());
    }

    [HttpPut("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var offer = _db.TradeOffers.Find(id);
        if (offer == null) return NotFound();

        offer.Status = "Accepted";

        // Transfer game ownership
        var requestedGame = _db.Games.Find(offer.RequestedGameId);
        if (requestedGame == null) return NotFound();

        requestedGame.OwnerId = offer.OfferedByUserId;
        requestedGame.PreviousOwners = (requestedGame.PreviousOwners ?? 0) + 1;

        if (offer.OfferedGameId > 0)
        {
            var offeredGame = _db.Games.Find(offer.OfferedGameId);
            if (offeredGame != null)
            {
                offeredGame.OwnerId = offer.RequestedFromUserId;
                offeredGame.PreviousOwners = (offeredGame.PreviousOwners ?? 0) + 1;
            }
        }

        _db.SaveChanges();

        // Send notification via Kafka
        var offeror = _db.Users.Find(offer.OfferedByUserId);
        var offeree = _db.Users.Find(offer.RequestedFromUserId);

        if (offeror != null && offeree != null)
        {
            await _emailProducer.SendTradeOfferAcceptedNotificationAsync(offer, offeror, offeree, requestedGame);
        }

        return Ok(offer);
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var offer = _db.TradeOffers.Find(id);
        if (offer == null) return NotFound();

        offer.Status = "Rejected";
        _db.SaveChanges();

        // Send notification via Kafka
        var offeror = _db.Users.Find(offer.OfferedByUserId);
        var offeree = _db.Users.Find(offer.RequestedFromUserId);
        var requestedGame = _db.Games.Find(offer.RequestedGameId);

        if (offeror != null && offeree != null && requestedGame != null)
        {
            await _emailProducer.SendTradeOfferRejectedNotificationAsync(offer, offeror, offeree, requestedGame);
        }

        return Ok(offer);
    }
}