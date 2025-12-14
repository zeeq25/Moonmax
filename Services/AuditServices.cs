using Moonmax.Data;
using Moonmax.Models;
using System;
using System.Threading.Tasks;

namespace Moonmax.Services
{
    public interface IAuditService
    {
        Task LogAsync(int userId, string action, string module, string description, int? targetId = null);
    }

    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;

        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string action, string module, string description, int? targetId = null)
        {
            var auditLog = new AuditLog
            {
                UserID = userId,
                Action = action,
                Module = module,
                Description = description,
                TargetID = targetId,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}