using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhatsAppClone.Api.Interface;
using WhatsAppClone.Api.Managers;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthManager auth) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(SendOtpRequest req)
    {
        var r = await auth.SendOtpAsync(req);
        return r.Success ? Ok(r) : BadRequest(r);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest req)
    {
        var r = await auth.VerifyOtpAsync(req);
        return r.Success ? Ok(r) : BadRequest(r);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest req)
    {
        var r = await auth.RefreshAsync(req);
        return r.Success ? Ok(r) : Unauthorized(r);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest req)
        => Ok(await auth.LogoutAsync(long.Parse(User.FindFirst("uid")!.Value), req.DeviceUniqueId));
}
