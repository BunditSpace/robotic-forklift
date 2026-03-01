using Forklift.Application.Interfaces;
using Forklift.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forklift.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ForkliftController : ControllerBase
{
    #region services and constructor
    private readonly IForkliftService _forkliftService;

    public ForkliftController(IForkliftService forkliftService)
    {
        ArgumentNullException.ThrowIfNull(forkliftService);
        _forkliftService = forkliftService;
    }

    #endregion

    #region API Endpoints

    /// <summary>
    /// Gets all forklifts.
    /// </summary>
    /// <returns>A list of all forklifts.</returns>
    [Route("getAllForklifts")]
    [HttpGet]
    public async Task<IActionResult> GetAllForklifts()
    {
        var forklifts = await _forkliftService.GetAllForkliftsAsync();
        return Ok(forklifts);
    }

    /// <summary>
    /// Imports forklift data from a file. The file can be in CSV or JSON format. The method validates the file, parses the data, checks for duplicates, and saves the new forklift records to the database. It returns appropriate responses based on the success or failure of the operation.
    /// </summary>
    /// <param name="file">The file containing forklift data to import.</param>
    /// <returns>An IActionResult indicating the result of the import operation.</returns>
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "File is empty or null." });

        try
        {
            await _forkliftService.ImportForkliftsAsync(file);
            return Ok(new { message = "Data imported successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary> Deletes a forklift item by its ID. The method attempts to delete the specified forklift and returns an appropriate
    /// response based on the success or failure of the operation.
    /// </summary> <param name="id">The ID of the forklift item to delete.</param>
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _forkliftService.DeleteForkliftByIdAsync(id);
            return Ok(new { message = "Forklift deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("deleteAll")]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _forkliftService.DeleteAllForkliftAsync();
            return Ok(new { message = "All forklifts deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    #endregion
}
