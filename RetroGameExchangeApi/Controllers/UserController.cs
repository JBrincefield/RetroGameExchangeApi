using Microsoft.AspNetCore.Mvc;
using RetroGameExchangeApi.Dtos;

namespace RetroGameExchangeApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ExchangeDbContext _db;

        public UsersController(ExchangeDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public IActionResult Register(UserCreateDto dto)
        {
            if (_db.Users.Any(u => u.Email == dto.Email))
                return BadRequest(new { error = "Email already registered" });

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                StreetAddress = dto.StreetAddress
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Created($"/users/{user.Id}", ToResponse(user));
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UserUpdateDto dto)
        {
            var user = _db.Users.Find(id);
            if (user == null)
                return NotFound();

            user.Name = dto.Name;
            user.StreetAddress = dto.StreetAddress;

            _db.SaveChanges();
            return Ok(ToResponse(user));
        }

        private object ToResponse(User user) => new
        {
            user.Id,
            user.Name,
            user.Email,
            user.StreetAddress,
            links = new
            {
                self = $"/users/{user.Id}",
                games = $"/users/{user.Id}/games"
            }
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _db.Users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.StreetAddress,
                    links = new
                    {
                        self = $"/users/{u.Id}",
                        games = $"/users/{u.Id}/games"
                    }
                });

            return Ok(users);
        }

        [HttpGet("{userId}/games")]
        public IActionResult GetUserGames(int userId)
        {
            var games = _db.Games
                .Where(g => g.OwnerId == userId)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    links = new
                    {
                        self = $"/games/{g.Id}"
                    }
                });

            return Ok(games);
        }
    }

}
