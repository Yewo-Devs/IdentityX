﻿namespace IdentityX.Application.DTO.Users
{
	public class LoginDto
	{
		public string? Email { get; set; }
		public string? Username { get; set; }
		public string Password { get; set; }
		public bool KeepLoggedIn { get; set; }
	}
}
