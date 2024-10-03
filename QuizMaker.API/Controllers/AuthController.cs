using QuizMaker.Core.DTOs;
using QuizMaker.Core.Interfaces;
using System.Web.Http;
using System.IdentityModel.Tokens.Jwt;

namespace QuizMaker.API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Korisničko ime i lozinka su obavezni.");
            }

            var token = _authService.Authenticate(loginDto.Username, loginDto.Password);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(new { Token = token });
        }
    }
}
