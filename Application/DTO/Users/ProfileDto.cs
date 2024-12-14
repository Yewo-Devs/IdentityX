namespace IdentityX.Application.DTO.Users
{
	public class ProfileDto
	{
		public string UserId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhotoUrl { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
	}
}
