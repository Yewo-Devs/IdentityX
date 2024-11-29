using IdentityX.Application.DTO.Users;
using IdentityX.Application.DTO.Validation;
using IdentityX.Core.Entities;

namespace IdentityX.Application.Interfaces
{
	public interface IAccountService
	{
		Task<ResultObjectDto<UserDto>> Login(LoginDto loginDto);
		Task<ResultObjectDto<UserDto>> SocialLogin(SocialLoginDto socialLoginDto);
		Task<ResultObjectDto<UserDto>> Register(RegisterDto registerDto, bool requireEmailVerification = false);
		Task<ResultObjectDto<UserDto>> EditUser(EditUserDto editUserDto);
		Task GenerateVerificationToken(string accountId);
		Task<ResultObjectDto<string>> VerifyAccount(string accountId, string token);
		Task PurgeSpamAccounts();
		Task<IEnumerable<AppUser>> GetAccounts();
		Task<AppUser> GetUserFromId(string accountId);
	}
}
