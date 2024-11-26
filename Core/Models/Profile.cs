namespace IdentityX.Core.Models
{
	public class Profile
	{
		public string UserId { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }		
		public DateTime DateOfBirth { get; set; }
		public PhoneNumber PhoneNumber { get; set; }
		public string Address { get; set; }
	}
}
