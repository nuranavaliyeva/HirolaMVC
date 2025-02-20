using HirolaMVC.DAL;
using HirolaMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HirolaMVC.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;


        public LayoutService(AppDbContext context)
        {
            _context = context;

        }
        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return settings;
        }
    }
}
