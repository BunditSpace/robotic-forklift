using Forklift.Core.Entities;
using Forklift.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace Forklift.Infrastructure.Test.Repositories;

public class ForkliftRepositoryTest : IDisposable
{
    private SqliteConnection _connection;

    private DbContextOptions<global::Forklift.Infrastructure.Data.AppDbContext> CreateOptions()
    {
        // Create and open the connection if it doesn't exist
        if (_connection == null)
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
        }

        return new DbContextOptionsBuilder<global::Forklift.Infrastructure.Data.AppDbContext>()
            .UseSqlite(_connection) // Use SQLite instead of In-Memory
            .Options;
    }
    private DbContextOptions<global::Forklift.Infrastructure.Data.AppDbContext> CreateOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<global::Forklift.Infrastructure.Data.AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    #region Forklift repository tests
    /// <summary>
    /// Test to verify that the GetAllAsync method of the ForkliftRepository correctly retrieves all forklift items 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ForkliftRepository_GetAll()
    {
        // Arrange
        var options = CreateOptions($"TestDatabase_Items_{Guid.NewGuid()}");
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var firstForkLift = new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 1", ModelNumber = "First Model", ManufacturingDate = DateTime.Now };
            var secondforkLift = new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 2", ModelNumber = "Second Model", ManufacturingDate = DateTime.Now };
            context.Forklifts.Add(firstForkLift);
            context.Forklifts.Add(secondforkLift);
            await context.SaveChangesAsync();
        }

        // Act
        IEnumerable<ForkLift> retrievedItems;
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var repository = new ForkliftRepository(context);
            retrievedItems = await repository.GetAllAsync();
        }

        // Assert
        Assert.NotNull(retrievedItems);
        Assert.Equal(2, retrievedItems.Count());
        Assert.Contains(retrievedItems, item => item.Name == "Forklift 1" && item.ModelNumber == "First Model");
        Assert.Contains(retrievedItems, item => item.Name == "Forklift 2" && item.ModelNumber == "Second Model");
    }

    /// <summary>
    /// Test to verify that the AddRangeAsync method of the ForkliftRepository correctly adds a range of forklift items to the database 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ForkliftRepository_AddRange()
    {
        // Arrange
        var options = CreateOptions($"TestDatabase_Items_{Guid.NewGuid()}");
        var forklifts = new List<ForkLift>
            {
                new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 1", ModelNumber = "Test Model", ManufacturingDate = DateTime.Now }
            };

        // Act
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var repository = new ForkliftRepository(context);
            await repository.AddRangeAsync(forklifts);
        }

        // Assert
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var savedItem = await context.Forklifts.FirstOrDefaultAsync();
            Assert.NotNull(savedItem);
            Assert.Equal("Forklift 1", savedItem.Name);
            Assert.Equal("Test Model", savedItem.ModelNumber);
        }
    }
    [Fact]
    public async Task ForkliftRepository_DeleteForkliftByIdAsync()
    {
        // Arrange
        var options = CreateOptions($"TestDatabase_Items_{Guid.NewGuid()}");
        Guid forkliftId;
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var forklift = new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 1", ModelNumber = "Test Model", ManufacturingDate = DateTime.Now };
            context.Forklifts.Add(forklift);
            await context.SaveChangesAsync();
            forkliftId = forklift.Id;
        }

        // Act
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var repository = new ForkliftRepository(context);
            await repository.DeleteForkliftByIdAsync(forkliftId);
        }

        // Assert
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var deletedItem = await context.Forklifts.FindAsync(forkliftId);
            Assert.Null(deletedItem);
        }
    }

    [Fact]
    public async Task ForkliftRepository_DeleteAllForkliftAsync()
    {
        // Arrange
        var options = CreateOptions();
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
            var firstForkLift = new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 1", ModelNumber = "First Model", ManufacturingDate = DateTime.Now };
            var secondforkLift = new ForkLift { Id = Guid.NewGuid(), Name = "Forklift 2", ModelNumber = "Second Model", ManufacturingDate = DateTime.Now };
            context.Forklifts.Add(firstForkLift);
            context.Forklifts.Add(secondforkLift);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var repository = new ForkliftRepository(context);
            await repository.DeleteAllForkliftAsync();
        }

        // Assert
        using (var context = new global::Forklift.Infrastructure.Data.AppDbContext(options))
        {
            var remainingItems = await context.Forklifts.ToListAsync();
            Assert.Empty(remainingItems);
        }
    }

    #endregion
}

