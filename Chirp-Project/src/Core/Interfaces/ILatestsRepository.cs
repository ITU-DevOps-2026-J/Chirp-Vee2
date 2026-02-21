namespace Core.Interfaces;

public interface ILatestsRepository
{
    /// <summary>
    /// Add an entry for the latest command ran by the simulator
    /// </summary>
    /// <param name="latestId">id for the latest command ran by the simulator</param>
    /// <returns>void</returns>
    void AddLatest(int? latestId);
    
    /// <summary>
    /// Gets the latest global value
    /// </summary>
    /// <returns>int</returns>
    Task<int> GetLatestId();
}