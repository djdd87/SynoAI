using Microsoft.Extensions.Logging;
using SynoAI.Core.Models;
using SynoAI.Core.Interfaces;

namespace SynoAI.Core.Services;

public class SettingService : ISettingService
{
    public SettingService(ILogger<ISettingService> logger)
    {

    }

    public Task<IEnumerable<Setting>> GetListAsync()
    {
        throw new NotImplementedException();
    }
}