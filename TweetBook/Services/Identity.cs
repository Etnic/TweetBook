using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Domain;
using Tweetbook.Options;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Data;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public class Identity : IIdentity
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters tokenValidationParameters;
        private readonly DataContext context;

        public Identity(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtSettings jwtSettings, TokenValidationParameters tokenValidationParameters, DataContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.context = context;
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

            return await GenerateAuthenitcationResult(user);
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

            return await GenerateAuthenitcationResult(user);
        }

        public async Task<AuthenticationResult> Refresh(string token, string refreshToken)
        {
            var validToken = this.GetPrincipalFromToken(token);

            if (validToken == null)
            {
                return new AuthenticationResult
                {
                    ErrorMessage = new[] { "Invalid token" }
                };
            }

            var expiryDateUnix = long.Parse(validToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This token hasn't expired yet" } };
            }

            var jti = validToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await this.context.RefreshToken.SingleOrDefaultAsync(x => x.Token == refreshToken);

            if (storedRefreshToken == null)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This refresh token does not exist" } };
            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This refresh token has expired" } };
            }

            if (storedRefreshToken.Invalidated)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This refresh token has been invalidated" } };
            }

            if (storedRefreshToken.Used)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This refresh token has been used" } };
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult { ErrorMessage = new[] { "This refresh token does not match this JWT" } };
            }

            storedRefreshToken.Used = true;
            this.context.RefreshToken.Update(storedRefreshToken);
            await this.context.SaveChangesAsync();

            var user = await this.userManager.FindByIdAsync(validToken.Claims.Single(x => x.Type == "id").Value);
            return await GenerateAuthenitcationResult(user);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecAlgorithm(validatedToken))
                {
                    return null;
                }
                else
                {
                    return principal;
                }
            }
            catch { return null; }
        }

        private bool IsJwtWithValidSecAlgorithm(SecurityToken securityToken)
        {
            return (securityToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResult> GenerateAuthenitcationResult(IdentityUser user)
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
                Expires = DateTime.UtcNow.Add(this.jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await this.context.RefreshToken.AddAsync(refreshToken);
            await this.context.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
