using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Interface;

public interface IContactManager
{
    Task<ApiResponse<List<UserDto>>> SyncAsync(long userId, ContactSyncRequest req);
}