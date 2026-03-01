using Forklift.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Forklift.Application.Interfaces;

/// <summary>
/// interface for forklift service containing methods to get all forklifts and import forklifts from a file
/// </summary>
public interface IForkliftService
{
    /// <summary>
    /// Gets all forklifts.
    /// </summary>
    /// <returns>A collection of forklifts.</returns>
    Task<IEnumerable<ForkliftDto>> GetAllForkliftsAsync();

    /// <summary>
    /// Imports forklifts from a file.
    /// </summary>  
    /// <param name="file">The file to import.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ImportForkliftsAsync(IFormFile file);

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
