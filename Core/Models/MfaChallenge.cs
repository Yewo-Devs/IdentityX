namespace IdentityX.Core.Models
{
	public class MfaChallenge
	{
		public string Id { get; set; }
		public string Email { get; set; }
		public string Token { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime ExpiryTime { get; set; }
	}
}
