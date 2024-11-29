using IdentityX.Application.DTO.Users;
using IdentityX.Application.Interfaces;

namespace IdentityX.Infrastructure.Services
{
    public class ProfileService: IProfileService
	{
		private readonly IDataService _dataService;

		public ProfileService(IDataService dataService)
		{
			_dataService = dataService;
		}

		public async Task CreateUserProfile(CreateUserProfileDto createUserProfileDto)
		{
			await _dataService
				.StoreData("Profile", createUserProfileDto, createUserProfileDto.UserId);
		}

		public async Task DeleteUserProfile(string userId)
		{
			await _dataService
				.DeleteData("Profile", userId);
		}

		public async Task<ProfileDto> GetUserProfile(string userId)
		{
			return await _dataService
				.GetInstanceOfType<ProfileDto>("Profile", userId);
		}

		public async Task<IEnumerable<ProfileDto>> GetUserProfiles()
		{
			var profiles = await _dataService.GetCollectionOfType<ProfileDto>("Profile");

			return profiles;
		}

		public async Task UpdateUserProfile(EditProfileDto editProfileDto)
		{
			await _dataService
				.UpdateData("Profile", editProfileDto.UserId, editProfileDto);
		}
	}
}
