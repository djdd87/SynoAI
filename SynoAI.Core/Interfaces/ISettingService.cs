using SynoAI.Core.Data;

namespace SynoAI.Core.Interfaces;

public interface ISettingService
{
    Task<IEnumerable<Setting>> GetListAsync();
}
