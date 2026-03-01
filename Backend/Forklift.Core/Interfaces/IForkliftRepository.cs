using Forklift.Core.Entities;

namespace Forklift.Core.Interfaces;

/// <summary>
/// Interface for forklift repository operations.
/// </summary>
public interface IForkliftRepository
{
    /// <summary>
    /// Gets all forklift items from the repository.
    /// </summary>
    /// <returns>A collection of all forklift items.</returns>
    Task<IEnumerable<ForkLift>> GetAllAsync();
    /// <summary>
    /// Adds a range of forklift items to the repository.
    /// </summary>
    /// <param name="forklifts">The collection of forklift items to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<ForkLift> forklifts);

    /// <summary>
    /// Deletes a forklift item from the repository by its ID.
    /// </summary>
    /// <param name="id">The ID of the forklift item to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteForkliftByIdAsync(Guid id);

    /// <summary>
    /// Deletes all forklift items from the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAllForkliftAsync();
}
