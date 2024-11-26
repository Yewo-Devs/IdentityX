namespace IdentityX.Infrastructure.Services
{
    public class AccountService: IAccountService
	{
		private readonly ITokenService _tokenService;

		public AccountService(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		public async Task<ResultObjectDto<UserDto>> Login(LoginDto loginDto)
		{
			IEnumerable<AppUser> appUsers = await GetAppUsers();

			//Email Lookup
			AppUser user = appUsers
				.FirstOrDefault(user => user.Email.ToLower() == loginDto.Email.ToLower());

			//Username Lookup
			if (user == null)
				user = appUsers
					.FirstOrDefault(user => user.Username.ToLower() == loginDto.Username.ToLower());

			if (user == null)
				return new ResultObjectDto<UserDto>() { Error = "Account doesn't exist" };

			if (!user.AccountEnabled)
				return new ResultObjectDto<UserDto>() { Error = "Your account has been disabled" };

			using var hmac = new HMACSHA512(user.PasswordSalt);
			Byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

			for (int i = 0; i < computedHash.Length; i++)
			{
				if (computedHash[i] != user.PasswordHash[i])
					return new ResultObjectDto<UserDto>() { Error = "Wrong password" };
			}

			if (!user.AccountVerified)
			{
				//Generate verification Token
				await GenerateVerificationToken(user.ID);

				return new ResultObjectDto<UserDto>() { Error = $"?accountId={user.ID}" };
			}

			return new ResultObjectDto<UserDto>() { Result = await GetUserDto(user) };
		}

		public async Task<ResultObjectDto<UserDto>> SocialLogin(SocialLoginDto socialLoginDto)
		{
			//if account exists login
			IEnumerable<AppUser> appUsers = await GetAppUsers();

			AppUser account = appUsers.FirstOrDefault(user => user.ID.ToLower() == socialLoginDto.Id);

			if (account != null)
			{
				AppUser user = account;

				if (!user.AccountEnabled)
					return new ResultObjectDto<UserDto>() { Error = "Your account has been disabled" };

				return new ResultObjectDto<UserDto>() { Result = await GetUserDto(user) };
			}

			//if account doesn't exist create an account
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

			AppUser newUser = socialLoginDto.Map<AppUser, SocialLoginDto>();

			newUser.Permissions = new List<string>() { "External" };

			using (var hmac = new HMACSHA512())
			{
				newUser.PasswordHash = hmac
					.ComputeHash(Encoding.UTF8.GetBytes(DateTime.UtcNow.Ticks.ToString()));
				newUser.PasswordSalt = hmac.Key;
			}

			newUser.FirstName = textInfo.ToTitleCase(socialLoginDto.FirstName.ToLower());
			newUser.LastName = textInfo.ToTitleCase(socialLoginDto.LastName.ToLower());
			newUser.DateTimeCreated = DateTime.UtcNow;
			newUser.ID = socialLoginDto.Id;
			newUser.AccountVerified = true;

			await _firebaseService.StoreData(FirebaseDataNodes.Account, newUser, newUser.ID);

			return new ResultObjectDto<UserDto>() { Result = await GetUserDto(newUser) };
		}

		public async Task<ResultObject<UserDto>> Register(RegisterDto registerDto)
		{
			if (await UserExists(registerDto.Email))
				return new ResultObject<UserDto>() { Error = "Account already exists." };

			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

			AppUser user = registerDto.Map<AppUser, RegisterDto>();

			using (var hmac = new HMACSHA512())
			{
				user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
				user.PasswordSalt = hmac.Key;
			}

			user.FirstName = textInfo.ToTitleCase(registerDto.Firstname.ToLower());
			user.LastName = textInfo.ToTitleCase(registerDto.Lastname.ToLower());
			user.ID = DateTime.UtcNow.Ticks.ToString();
			user.DateTimeCreated = DateTime.UtcNow;

			await _firebaseService.StoreData(FirebaseDataNodes.Account, user, user.ID);

			//Generate verification Token
			await GenerateVerificationToken(user.ID);

			return new ResultObject<UserDto>() { Error = $"?accountId={user.ID}" };
		}

		public async Task GenerateVerificationToken(string accountId)
		{
			AppUser user = await GetUserFromId(accountId);

			if (user == null)
				throw new Exception("Account Doesn't Exist");

			string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			foreach (var item in characters)
			{
				characters += item.ToString().ToLower();
			}

			characters += "1234567890";

			string token = new string(Enumerable
						.Range(0, 256)
						.Select(num => characters[new Random().Next() % characters.Length])
						.ToArray());

			token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

			await _firebaseService
				.UpdateData(FirebaseDataNodes.Account,
				$"{user.ID}/AccountVerificationToken", token);

			//Send Email            

			string UrlEncodedToken = HttpUtility.UrlEncode(token);

			string url = $"{Application_Domain}/api/account/verify?accountId={user.ID}&token={UrlEncodedToken}";

			await _emailService.SendAccountActivation(url, user);
		}

		public async Task<IEnumerable<AppUser>> GetAccounts()
		{
			return await _firebaseService.GetCollectionOfType<AppUser>(FirebaseDataNodes.Account);
		}

		public async Task<ResultObject<UserDto>> EditUser(EditUserDto editUserDto)
		{
			AppUser user = (await GetAppUsers())
				.FirstOrDefault(user => user.ID == editUserDto.ID);

			if (user == null)
				return new ResultObject<UserDto>() { Error = "User doesn't exist" };

			//Edit user
			user.AccountEnabled = editUserDto.AccountEnabled;
			user.Email = editUserDto.Email;
			user.FirstName = editUserDto.FirstName;
			user.LastName = editUserDto.LastName;

			if (!string.IsNullOrEmpty(editUserDto.PhotoUrl))
				user.PhotoUrl = await _firebaseStorageService
					.StoreProfilePhoto(editUserDto.PhotoUrl, user.ID, "Profile Photo");

			user.Permissions = editUserDto.Permissions;

			await _firebaseService.UpdateData(FirebaseDataNodes.Account, editUserDto.ID, user);

			return new ResultObject<UserDto>() { Result = await GetUserDto(user) };
		}

		public async Task<AppUser> GetUserFromId(string accountId)
		{
			return await _firebaseService.GetInstanceOfType<AppUser>(FirebaseDataNodes.Account, accountId);
		}

		public async Task<ResultObject<string>> VerifyAccount(string accountId, string token)
		{
			AppUser user = await GetUserFromId(accountId);

			if (user == null)
				return new ResultObject<string>()
				{
					Error = "Account doesn't exist",
					Result = $"{Application_Domain}/verification-result?result=fail"
				};

			if (token != user.AccountVerificationToken)
				return new ResultObject<string>()
				{
					Error = "Invalid token",
					Result = $"{Application_Domain}/verification-result?result=fail"
				};

			user.AccountVerificationToken = "";
			user.AccountVerified = true;

			await _firebaseService.UpdateData(FirebaseDataNodes.Account, user.ID, user);

			return new ResultObject<string>()
			{
				Result = $"{Application_Domain}/verification-result?result=success"
			};
		}

		public async Task PurgeSpamAccounts()
		{
			IEnumerable<AppUser> users = await GetAccounts();

			foreach (AppUser user in users)
			{
				if (!user.AccountVerified)
				{
					TimeSpan timeSpan = DateTime.UtcNow - user.DateTimeCreated;

					if (timeSpan.TotalHours >= 96)
					{
						await _firebaseService.DeleteData(FirebaseDataNodes.Account, user.ID);
					}
				}
			}
		}

		private async Task<UserDto> GetUserDto(AppUser appUser)
		{
			UserDto userDto = appUser.Map<UserDto, AppUser>();

			userDto.Token = _tokenService.CreateToken(appUser);

			var package = await _packageService.GetPackage(appUser.ID);

			if (package == null)
				return userDto;

			userDto.Package = package.Name;

			return userDto;
		}
	}
}
