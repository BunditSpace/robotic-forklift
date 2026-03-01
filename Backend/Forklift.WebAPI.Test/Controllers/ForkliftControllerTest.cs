using Forklift.Application.DTOs;
using Forklift.Application.Interfaces;
using Forklift.Core.Entities;
using Forklift.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Forklift.WebAPI.Test.Controllers;

public class ForkliftControllerTest
{
    #region services and constructor
    private readonly Mock<IForkliftService> _forkliftServiceMock;
    private readonly ForkliftController _controller;

    public ForkliftControllerTest()
    {
        _forkliftServiceMock = new Mock<IForkliftService>();
        _controller = new ForkliftController(_forkliftServiceMock.Object);
    }
    #endregion

    #region endpoint tests
    [Fact]
    public async Task GetAllForklifts_ReturnsOkWithForkliftItems()
    {
        // Arrange
        var forkLiftItems = new List<ForkliftDto>
    {
        new ForkliftDto { Id = Guid.NewGuid(), Name = "Item 1", ModelNumber = "Model A", ManufacturingDate = DateTime.UtcNow },
        new ForkliftDto { Id = Guid.NewGuid(), Name = "Item 2", ModelNumber = "Model B", ManufacturingDate = DateTime.UtcNow }
    };

        _forkliftServiceMock.Setup(x => x.GetAllForkliftsAsync()).ReturnsAsync(forkLiftItems);

        // Act
        var result = await _controller.GetAllForklifts();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, (result as OkObjectResult)?.StatusCode);
        Assert.Equal(forkLiftItems, (result as OkObjectResult)?.Value);
    }

    [Fact]
    public async Task GetAllForklifts_ReturnsEmptyList_WhenNoForkliftsExist()
    {
        // Arrange
        _forkliftServiceMock.Setup(x => x.GetAllForkliftsAsync()).ReturnsAsync(new List<ForkliftDto>());

        // Act
        var result = await _controller.GetAllForklifts();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, (result as OkObjectResult)?.StatusCode);
        Assert.IsType<List<ForkliftDto>>((result as OkObjectResult)?.Value);
        Assert.Empty((result as OkObjectResult)?.Value as List<ForkliftDto> ?? []);
    }

    [Fact]
    public async Task Import_ReturnsBadRequest_WhenFileIsNull()
    {
        // Act
        var result = await _controller.Import(null!);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("File is empty or null.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Import_ReturnsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        // Act
        var result = await _controller.Import(fileMock.Object);
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("File is empty or null.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Import_ReturnsOk_WhenImportIsSuccessful()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        _forkliftServiceMock.Setup(x => x.ImportForkliftsAsync(It.IsAny<IFormFile>())).Returns(Task.CompletedTask);
        // Act
        var result = await _controller.Import(fileMock.Object);
        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, (result as OkObjectResult)?.StatusCode);
        Assert.Contains("Data imported successfully.", (result as OkObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Import_ReturnsBadRequest_WhenDuplicateDataFound()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        fileMock.Setup(f => f.FileName).Returns("test.csv");
        _forkliftServiceMock
            .Setup(x => x.ImportForkliftsAsync(It.IsAny<IFormFile>()))
            .ThrowsAsync(new ArgumentException("Duplicate forklift records found in the import file."));

        // Act
        var result = await _controller.Import(fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("Duplicate forklift records found in the import file.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenDeletionIsSuccessful()
    {
        // arrange
        var id = Guid.NewGuid();
        _forkliftServiceMock.Setup(x => x.DeleteForkliftByIdAsync(id)).Returns(Task.CompletedTask);

        // act
        var result = await _controller.Delete(id);

        // assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, (result as OkObjectResult)?.StatusCode);
        Assert.Contains("Forklift deleted successfully.", (result as OkObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenDeletionFails()
    {
        // arrange
        var id = Guid.NewGuid();
        _forkliftServiceMock.Setup(x => x.DeleteForkliftByIdAsync(id)).ThrowsAsync(new Exception("Deletion failed."));

        // act
        var result = await _controller.Delete(id);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("Deletion failed.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task DeleteAll_ReturnsOk_WhenDeletionIsSuccessful()
    {
        // arrange
        _forkliftServiceMock.Setup(x => x.DeleteAllForkliftAsync()).Returns(Task.CompletedTask);

        // act
        var result = await _controller.DeleteAll();

        // assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, (result as OkObjectResult)?.StatusCode);
        Assert.Contains("All forklifts deleted successfully.", (result as OkObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task DeleteAll_ReturnsBadRequest_WhenDeletionFails()
    {
        // arrange
        _forkliftServiceMock.Setup(x => x.DeleteAllForkliftAsync()).ThrowsAsync(new Exception("Deletion failed."));

        // act
        var result = await _controller.DeleteAll();

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("Deletion failed.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenExceptionThrown()
    {
        // arrange
        var id = Guid.NewGuid();
        _forkliftServiceMock.Setup(x => x.DeleteForkliftByIdAsync(id)).ThrowsAsync(new Exception("An error occurred."));

        // act
        var result = await _controller.Delete(id);

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("An error occurred.", (result as BadRequestObjectResult)?.Value?.ToString());
    }

    [Fact]
    public async Task DeleteAll_ReturnsBadRequest_WhenExceptionThrown()
    {
        // arrange
        _forkliftServiceMock.Setup(x => x.DeleteAllForkliftAsync()).ThrowsAsync(new Exception("An error occurred."));

        // act
        var result = await _controller.DeleteAll();

        // assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, (result as BadRequestObjectResult)?.StatusCode);
        Assert.Contains("An error occurred.", (result as BadRequestObjectResult)?.Value?.ToString());
    }
    #endregion
}
