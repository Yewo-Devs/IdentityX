using IdentityX.Application.Interfaces;
using IdentityX.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityX.Application.Extensions
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AddIdentityXAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = configuration["Token_Issuer"],
						ValidAudience = configuration["Token_Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token_Key"]))
					};
				});

			services.AddScoped<ITokenService, TokenService>();

			return services;
		}

		public static IServiceCollection AddIdentityXUserManagement<EmailServiceImplementation, DataServiceImplementation>(this IServiceCollection services)
		{
			services.AddScoped(typeof(IEmailService), typeof(EmailServiceImplementation));
			services.AddScoped(typeof(IDataService), typeof(DataServiceImplementation));
  
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IProfileService, ProfileService>();

			return services;
		}

		public static IServiceCollection AddIdentityXMultiFactorAuthentication(this IServiceCollection services)
		{
			services.AddScoped<IMultiFactorAuthService, MultiFactorAuthService>();

			return services;
		}

		public static IServiceCollection AddIdentityXRoleBasedAuthorization(this IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
				options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
			});

			return services;
		}
	}
}
