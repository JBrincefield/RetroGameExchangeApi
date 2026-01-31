using Microsoft.AspNetCore.Mvc;
using RetroGameExchangeApi;
using System;

[ApiController]
[Route("tradeoffers")]
public class TradeOffersController : ControllerBase
{
    private readonly ExchangeDbContext _db;

    public TradeOffersController(ExchangeDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public IActionResult CreateOffer(TradeOffer offer)
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
    public IActionResult Accept(int id)
    {
        var offer = _db.TradeOffers.Find(id);
        if (offer == null) return NotFound();

        offer.Status = "Accepted";
        _db.SaveChanges();

        return Ok(offer);
    }

    [HttpPut("{id}/reject")]
    public IActionResult Reject(int id)
    {
        var offer = _db.TradeOffers.Find(id);
        if (offer == null) return NotFound();

        offer.Status = "Rejected";
        _db.SaveChanges();

        return Ok(offer);
    }
}