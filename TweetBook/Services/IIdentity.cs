using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public interface IIdentity
    {
        Task<AuthenticationResult> Register(UserLoginRequest userRegistrationRequest);
        Task<AuthenticationResult> Login(UserLoginRequest userRegistrationRequest);
        Task<AuthenticationResult> Refresh(string token, string refreshToken);
    }
}
