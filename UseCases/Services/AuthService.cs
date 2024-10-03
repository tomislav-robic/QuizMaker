using Microsoft.IdentityModel.Tokens;
using QuizMaker.Core.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UseCases.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _adminUsername;
        private readonly string _adminPassword;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(string adminUsername, string adminPassword, string jwtSecret, string issuer, string audience)
        {
            _adminUsername = adminUsername;
            _adminPassword = adminPassword;
            _jwtSecret = jwtSecret;
            _issuer = issuer;
            _audience = audience;
        }

        public string Authenticate(string username, string password)
        {
            if (username == _adminUsername && password == _adminPassword)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            else
            {
                return null;
            }
        }
    }
}
