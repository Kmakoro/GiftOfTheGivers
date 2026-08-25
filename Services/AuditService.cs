using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;

namespace GiftOfTheGivers.Services;

public interface IAuditService
{
    Task WriteAsync(string action, string? userId = null, string? entityName = null,
        string? entityId = null, string? details = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task WriteAsync(string action, string? userId = null, string? entityName = null,
        string? entityId = null, string? details = null)
    {
        var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
