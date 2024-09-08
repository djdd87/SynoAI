using SynoAI.API.Models.Data;

namespace SynoAI.API.Services;

public interface ISettingsService
{
    Task<Setting> GetListAsync();
}

public class SettingsService : ISettingsService
{
    public async Task<Setting> GetListAsync()
    {
        throw new NotImplementedException();
    }
}