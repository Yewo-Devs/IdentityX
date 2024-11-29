namespace IdentityX.Application.DTO.Users
{
	public class BaseUserManagementDto
	{
		public string Email { get; set; }		
		public string? Role { get; set; }
		public List<string>? Permissions { get; set; }
	}
}
