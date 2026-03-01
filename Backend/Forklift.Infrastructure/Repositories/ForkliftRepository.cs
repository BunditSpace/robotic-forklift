using Forklift.Core.Entities;
using Forklift.Core.Interfaces;
using Forklift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forklift.Infrastructure.Repositories;

public class ForkliftRepository : IForkliftRepository
{
    private readonly AppDbContext _context;

    public ForkliftRepository(AppDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<IEnumerable<ForkLift>> GetAllAsync()
    {
        return await _context.Forklifts.ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<ForkLift> forklifts)
    {
        await _context.Forklifts.AddRangeAsync(forklifts);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteForkliftByIdAsync(Guid id)
    {
        var forklift = await _context.Forklifts.FindAsync(id);
        if (forklift != null)
        {
            _context.Forklifts.Remove(forklift);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAllForkliftAsync()
    {
        await _context.Forklifts.ExecuteDeleteAsync();
    }
}
