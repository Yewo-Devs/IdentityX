using IdentityX.Application.DTO.Validation;

namespace IdentityX.Application.Interfaces
{
	public interface IMultiFactorAuthService
	{
		Task InitiateMfaChallenge(string email);
		Task<ResultObjectDto<bool>> AuthenticateMfaChallenge(string email, string token);
	}
}
