namespace IdentityX.Application.DTO.Users
{
    public class EditUserDto: BaseUserManagementDto
    {
        public string Id { get; set; }
		public string? Username { get; set; }
        public bool AccountEnabled { get; set; } = true;
    }
}
