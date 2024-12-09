namespace IdentityX.Application.DTO.Users
{
    public class RegisterDto: BaseUserManagementDto
	{
		public string? Username { get; set; }
		public string Password { get; set; }
	}
}
