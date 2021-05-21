using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace IdentityClient
{
    public class ValidateTokenHandler : AuthorizationHandler<ValidTokenRequirement>
    {
        private readonly HttpClient httpClient;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ValidateTokenHandler(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidTokenRequirement requirement)
        {
            var accessToken = httpContextAccessor.HttpContext.Request.Headers
                .FirstOrDefault(h => h.Key == HeaderNames.Authorization).Value.FirstOrDefault();

            if (accessToken != null)
            {
                try
                {
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "me");
                    requestMessage.Headers.TryAddWithoutValidation(HeaderNames.Authorization, accessToken);
                    using var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        context.Succeed(requirement);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
