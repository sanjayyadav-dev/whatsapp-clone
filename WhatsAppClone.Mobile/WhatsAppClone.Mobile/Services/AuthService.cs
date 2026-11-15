using System;
using System.Collections.Generic;
using System.Text;
using static WhatsAppClone.Mobile.Models.ApiModels;

namespace WhatsAppClone.Mobile.Services
{
    public class AuthService(ApiClient api, TokenStore tokens)
    {
        public Task<ApiResponse<string>> SendOtpAsync(string phone)
            => api.PostAsync<string>("api/v1/auth/send-otp", new SendOtpRequest(phone));

        public async Task<ApiResponse<AuthResult>> VerifyOtpAsync(string phone, string otp)
        {
            var r = await api.PostAsync<AuthResult>("api/v1/auth/verify-otp",
                new VerifyOtpRequest(phone, otp, tokens.DeviceId, tokens.Platform, null));
            if (r.Success && r.Data is not null) await tokens.SaveAsync(r.Data);
            return r;
        }

        public Task<ApiResponse<UserDto>> GetMeAsync() => api.GetAsync<UserDto>("api/v1/users/me");

        public Task<ApiResponse<UserDto>> UpdateProfileAsync(string name, string? about)
            => api.PutAsync<UserDto>("api/v1/users/me", new UpdateProfileRequest(name, about, null));
    }
}
