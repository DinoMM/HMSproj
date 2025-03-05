using DBLayer;
using HMSModels.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniComponents;

namespace HMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        // Allows anonymous access so non-logged-in users can call it for login.
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] HMSModels.LoginRequest request)
        {
            // Use your custom UserService logic to authenticate
            bool success = await _userService.LogInUserAsync(request.Name, request.Password);
            if (!success)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            // Upon a successful login, generate a JWT token.
            var user = _userService.LoggedUser;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user?.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user?.Id ?? "")
                // Here, add additional claims as needed (e.g., roles, etc.)
                //TODO
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4), // or whatever your token lifespan should be
                Issuer = "YourIssuer",
                Audience = "YourAudience",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_BEARER_SECRET"))),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            //ziskanie tokenu
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            //prevod na usera
            HMSModels.IdentityUserOwn userhms = new()
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
            };

            //prevod na role
            List<HMSModels.Services.RolesOwn> list = new();
            var ress = await _userService.GetRolesFromUser(user);
            foreach (var item in ress)
            {
                list.Add(UniComponents.EnumExtensions.GetEnumValue<HMSModels.Services.RolesOwn>(item));
            }

            return Ok(new HMSModels.LoginResponse { User = userhms, Roles = list, Token = tokenString });
        }

        // Optional: A logout endpoint can clear any server-side session or cached user state.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _userService.LogOutUser();
            return Ok(new { Message = "Logged out successfully" });
        }
    }

}
