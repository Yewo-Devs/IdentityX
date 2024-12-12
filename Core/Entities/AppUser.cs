namespace IdentityX.Core.Entities
{
	public class AppUser
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
		public List<string> Permissions { get; set; }
		public bool AccountEnabled { get; set; }
		public bool AccountVerified { get; set; }
		public string AccountVerificationToken { get; set; }
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string PasswordResetToken { get; set; }
	}
}
