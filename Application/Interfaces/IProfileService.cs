using IdentityX.Application.DTO.Users;

namespace IdentityX.Application.Interfaces
{
	public interface IProfileService
	{
		Task<IEnumerable<ProfileDto>> GetUserProfiles();
		Task<ProfileDto> GetUserProfile(string userId);
		Task UpdateUserProfile(EditProfileDto editProfileDto);
		Task CreateUserProfile(CreateUserProfileDto createUserProfileDto);
		Task DeleteUserProfile(string userId);
	}
}
