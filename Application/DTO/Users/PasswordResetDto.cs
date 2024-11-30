namespace IdentityX.Application.DTO.Users
{
	public class PasswordResetDto
	{
		public string AccountId { get; set; }
		public string Token { get; set; }
		public string Password { get; set; }
	}
}
