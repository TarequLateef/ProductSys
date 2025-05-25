using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core.DbStructs;
using SharedLiberary.Core.Interfaces;
using SharedLiberary.Models.UserManagment;

namespace Product.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutherController : ControllerBase
    {
        readonly IAutherRepository _auther;

        public AutherController(IAutherRepository auther) => _auther=auther;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _auther.RegisterAsync(registerModel);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshTokenInCookie(result.Token, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel tokenRequestModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _auther.GetTokenAsync(tokenRequestModel);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel roleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _auther.AddRoleAsync(roleModel);

            return string.IsNullOrEmpty(result) ? Ok(roleModel) : BadRequest(result);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken Dto)
        {
            var token = Dto.Token?? Request.Cookies[UserCookie.CookieName];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");

            var result = await _auther.RevokeTokenAsync(token);
            if (!result) return BadRequest("Token is invlid");

            return Ok();
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies[UserCookie.CookieName];

            var result = await _auther.RefreshToken(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }


        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOpetion = new CookieOptions
            {
                HttpOnly=true,
                Expires=expires.ToLocalTime()
            };

            Response.Cookies.Append(UserCookie.CookieName, refreshToken, cookieOpetion);
        }

    }
}
