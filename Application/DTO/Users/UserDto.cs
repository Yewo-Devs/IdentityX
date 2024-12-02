namespace IdentityX.Application.DTO.Users
{
	public class UserDto
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string Username { get; set; }
		public string Role { get; set; }
		public List<string> Permissions { get; set; }
		public string Token { get; set; }
		public string RefreshToken { get; set; }
	}
}
