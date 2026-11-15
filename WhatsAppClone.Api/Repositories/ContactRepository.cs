using System.Data;
using System.Text.Json;
using Dapper;
using WhatsAppClone.Api.Models;

namespace WhatsAppClone.Api.Repositories;

public interface IContactRepository
{
    Task<IEnumerable<User>> FindByPhonesAsync(IEnumerable<string> phones, long excludeUserId);
}

public class ContactRepository(IDbConnectionFactory db) : IContactRepository
{
    public async Task<IEnumerable<User>> FindByPhonesAsync(IEnumerable<string> phones, long excludeUserId)
    {
        using var c = db.Create();
        return await c.QueryAsync<User>("dbo.usp_User_FindByPhones",
            new { PhonesJson = JsonSerializer.Serialize(phones), ExcludeUserId = excludeUserId },
            commandType: CommandType.StoredProcedure);
    }
}