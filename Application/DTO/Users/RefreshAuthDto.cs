namespace IdentityX.Application.DTO.Users
{
	public class RefreshAuthDto
	{
		public string AccountId { get; set; }
		public string RefreshToken { get; set; }
		public bool KeepLoggedIn { get; set; }
	}
}
