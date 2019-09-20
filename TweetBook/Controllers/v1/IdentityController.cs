using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TweetBook.Contracts.v1.Requests;
using TweetBook.Contracts.v1.Response;
using TweetBook.Contracts.V1;
using TweetBook.Services;

namespace TweetBook.Controllers.v1
{
    public class IdentityController : Controller
    {
        private readonly IIdentity identity;

        public IdentityController(IIdentity identity)
        {
            this.identity = identity;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            var authResponse = await this.identity.Register(userRegistrationRequest);

            if (!authResponse.Success)
            {
                return BadRequest(authResponse.ErrorMessage);
            }

            return Ok(new AuthResponse()
            {
                Token = authResponse.Token
            });
        }
    }
}
