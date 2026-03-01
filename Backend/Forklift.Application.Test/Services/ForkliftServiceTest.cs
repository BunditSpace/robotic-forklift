using Forklift.Application.Services;
using Forklift.Core.Entities;
using Forklift.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;


namespace Forklift.Application.Test.Services;

public class ForkliftServiceTest
{

    [Fact]
    public async Task GetAllForkliftsAsync_ReturnsMappedForkliftDtos()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>
        {
            new() { Id = Guid.NewGuid(), Name = "Forklift A", ModelNumber = "Model-1", ManufacturingDate = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Forklift B", ModelNumber = "Model-2", ManufacturingDate = DateTime.UtcNow }
        });

        var service = new ForkliftService(repositoryMock.Object);
        var result = await service.GetAllForkliftsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, dto => dto.Name == "Forklift A" && dto.ModelNumber == "Model-1");
        Assert.Contains(result, dto => dto.Name == "Forklift B" && dto.ModelNumber == "Model-2");
    }

    [Fact]
    public async Task GetAllForkliftsAsync_ReturnsEmptyCollection_WhenNoForklifts()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        var service = new ForkliftService(repositoryMock.Object);
        var result = await service.GetAllForkliftsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ImportForkliftsAsync_ImportsCsvAndSavesForklifts()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        List<ForkLift>? savedForklifts = null;
        repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()))
            .Callback<IEnumerable<ForkLift>>(items => savedForklifts = items.ToList())
            .Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        var csv = "Name,ModelNumber,ManufacturingDate\nForklift A,Model-1,2024-01-01\nForklift B,Model-2,2024-02-01";
        var fileMock = CreateFileMock(csv, "forklifts.csv");

        await service.ImportForkliftsAsync(fileMock.Object);

        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Once);
        Assert.NotNull(savedForklifts);
        Assert.Equal(2, savedForklifts!.Count);
        Assert.Contains(savedForklifts, item => item.Name == "Forklift A" && item.ModelNumber == "Model-1");
        Assert.Contains(savedForklifts, item => item.Name == "Forklift B" && item.ModelNumber == "Model-2");
    }

    [Fact]
    public async Task ImportForkliftsAsync_ImportsJsonAndSavesForklifts()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        List<ForkLift>? savedForklifts = null;
        repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()))
            .Callback<IEnumerable<ForkLift>>(items => savedForklifts = items.ToList())
            .Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        var json = "[{\"name\":\"Forklift X\",\"modelNumber\":\"Model-X\",\"manufacturingDate\":\"2024-05-10\"}]";
        var fileMock = CreateFileMock(json, "forklifts.json");

        await service.ImportForkliftsAsync(fileMock.Object);

        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Once);
        Assert.NotNull(savedForklifts);
        Assert.Single(savedForklifts!);
        Assert.Equal("Forklift X", savedForklifts![0].Name);
        Assert.Equal("Model-X", savedForklifts[0].ModelNumber);
    }

    [Fact]
    public async Task ImportForkliftsAsync_ImportsCsvWithSnakeCaseHeaders()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        List<ForkLift>? savedForklifts = null;
        repositoryMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()))
            .Callback<IEnumerable<ForkLift>>(items => savedForklifts = items.ToList())
            .Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        var csv = "forklift_name,model_number,manufacturing_date\nForklift S,Model-S,2024-03-01";
        var fileMock = CreateFileMock(csv, "forklifts.csv");

        await service.ImportForkliftsAsync(fileMock.Object);

        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Once);
        Assert.NotNull(savedForklifts);
        Assert.Single(savedForklifts!);
        Assert.Equal("Forklift S", savedForklifts![0].Name);
        Assert.Equal("Model-S", savedForklifts[0].ModelNumber);
    }

    [Fact]
    public async Task ImportForkliftsAsync_Throws_WhenDuplicateExistsInFile()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        var service = new ForkliftService(repositoryMock.Object);
        var csv = "Name,ModelNumber,ManufacturingDate\nForklift A,Model-1,2024-01-01\nForklift A,Model-1,2024-01-02";
        var fileMock = CreateFileMock(csv, "forklifts.csv");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ImportForkliftsAsync(fileMock.Object));

        Assert.Equal("Duplicate forklift records found in the import file.", exception.Message);
        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Never);
    }

    [Fact]
    public async Task ImportForkliftsAsync_Throws_WhenDuplicateExistsInDatabase()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>
        {
            new() { Name = "Forklift A", ModelNumber = "Model-1", ManufacturingDate = DateTime.UtcNow }
        });

        var service = new ForkliftService(repositoryMock.Object);
        var csv = "Name,ModelNumber,ManufacturingDate\nForklift A,Model-1,2024-01-01";
        var fileMock = CreateFileMock(csv, "forklifts.csv");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ImportForkliftsAsync(fileMock.Object));

        Assert.Equal("One or more forklift records already exist.", exception.Message);
        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Never);
    }

    [Fact]
    public async Task ImportForkliftsAsync_Throws_WhenFileExtensionIsUnsupported()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ForkLift>());

        var service = new ForkliftService(repositoryMock.Object);
        var content = "Name,ModelNumber,ManufacturingDate\nForklift A,Model-1,2024-01-01";
        var fileMock = CreateFileMock(content, "forklifts.txt");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ImportForkliftsAsync(fileMock.Object));

        Assert.Equal("Unsupported file format. Only .csv and .json are supported.", exception.Message);
        repositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<ForkLift>>()), Times.Never);
    }

    private static Mock<IFormFile> CreateFileMock(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(bytes.Length);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return fileMock;
    }

    [Fact]
    public async Task DeleteForkliftByIdAsync_DeletesForklift()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        var forkliftId = Guid.NewGuid();
        repositoryMock.Setup(r => r.DeleteForkliftByIdAsync(forkliftId)).Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        await service.DeleteForkliftByIdAsync(forkliftId);

        repositoryMock.Verify(r => r.DeleteForkliftByIdAsync(forkliftId), Times.Once);
    }

    [Fact]
    public async Task DeleteForkliftByIdAsync_Throws_WhenIdIsEmpty()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        var service = new ForkliftService(repositoryMock.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteForkliftByIdAsync(Guid.Empty));

        Assert.Equal("Invalid forklift ID.", exception.Message);
        repositoryMock.Verify(r => r.DeleteForkliftByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
    [Fact]
    public async Task DeleteForkliftByIdAsync_Throws_WhenIdIsNull()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        var service = new ForkliftService(repositoryMock.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteForkliftByIdAsync(Guid.Empty));

        Assert.Equal("Invalid forklift ID.", exception.Message);
        repositoryMock.Verify(r => r.DeleteForkliftByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
    [Fact]
    public async Task DeleteForkliftByIdAsync_DeletesExistingForklift()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        var forkliftId = Guid.NewGuid();
        repositoryMock.Setup(r => r.DeleteForkliftByIdAsync(forkliftId)).Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        await service.DeleteForkliftByIdAsync(forkliftId);

        repositoryMock.Verify(r => r.DeleteForkliftByIdAsync(forkliftId), Times.Once);
    }

    [Fact]
    public async Task DeleteAllForkliftAsync_DeletesAllForklifts()
    {
        var repositoryMock = new Mock<IForkliftRepository>();
        repositoryMock.Setup(r => r.DeleteAllForkliftAsync()).Returns(Task.CompletedTask);

        var service = new ForkliftService(repositoryMock.Object);
        await service.DeleteAllForkliftAsync();

        repositoryMock.Verify(r => r.DeleteAllForkliftAsync(), Times.Once);
    }
}
