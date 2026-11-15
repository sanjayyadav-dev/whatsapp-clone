using System;
using System.Collections.Generic;
using System.Text;
using static WhatsAppClone.Mobile.Models.ApiModels;

namespace WhatsAppClone.Mobile.Services
{
    public class TokenStore
    {
        public async Task SaveAsync(AuthResult r)
        {
            await SecureStorage.SetAsync("access", r.AccessToken);
            await SecureStorage.SetAsync("refresh", r.RefreshToken);
            await SecureStorage.SetAsync("uid", r.User.UserId.ToString());
        }

        public Task<string?> GetAccessAsync() => SecureStorage.GetAsync("access");
        public Task<string?> GetRefreshAsync() => SecureStorage.GetAsync("refresh");
        public async Task<long?> GetUserIdAsync()
            => long.TryParse(await SecureStorage.GetAsync("uid"), out var id) ? id : null;

        public string DeviceId
        {
            get
            {
                var id = Preferences.Get("device_id", "");
                if (id == "") { id = Guid.NewGuid().ToString(); Preferences.Set("device_id", id); }
                return id;
            }
        }

        public string Platform => DeviceInfo.Platform.ToString().ToLowerInvariant();

        public void Clear() => SecureStorage.RemoveAll();
    }
}
