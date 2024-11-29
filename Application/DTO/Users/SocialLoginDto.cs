namespace IdentityX.Application.DTO.Users
{
	public class SocialLoginDto: BaseUserManagementDto
	{
		public string Id { get; set; }
		public string PhotoUrl { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
