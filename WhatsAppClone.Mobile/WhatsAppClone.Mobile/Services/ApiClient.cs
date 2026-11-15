using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static WhatsAppClone.Mobile.Models.ApiModels;

namespace WhatsAppClone.Mobile.Services
{
    public class ApiClient(HttpClient http, TokenStore tokens)
    {
        private static readonly JsonSerializerOptions J = new(JsonSerializerDefaults.Web);

        public Task<ApiResponse<T>> PostAsync<T>(string url, object? body, bool auth = false) => SendAsync<T>(HttpMethod.Post, url, body, auth);
        public Task<ApiResponse<T>> PutAsync<T>(string url, object? body) => SendAsync<T>(HttpMethod.Put, url, body, true);
        public Task<ApiResponse<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url, null, true);

        private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body, bool auth, bool retry = true)
        {
            try
            {
                using var req = new HttpRequestMessage(method, url);
                if (body is not null) req.Content = JsonContent.Create(body, options: J);
                if (auth && await tokens.GetAccessAsync() is { } at)
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", at);

                using var res = await http.SendAsync(req);

                if (res.StatusCode == HttpStatusCode.Unauthorized && auth && retry && await RefreshAsync())
                    return await SendAsync<T>(method, url, body, auth, retry: false);

                var text = await res.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(text))
                    return new(false, default, $"Server error ({(int)res.StatusCode}).");

                return JsonSerializer.Deserialize<ApiResponse<T>>(text, J) ?? new(false, default, "Invalid response.");
            }
            catch (Exception ex)
            {
                return new(false, default, "Network error: " + ex.Message);
            }
        }

        private async Task<bool> RefreshAsync()
        {
            var refresh = await tokens.GetRefreshAsync();
            var uid = await tokens.GetUserIdAsync();
            if (refresh is null || uid is null) return false;

            var r = await SendAsync<AuthResult>(HttpMethod.Post, "api/v1/auth/refresh-token",
                new RefreshTokenRequest(uid.Value, tokens.DeviceId, tokens.Platform, refresh), auth: false);

            if (!r.Success || r.Data is null) { tokens.Clear(); return false; }
            await tokens.SaveAsync(r.Data);
            return true;
        }
    }
}
