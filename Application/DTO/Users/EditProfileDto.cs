using IdentityX.Core.Models;

namespace IdentityX.Application.DTO.Users
{
	public class EditProfileDto
	{
		public string UserId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string? PhotoUrl { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public PhoneNumber? PhoneNumber { get; set; }
		public string? Address { get; set; }
	}
}
