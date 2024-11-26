namespace IdentityX.Core.Entities
{
	public class AppUser
	{
		public string Id { get; set; }
		public string Role { get; set; }
		public List<string> Permissions { get; set; }
		public bool AccountEnabled { get; set; }
		public bool AccountVerified { get; set; }
		public byte[] PasswordHash { get; set; }
		public byte[] PasswordSalt { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
