using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebSignalRServer.Core;
using WebSignalRServer.Models;

namespace WebSignalRServer.Controllers
{
	public class JWTTokenController : Controller
	{
		[HttpPost("api/token")]
		public async Task<IActionResult> GetTokenForCredentialsAsync([FromBody] LoginRequest login)
		{
			var result = false;
			if (login != null && login.Password == "Test" && login.Username == "Test")
			{
				result = true;
			}

			if (result)
			{
				var token = await GenerateToken(login.Username);

				return Ok(token);
			}

			return Unauthorized();
		}

		public async Task<string> GenerateToken(string Username)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			string tokenString = string.Empty;

			try
			{
				await Task.Run(() =>
				{
					var key = Encoding.ASCII.GetBytes(GlobalContext.SystemConfig.JwtSecretKey);
					var tokenDescriptor = new SecurityTokenDescriptor
					{
						Subject = new ClaimsIdentity(new[] {
					new Claim(ClaimTypes.NameIdentifier, Username)
					}),
						Expires = DateTime.UtcNow.AddMinutes(30),
						SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
					};
					var token = tokenHandler.CreateToken(tokenDescriptor);
					tokenString = tokenHandler.WriteToken(token);
				});
			}
			catch (Exception ex)
			{
				//  ex.WriteLog();
			}

			return tokenString;
		}

		//private string GenerateToken(string userId)
		//{
		//    var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(GlobalContext.SystemConfig.JwtSecretKey));

		//    var claims = new[]
		//    {
		//        new Claim(ClaimTypes.NameIdentifier, userId)
		//    };

		//    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
		//    var token = new JwtSecurityToken("signalrdemo", "signalrdemo", claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: credentials);

		//    return new JwtSecurityTokenHandler().WriteToken(token);
		//}
	}
}