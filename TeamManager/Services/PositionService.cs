using Microsoft.EntityFrameworkCore;
using TeamManager.Data;
using TeamManager.Models;

namespace TeamManager.Services;

public class PositionService(SportManagerDbContext dbContext)
{
    public async Task<List<Position>> GetAllAsync()
    {
        return await dbContext.Positions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Position position)
    {
        try
        {
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not save position. Please try again.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var position = await dbContext.Positions.FindAsync(id);
        if (position is null)
        {
            throw new InvalidOperationException("Position not found.");
        }

        try
        {
            dbContext.Positions.Remove(position);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not delete position. It may be used by players.");
        }
    }

    public async Task UpdateAsync(Position updatedPosition)
    {
        var position = await dbContext.Positions.FindAsync(updatedPosition.Id);
        if (position is null)
        {
            throw new InvalidOperationException("Position not found.");
        }

        position.Name = updatedPosition.Name;
        position.NeededSpeed = updatedPosition.NeededSpeed;
        position.NeededEndurance = updatedPosition.NeededEndurance;
        position.NeededStrength = updatedPosition.NeededStrength;
        position.NeededTechnique = updatedPosition.NeededTechnique;

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Could not update position. Please try again.");
        }
    }
}

