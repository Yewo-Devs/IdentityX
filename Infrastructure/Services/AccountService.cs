using IdentityX.Application.DTO.Users;
using IdentityX.Application.DTO.Validation;
using IdentityX.Application.Extensions;
using IdentityX.Application.Interfaces;
using IdentityX.Core.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IdentityX.Infrastructure.Services
{
	public class AccountService : IAccountService
	{
		private readonly string Application_Domain = Environment.GetEnvironmentVariable("Application_Domain");

		private readonly ITokenService _tokenService;
		private readonly IDataService _dataService;
		private readonly IEmailService _emailService;

		public AccountService(ITokenService tokenService, IDataService dataService,
			IEmailService emailService)
		{
			_tokenService = tokenService;
			_dataService = dataService;
			_emailService = emailService;
		}

		public async Task<ResultObjectDto<UserDto>> Login(LoginDto loginDto)
		{
			var user = await FindUserByEmailOrUsername(loginDto.Email, loginDto.Username);
			if (user == null)
				return new ResultObjectDto<UserDto> { Error = "Account doesn't exist" };

			if (!user.AccountEnabled)
				return new ResultObjectDto<UserDto> { Error = "Your account has been disabled" };

			if (!VerifyPassword(user, loginDto.Password))
				return new ResultObjectDto<UserDto> { Error = "Wrong password" };

			if (!user.AccountVerified)
			{
				await GenerateVerificationToken(user.Id);
				return new ResultObjectDto<UserDto> { Error = $"?accountId={user.Id}" };
			}

			return new ResultObjectDto<UserDto> { Result = GetUserDto(user, loginDto.KeepLoggedIn) };
		}

		public async Task<ResultObjectDto<UserDto>> RefreshAuth(RefreshAuthDto refreshAuthDto)
		{
			var user = await GetUserFromId(refreshAuthDto.AccountId);

			if (user == null)
				return new ResultObjectDto<UserDto> { Error = "Account doesn't exist" };

			if (!user.AccountEnabled)
				return new ResultObjectDto<UserDto> { Error = "Your account has been disabled" };

			if (!user.AccountVerified)
			{
				await GenerateVerificationToken(user.Id);
				return new ResultObjectDto<UserDto> { Error = $"?accountId={user.Id}" };
			}

			var tokens = _tokenService.RefreshSessionToken(refreshAuthDto.RefreshToken);

			var userDto = GetUserDto(user, refreshAuthDto.KeepLoggedIn);

			userDto.RefreshToken = tokens.RefreshToken;
			userDto.Token = tokens.AccessToken;

			return new ResultObjectDto<UserDto> { Result = userDto };
		}

		public async Task<ResultObjectDto<string>> InitiatePasswordReset(string email, string username)
		{
			var user = await FindUserByEmailOrUsername(email, username);
			if (user == null)
				return new ResultObjectDto<string> { Error = "Account doesn't exist" };

			var resetToken = GenerateToken();

			user.PasswordResetToken = resetToken;

			await _dataService.UpdateData("Account", $"{user.Id}", user);

			var urlEncodedToken = HttpUtility.UrlEncode(resetToken);
			var url = $"{Application_Domain}/reset-password?accountId={user.Id}&token={urlEncodedToken}";

			await _emailService.SendPasswordResetLink(user.Email, url);

			return new ResultObjectDto<string> { Result = "Password reset link sent" };
		}

		public async Task<ResultObjectDto<string>> ResetPassword(PasswordResetDto passwordResetDto)
		{
			string accountId = passwordResetDto.AccountId, 
				token = passwordResetDto.Token, 
				newPassword = passwordResetDto.Password;

			var user = await GetUserFromId(accountId);
			if (user == null)
				return new ResultObjectDto<string> { Error = "Account doesn't exist" };

			if (token != user.PasswordResetToken)
				return new ResultObjectDto<string> { Error = "Invalid token" };

			using var hmac = new HMACSHA512();
			user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(newPassword));
			user.PasswordSalt = hmac.Key;
			user.PasswordResetToken = string.Empty;
			await _dataService.UpdateData("Account", user.Id, user);

			return new ResultObjectDto<string> { Result = "Password reset successful" };
		}

		public async Task<ResultObjectDto<UserDto>> SocialLogin(SocialLoginDto socialLoginDto)
		{
			var user = await GetUserFromId(socialLoginDto.Id);
			if (user != null)
			{
				if (!user.AccountEnabled)
					return new ResultObjectDto<UserDto> { Error = "Your account has been disabled" };

				return new ResultObjectDto<UserDto> { Result = GetUserDto(user, socialLoginDto.KeepLoggedIn) };
			}

			var newUser = RegisterUser<SocialLoginDto>(socialLoginDto, true);
			await _dataService.StoreData("Account", newUser, newUser.Id);

			return new ResultObjectDto<UserDto> { Result = GetUserDto(newUser, socialLoginDto.KeepLoggedIn) };
		}

		public async Task<ResultObjectDto<UserDto>> Register(RegisterDto registerDto, bool requireEmailVerification = false)
		{
			var existingUser = await FindUserByEmail(registerDto.Email);
			if (existingUser != null)
				return new ResultObjectDto<UserDto> { Error = "Account already exists." };

			var newUser = RegisterUser<RegisterDto>(registerDto, !requireEmailVerification);
			await _dataService.StoreData("Account", newUser, newUser.Id);

			if (requireEmailVerification)
				await GenerateVerificationToken(newUser.Id);

			return new ResultObjectDto<UserDto> { Error = $"?accountId={newUser.Id}" };
		}

		public async Task<ResultObjectDto<UserDto>> EditUser(EditUserDto editUserDto)
		{
			var user = await GetUserFromId(editUserDto.Id);
			if (user == null)
				return new ResultObjectDto<UserDto> { Error = "User doesn't exist" };

			UpdateUserDetails(user, editUserDto);
			await _dataService.UpdateData("Account", user.Id, user);

			return new ResultObjectDto<UserDto> { Result = GetUserDto(user) };
		}

		public async Task GenerateVerificationToken(string accountId)
		{
			var user = await GetUserFromId(accountId);
			if (user == null)
				throw new Exception("Account Doesn't Exist");

			var token = GenerateToken();
			await _dataService.UpdateData("Account", $"{user.Id}/AccountVerificationToken", token);

			var urlEncodedToken = HttpUtility.UrlEncode(token);
			var url = $"{Application_Domain}/api/account/verify?accountId={user.Id}&token={urlEncodedToken}";

			await _emailService.SendAccountActivation(url, user);
		}

		public async Task<ResultObjectDto<string>> VerifyAccount(string accountId, string token)
		{
			var user = await GetUserFromId(accountId);
			if (user == null)
				return new ResultObjectDto<string> { Error = "Account doesn't exist", Result = $"{Application_Domain}/verification-result?result=fail" };

			if (token != user.AccountVerificationToken)
				return new ResultObjectDto<string> { Error = "Invalid token", Result = $"{Application_Domain}/verification-result?result=fail" };

			user.AccountVerificationToken = string.Empty;
			user.AccountVerified = true;

			await _dataService.UpdateData("Account", user.Id, user);

			return new ResultObjectDto<string> { Result = $"{Application_Domain}/verification-result?result=success" };
		}

		public async Task PurgeSpamAccounts()
		{
			var users = await GetAccounts();
			foreach (var user in users)
			{
				if (!user.AccountVerified && (DateTime.UtcNow - user.CreatedAt).TotalHours >= 96)
				{
					await _dataService.DeleteData("Account", user.Id);
				}
			}
		}

		private async Task<AppUser> FindUserByEmailOrUsername(string email, string username)
		{
			if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(username))
				return null;

			var users = await GetAccounts();
			return users.FirstOrDefault(user =>
				(!string.IsNullOrEmpty(user.Email) && user.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
				(!string.IsNullOrEmpty(user.Username) && user.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
		}

		private async Task<AppUser> FindUserByEmail(string email)
		{
			if (string.IsNullOrEmpty(email))
				return null;

			var users = await GetAccounts();
			return users.FirstOrDefault(user =>
				!string.IsNullOrEmpty(user.Email) && user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
		}

		private bool VerifyPassword(AppUser user, string password)
		{
			using var hmac = new HMACSHA512(user.PasswordSalt);
			var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
			return computedHash.SequenceEqual(user.PasswordHash);
		}

		private void UpdateUserDetails(AppUser user, EditUserDto editUserDto)
		{
			user.AccountEnabled = editUserDto.AccountEnabled;
			user.Email = editUserDto.Email;
			user.Role = editUserDto.Role;
			user.Permissions = editUserDto.Permissions;
		}

		private string GenerateToken()
		{
			const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
			var token = new string(Enumerable.Range(0, 256).Select(_ => characters[RandomNumberGenerator.GetInt32(characters.Length)]).ToArray());
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
		}

		private AppUser RegisterUser<T>(T userManagementDto, bool verified = false)
		{
			var user = userManagementDto.Map<AppUser>();

			using var hmac = new HMACSHA512();
			user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userManagementDto is RegisterDto registerDto ? registerDto.Password : DateTime.UtcNow.Ticks.ToString()));
			user.PasswordSalt = hmac.Key;
			user.CreatedAt = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;
			user.Id = userManagementDto is SocialLoginDto socialLoginDto ? socialLoginDto.Id : Guid.NewGuid().ToString();
			user.AccountVerified = verified;
			user.AccountEnabled = true;
			return user;
		}

		private UserDto GetUserDto(AppUser appUser, bool keepLoggedIn = false)
		{
			var userDto = appUser.Map<UserDto>();
			var tokens = _tokenService.GenerateSessionTokens(appUser, keepLoggedIn);
			userDto.Token = tokens.AccessToken;
			userDto.RefreshToken = tokens.RefreshToken;
			return userDto;
		}

		public async Task<IEnumerable<UserDto>> GetUsers()
		{
			var accounts = await _dataService.GetCollectionOfType<UserDto>("Account");

			return accounts;
		}

		private async Task<IEnumerable<AppUser>> GetAccounts()
		{
			return await _dataService.GetCollectionOfType<AppUser>("Account");
		}

		public async Task<AppUser> GetUserFromId(string accountId)
		{
			return await _dataService.GetInstanceOfType<AppUser>("Account", accountId);
		}
	}
}
