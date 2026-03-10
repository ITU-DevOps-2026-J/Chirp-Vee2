using Core.Interfaces;
using Core.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LatestsRepository : ILatestsRepository
{
    private readonly ChatDbContext _dbContext;

    public LatestsRepository(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public void AddLatest(int? latestId)
    {
        if (!latestId.HasValue) return;
        var newLatest = new Latest()
        {
            LatestCommandId = latestId.Value,
            UpdatedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        _dbContext.Latests.Add(newLatest);
        _dbContext.SaveChanges();
    }

    public async Task<int> GetLatestId()
    {
        var query = (
            from q in _dbContext.Latests
            orderby q.CreatedDate descending
            select q.LatestCommandId
        );

        var result = await query.ToListAsync();
        
        return result.FirstOrDefault();
    }
}