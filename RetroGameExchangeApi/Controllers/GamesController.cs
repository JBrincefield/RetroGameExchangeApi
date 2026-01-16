using Microsoft.AspNetCore.Mvc;
using RetroGameExchangeApi.Dtos;

namespace RetroGameExchangeApi.Controllers
{
    [ApiController]
    [Route("games")]
    public class GamesController : ControllerBase
    {
        private readonly ExchangeDbContext _db;

        public GamesController(ExchangeDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult Create(GameCreateDto dto)
        {
            var owner = _db.Users.Find(dto.OwnerId);
            if (owner == null)
                return BadRequest(new { error = "Owner does not exist" });

            var game = new Game
            {
                Name = dto.Name,
                Publisher = dto.Publisher,
                YearPublished = dto.YearPublished,
                System = dto.System,
                Condition = dto.Condition,
                PreviousOwners = dto.PreviousOwners,
                OwnerId = dto.OwnerId
            };

            _db.Games.Add(game);
            _db.SaveChanges();

            return Created($"/games/{game.Id}", ToResponse(game));
        }

        [HttpGet("search")]
        public IActionResult Search(string name)
        {
            var games = _db.Games
                .Where(g => g.Name.Contains(name))
                .Select(ToResponse);

            return Ok(games);
        }

        private object ToResponse(Game game) => new
        {
            game.Id,
            game.Name,
            game.Publisher,
            game.YearPublished,
            game.System,
            game.Condition,
            game.PreviousOwners,
            links = new
            {
                self = $"/games/{game.Id}",
                owner = $"/users/{game.OwnerId}",
                update = $"/games/{game.Id}",
                delete = $"/games/{game.Id}"
            }
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            var games = _db.Games
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.Publisher,
                    g.YearPublished,
                    g.System,
                    g.Condition,
                    g.PreviousOwners,
                    links = new
                    {
                        self = $"/games/{g.Id}",
                        owner = $"/users/{g.OwnerId}"
                    }
                });

            return Ok(games);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, GameCreateDto dto)
        {
            var game = _db.Games.Find(id);
            if (game == null)
                return NotFound();

            game.Name = dto.Name;
            game.Publisher = dto.Publisher;
            game.YearPublished = dto.YearPublished;
            game.System = dto.System;
            game.Condition = dto.Condition;
            game.PreviousOwners = dto.PreviousOwners;

            _db.SaveChanges();

            return Ok(ToResponse(game));
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var game = _db.Games.Find(id);
            if (game == null)
                return NotFound();

            _db.Games.Remove(game);
            _db.SaveChanges();

            return NoContent();
        }


    }
}
