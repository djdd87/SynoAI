using SynoAI.Core.Models;

namespace SynoAI.Core.Interfaces;

public interface ISettingService
{
    /// <summary>
    /// Gets a list of all settings.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{Setting}"/></returns>
    Task<IEnumerable<Setting>> GetListAsync();
}
