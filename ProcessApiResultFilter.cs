using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using TwitchTokenPoc.Services;

namespace TwitchTokenPoc
{
    public class ProcessApiResultFilter : IActionFilter
    {
        private readonly JwtTokenService _jwtTokenService;

        public ProcessApiResultFilter(JwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }
        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // no action
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if ((context.Result as OkObjectResult)?.Value is ApiResult result)
            {
                if (result.TokensChanged)
                {
                    var newTokenString = _jwtTokenService.CreateJwtTokenString(
                        result.NewAccessToken, result.NewRefreshToken).Result;
                    context.HttpContext.Response.Headers.Add("X-NewToken", newTokenString);
                }
                
                context.Result = new ObjectResult(result.Result);
            }
        }
    }
}