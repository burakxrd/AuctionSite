using AuctionSite.Data;
using AuctionSite.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using System.Globalization; 
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionSite.Services
{
    public class ImageCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageCleanupService(IServiceProvider serviceProvider, IWebHostEnvironment webHostEnvironment)
        {
            _serviceProvider = serviceProvider;
            _webHostEnvironment = webHostEnvironment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Resolve the localizer within the scope
                    var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResources>>();
                    var currentCulture = CultureInfo.CurrentUICulture; // Get current UI culture for localization

                    var itemsToClean = await context.AuctionItems
                        .Where(item => item.EndTime.AddDays(30) < DateTime.Now && item.ImageUrl != null)
                        .ToListAsync(stoppingToken);

                    foreach (var item in itemsToClean)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, item.ImageUrl!.TrimStart('/'));

                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                                item.ImageUrl = null;
                                context.Update(item);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(localizer["ImageDeletionErrorMessage", ex.Message].Value);
                            }
                        }
                    }
                    await context.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
