using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTO;
using Models.Result;

namespace Test_System.Controllers.UserControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public async Task<ApiResult<string>> Login(LoginDTO login)
        {
            
            return ApiResult<string>.Ok("成功");
        }
    }
}
