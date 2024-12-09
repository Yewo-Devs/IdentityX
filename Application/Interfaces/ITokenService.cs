using IdentityX.Core.Entities;

namespace IdentityX.Application.Interfaces
{
	public interface ITokenService
	{
		(string AccessToken, string RefreshToken) RefreshSessionToken(string refreshToken);

		(string AccessToken, string RefreshToken) GenerateSessionTokens(AppUser appUser, bool keepLoggedIn = false);
	}
}
