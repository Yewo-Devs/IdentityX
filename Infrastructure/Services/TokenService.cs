using IdentityX.Application.Interfaces;
using IdentityX.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityX.Infrastructure.Services
{
	public class TokenService : ITokenService
	{
		private readonly SymmetricSecurityKey _key;
		private readonly string _issuer;
		private readonly string _audience;

		public TokenService(IConfiguration configuration)
		{
			_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token_Key"]));
			_issuer = configuration["Token_Issuer"];
			_audience = configuration["Token_Audience"];
		}

		private string GenerateToken(AppUser appUser, int expiryInMinutes)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, appUser.Id),
				new Claim(ClaimTypes.Role, appUser.Role),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _issuer,
				audience: _audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private string GenerateRefreshToken(AppUser appUser, int expiryInDays)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, appUser.Id),
				new Claim(ClaimTypes.Role, appUser.Role),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _issuer,
				audience: _audience,
				claims: claims,
				expires: DateTime.UtcNow.AddDays(expiryInDays),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public (string AccessToken, string RefreshToken) GenerateSessionTokens(AppUser appUser, bool keepLoggedIn = false)
		{
			int accessTokenExpiryInMinutes, refreshTokenExpiryInDays;

			if (keepLoggedIn)
			{
				accessTokenExpiryInMinutes = 60 * 24 * 7; // 7 days
				refreshTokenExpiryInDays = 30; // 30 days
			}
			else
			{

				if (appUser.Role == "Admin")
				{
					accessTokenExpiryInMinutes = 15;
					refreshTokenExpiryInDays = 3;
				}
				else
				{
					accessTokenExpiryInMinutes = 30;
					refreshTokenExpiryInDays = 7;
				}
			}

			var accessToken = GenerateToken(appUser, accessTokenExpiryInMinutes);
			var refreshToken = GenerateRefreshToken(appUser, refreshTokenExpiryInDays);

			return (accessToken, refreshToken);
		}

		public (string AccessToken, string RefreshToken) RefreshSessionToken(string refreshToken)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = false, // We are only validating the signature and claims here
				ValidateIssuerSigningKey = true,
				ValidIssuer = _issuer,
				ValidAudience = _audience,
				IssuerSigningKey = _key
			};

			try
			{
				var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken validatedToken);

				if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				{
					var appUser = new AppUser
					{
						Id = principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
						Role = principal.FindFirstValue(ClaimTypes.Role)
					};

					// Generate a new tokens with the same claims
					return GenerateSessionTokens(appUser); 
				}
				else
				{
					throw new SecurityTokenException("Invalid token");
				}
			}
			catch (Exception)
			{
				throw new SecurityTokenException("Invalid token");
			}
		}
	}
}
