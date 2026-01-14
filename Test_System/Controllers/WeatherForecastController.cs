using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Tools.Common;

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
        private readonly TcpClientService _tcp;
        private readonly SerialPortService _serial;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger, 
            ICurrentUser currentUser, 
            AppDbContext db,
            Func<string, TcpClientService> tcpFactory,
        Func<string, SerialPortService> serialFactory
            )
        {
            _logger = logger;
            _currentUser = currentUser;
            _db = db;
            _tcp = tcpFactory("CE1");
            _serial = serialFactory("COM1");
        }
    }
}
