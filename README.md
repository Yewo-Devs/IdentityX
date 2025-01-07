# IdentityX

IdentityX is a powerful and flexible identity management package for .NET 9 applications. It provides a comprehensive set of features to manage user authentication, authorization, and user profiles with ease.

## Features

- **User Authentication**: Securely authenticate users using various methods including password, OAuth, and multi-factor authentication.
- **Authorization**: Fine-grained control over user permissions and roles.
- **User Profiles**: Manage user profile information and preferences.
- **Security**: Built-in support for encryption, hashing, and secure storage of sensitive data.
- **Extensibility**: Easily extend and customize the package to fit your specific needs.

## Installation

To install IdentityX, run the following command in your .NET 9 project:

dotnet add package IdentityX

## Getting Started

### Configuration

1. Add the IdentityX services to your `Startup.cs`:

2. Set the following environment variables:
    - Token_Issuer
    - Token_Audience
    - Token_Key
    - Api_Domain
    - FrontEnd_Domain

4. Implement the service definitions for IEmailService & IDataService

### Usage

#### User Authentication

To authenticate a user, use the `SignInManager`:

var result = await signInManager.PasswordSignInAsync(username, password, isPersistent, lockoutOnFailure); if (result.Succeeded) { // User authenticated successfully } else { // Authentication failed }
    
#### User Authorization

To check if a user is in a specific role:

if (await userManager.IsInRoleAsync(user, "Admin")) { // User is an admin } else { // User is not an admin }

## Contributing

We welcome contributions to IdentityX! Please read our [contributing guidelines](CONTRIBUTING.md) for more information.

## License

IdentityX is licensed under the [MIT License](LICENSE).

## Contact

For any questions or issues, please open an issue on our [GitHub
