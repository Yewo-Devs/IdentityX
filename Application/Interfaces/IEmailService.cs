using IdentityX.Core.Entities;

namespace IdentityX.Application.Interfaces
{
	public interface IEmailService
	{
		Task SendAccountActivation(string url, AppUser user);
		Task SendPasswordResetLink(string email, string url);
	}
}
