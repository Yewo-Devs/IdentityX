namespace IdentityX.Application.DTO.Users
{
	public class ProfileDto
	{
		public string UserId { get; set; }
		public string Username { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string PhotoUrl { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
	}
}
