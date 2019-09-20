using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Options;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public class Identity : IIdentity
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly JwtSettings jwtSettings;

        public Identity(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtSettings = jwtSettings;
        }

        public async Task<AuthenticationResult> Login(UserLoginRequest userRegistrationRequest)
        {
            var user = await this.userManager.FindByEmailAsync(userRegistrationRequest.Email);

            if (user == null)
            {
                return new AuthenticationResult()
                {
                    ErrorMessage = new[] { "user does not exits" }
                };
            }

            var validPass = await this.userManager.CheckPasswordAsync(user, userRegistrationRequest.Password);

            if (!validPass)
            {
                return new AuthenticationResult()
                {
                    ErrorMessage = new[] { "user/pass wrong" }
                };
            }

            return GenerateAuthenitcationResult(user);
        }

        public async Task<AuthenticationResult> Register(UserLoginRequest userRegistrationRequest)
        {
            var userExists = this.userManager.FindByEmailAsync(userRegistrationRequest.Email);

            if (userExists.Result != null)
            {
                return new AuthenticationResult()
                {
                    ErrorMessage = new[] { "already exists" + userRegistrationRequest.Email }
                };
            }

            var user = new IdentityUser()
            {
                UserName = userRegistrationRequest.Email,
                Email = userRegistrationRequest.Email
            };

            var result = await this.userManager.CreateAsync(user, userRegistrationRequest.Password);

            if (!result.Succeeded)
            {
                return new AuthenticationResult()
                {
                    ErrorMessage = result.Errors.Select(x => x.Description)
                };
            }

            return GenerateAuthenitcationResult(user);
        }

        private AuthenticationResult GenerateAuthenitcationResult(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("id", user.Id),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
