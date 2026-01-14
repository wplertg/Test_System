using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Test_System.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly AppDbContext _db;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ICurrentUser currentUser, AppDbContext db)
        {
            _logger = logger;
            _currentUser = currentUser;
            _db = db;
        }
    }
}
