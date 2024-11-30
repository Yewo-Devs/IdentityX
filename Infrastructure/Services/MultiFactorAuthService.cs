using IdentityX.Application.DTO.Validation;
using IdentityX.Application.Interfaces;
using IdentityX.Core.Entities;
using IdentityX.Core.Models;

namespace IdentityX.Infrastructure.Services
{
	public class MultiFactorAuthService : IMultiFactorAuthService
	{
		private readonly IDataService _dataService;
		private readonly IEmailService _emailService;

		public MultiFactorAuthService(IDataService dataService, IEmailService emailService)
		{
			_dataService = dataService;
			_emailService = emailService;
		}

		public async Task<ResultObjectDto<bool>> AuthenticateMfaChallenge(string email, string token)
		{
			IEnumerable<MfaChallenge> challenges = await _dataService.GetCollectionOfType<MfaChallenge>("Mfa");

			MfaChallenge challenge = challenges.FirstOrDefault(chall => chall.Email.ToLower() == email.ToLower());

			if (challenge == null)
				return new ResultObjectDto<bool>()
				{
					Error = "There are no multi factor authentication codes available at this time"
				};

			if (challenge.Token == token && challenge.ExpiryTime > DateTime.UtcNow)
			{
				await _dataService.DeleteData("Mfa", challenge.Id);

				return new ResultObjectDto<bool>() 
				{
					Result = true				
				};
			}

			if (challenge.Token == token)
				return new ResultObjectDto<bool>()
				{
					Error = "The token you entered is incorrect"
				};

			if (challenge.ExpiryTime > DateTime.UtcNow)
				return new ResultObjectDto<bool>()
				{
					Error = "The token you entered has expired"
				};

			return new ResultObjectDto<bool>()
			{
				Result = false
			};
		}

		public async Task InitiateMfaChallenge(string email)
		{
			var token = GenerateToken();

			var challenge = new MfaChallenge
			{
				Id = Guid.NewGuid().ToString(),
				Email = email.ToLower(),
				Token = token,
				CreationTime = DateTime.UtcNow,
				ExpiryTime = DateTime.UtcNow.AddMinutes(15)
			};

			await _dataService.StoreData("Mfa", challenge, challenge.Id);

			await _emailService.SendMfaToken(token, email);
		}

		private string GenerateToken()
		{
			var random = new Random();
			return random.Next(100000, 999999).ToString();
		}
	}
}
