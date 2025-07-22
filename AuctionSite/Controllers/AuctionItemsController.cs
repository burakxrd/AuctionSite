using Microsoft.AspNetCore.Mvc;
using AuctionSite.Data;
using AuctionSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using AuctionSite.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding; 

namespace AuctionSite.Controllers
{
    public class AuctionItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuctionItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResources> sharedLocalizer,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _sharedLocalizer = sharedLocalizer;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            DateTime nowUtc = DateTime.UtcNow;

            var activeAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Include(a => a.Bids)
                .Where(item => item.StartTime <= nowUtc && item.EndTime > nowUtc)
                .OrderBy(item => item.EndTime)
                .ToListAsync();

            var upcomingAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Where(item => item.StartTime > nowUtc)
                .OrderBy(item => item.StartTime)
                .ToListAsync();

            ViewBag.ActiveAuctions = activeAuctions;
            ViewBag.UpcomingAuctions = upcomingAuctions;

            ViewData["Title"] = SharedResources.ResourceManager.GetString("AuctionItems", currentCulture) ?? "";

            return View();
        }
        public async Task<IActionResult> Search(string searchQuery, string sortBy, string sortOrder, decimal? minPrice, decimal? maxPrice, string statusFilter)
        {
            IQueryable<AuctionItem> auctionItems = _context.AuctionItems
                                                    .Include(a => a.Seller)
                                                    .Include(a => a.Bids);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                string lowerCaseSearchQuery = searchQuery.ToLower();
                string currentLanguageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                int searchLanguageLcid = 1033; 

                if (currentLanguageCode.Equals("tr", StringComparison.OrdinalIgnoreCase))
                {
                    searchLanguageLcid = 1055; 
                }

                auctionItems = auctionItems.Where(a =>
                    (a.Name != null && (EF.Functions.Contains(a.Name, $"FORMSOF(INFLECTIONAL, \"{searchQuery}\")", searchLanguageLcid) || a.Name.ToLower().Contains(lowerCaseSearchQuery))) ||
                    (a.Description != null && (EF.Functions.Contains(a.Description, $"FORMSOF(INFLECTIONAL, \"{searchQuery}\")", searchLanguageLcid) || a.Description.ToLower().Contains(lowerCaseSearchQuery)))
                );
            }

            if (minPrice.HasValue)
            {
                auctionItems = auctionItems.Where(a => a.CurrentBid >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                auctionItems = auctionItems.Where(a => a.CurrentBid <= maxPrice.Value);
            }

            DateTime now = DateTime.UtcNow;
            switch (statusFilter?.ToLower())
            {
                case "active": 
                    auctionItems = auctionItems.Where(a => a.StartTime <= now && a.EndTime > now);
                    break;
                case "upcoming":
                    auctionItems = auctionItems.Where(a => a.StartTime > now);
                    break;
                case "expired":
                    auctionItems = auctionItems.Where(a => a.EndTime <= now);
                    break;
                default: 
                    auctionItems = auctionItems.Where(a => a.EndTime > now || a.StartTime > now);
                    break;
            }

            switch (sortBy?.ToLower())
            {
                case "currentbid": 
                    auctionItems = (sortOrder?.ToLower() == "desc") ?
                                   auctionItems.OrderByDescending(a => a.CurrentBid) :
                                   auctionItems.OrderBy(a => a.CurrentBid);
                    break;
                case "timeremaining":
                    auctionItems = (sortOrder?.ToLower() == "desc") ?
                                   auctionItems.OrderByDescending(a => a.EndTime) :
                                   auctionItems.OrderBy(a => a.EndTime);
                    break;
                case "popularity": 
                    auctionItems = (sortOrder?.ToLower() == "desc") ?
                                   auctionItems.OrderByDescending(a => a.Bids.Count) :
                                   auctionItems.OrderBy(a => a.Bids.Count);
                    break;
                default:
                    auctionItems = auctionItems.OrderBy(a => a.EndTime);
                    break;
            }

            ViewData["CurrentSearchQuery"] = searchQuery;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["StatusFilter"] = statusFilter;

            return View(await auctionItems.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Suggest(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return Json(new string[0]);
            }
            var suggestions = await _context.AuctionItems
                                            .Where(a => (a.Name != null && a.Name.ToLower().StartsWith(term.ToLower())))
                                            .Select(a => a.Name)
                                            .Distinct() 
                                            .Take(10) 
                                            .ToListAsync();

            return Json(suggestions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auctionItem = await _context.AuctionItems
                                            .Include(a => a.Seller)
                                            .Include(a => a.Winner)
                                            .Include(a => a.Bids.OrderByDescending(b => b.Amount))
                                                .ThenInclude(b => b.Bidder)
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (auctionItem == null)
            {
                return NotFound();
            }

            ViewBag.IsSeller = auctionItem.SellerId == _userManager.GetUserId(User);

            return View(auctionItem);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,Description,StartingPrice,MinimumBidIncrement,StartTime,EndTime")] AuctionItem auctionItem, IFormFile? imageFile)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequiredToCreateAuction") ?? "";
                return Unauthorized();
            }

            auctionItem.SellerId = currentUser.Id;

            if (ModelState.ContainsKey(nameof(AuctionItem.SellerId)))
            {
                ModelState.Remove(nameof(AuctionItem.SellerId));
            }

            if (auctionItem.StartTime == DateTime.MinValue)
            {
                ModelState.AddModelError(nameof(auctionItem.StartTime), SharedResources.ResourceManager.GetString("StartTimeCannotBeMinValue") ?? "Başlangıç zamanı geçerli bir tarih olmalıdır.");
            }
            if (auctionItem.EndTime == DateTime.MinValue)
            {
                ModelState.AddModelError(nameof(auctionItem.EndTime), SharedResources.ResourceManager.GetString("EndTimeCannotBeMinValue") ?? "Bitiş zamanı geçerli bir tarih olmalıdır.");
            }

            ModelStateEntry? startTimeEntry;
            bool hasStartTimeEntry = ModelState.TryGetValue(nameof(auctionItem.StartTime), out startTimeEntry);
            bool isStartTimeFieldValid = !hasStartTimeEntry || (startTimeEntry != null && !startTimeEntry.Errors.Any());

            ModelStateEntry? endTimeEntry;
            bool hasEndTimeEntry = ModelState.TryGetValue(nameof(auctionItem.EndTime), out endTimeEntry);
            bool isEndTimeFieldValid = !hasEndTimeEntry || (endTimeEntry != null && !endTimeEntry.Errors.Any());

            if (isStartTimeFieldValid && isEndTimeFieldValid)
            {
                auctionItem.StartTime = auctionItem.StartTime.ToUniversalTime();
                auctionItem.EndTime = auctionItem.EndTime.ToUniversalTime();
            }

            if (auctionItem.StartTime < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(auctionItem.StartTime), SharedResources.ResourceManager.GetString("StartTimeCannotBeInPast") ?? "");
            }

            if (auctionItem.EndTime <= auctionItem.StartTime)
            {
                ModelState.AddModelError(nameof(auctionItem.EndTime), SharedResources.ResourceManager.GetString("EndTimeMustBeAfterStartTime") ?? "");
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "auction_items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                auctionItem.ImageUrl = $"/images/auction_items/{uniqueFileName}";
            }
            else
            {
                auctionItem.ImageUrl = "https://placehold.co/600x400/cccccc/333333?text=Resim+Yok";
            }

            if (ModelState.IsValid)
            {
                auctionItem.CurrentBid = auctionItem.StartingPrice;
                auctionItem.IsActive = true;
                auctionItem.IsSold = false;
                auctionItem.WinnerId = null;

                _context.Add(auctionItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionCreatedSuccessfully") ?? "";

                return RedirectToAction(nameof(Index));
            }
            if (auctionItem.StartTime.Kind == DateTimeKind.Utc)
            {
                auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
            }
            if (auctionItem.EndTime.Kind == DateTimeKind.Utc)
            {
                auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
            }
            return View(auctionItem);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (id == null)
            {
                return NotFound();
            }

            var auctionItem = await _context.AuctionItems
                                            .Include(a => a.Bids)
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (auctionItem == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || auctionItem.SellerId != currentUser.Id)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UnauthorizedEdit") ?? "";
                return RedirectToAction(nameof(Index));
            }

            auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
            auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();

            if (auctionItem.EndTime <= DateTime.Now || auctionItem.Bids?.Any() == true)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotEditActiveOrBiddedAuction") ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItem.Id });
            }

            return View(auctionItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartingPrice,MinimumBidIncrement,StartTime,EndTime")] AuctionItem auctionItem, IFormFile? imageFile)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (ModelState.ContainsKey(nameof(AuctionItem.SellerId)))
            {
                ModelState.Remove(nameof(AuctionItem.SellerId));
            }

            if (id != auctionItem.Id)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return Unauthorized();
            }

            var existingAuctionItem = await _context.AuctionItems
                                                    .Include(a => a.Bids)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (existingAuctionItem == null)
            {
                return NotFound();
            }

            if (existingAuctionItem.SellerId != currentUser.Id)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UnauthorizedEdit") ?? "";
                return RedirectToAction(nameof(Index));
            }

            if (existingAuctionItem.EndTime.ToLocalTime() <= DateTime.Now || existingAuctionItem.Bids?.Any() == true)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotEditActiveOrBiddedAuction") ?? "";
                return RedirectToAction(nameof(Details), new { id = existingAuctionItem.Id });
            }

            if (auctionItem.StartTime == DateTime.MinValue)
            {
                ModelState.AddModelError(nameof(auctionItem.StartTime), SharedResources.ResourceManager.GetString("StartTimeCannotBeMinValue") ?? "Başlangıç zamanı geçerli bir tarih olmalıdır.");
            }
            if (auctionItem.EndTime == DateTime.MinValue)
            {
                ModelState.AddModelError(nameof(auctionItem.EndTime), SharedResources.ResourceManager.GetString("EndTimeCannotBeMinValue") ?? "Bitiş zamanı geçerli bir tarih olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "auction_items");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        if (!string.IsNullOrEmpty(existingAuctionItem.ImageUrl))
                        {
                            var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingAuctionItem.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        auctionItem.ImageUrl = $"/images/auction_items/{uniqueFileName}";
                    }
                    else
                    {
                        auctionItem.ImageUrl = existingAuctionItem.ImageUrl;
                    }

                    auctionItem.SellerId = existingAuctionItem.SellerId;
                    auctionItem.CurrentBid = existingAuctionItem.CurrentBid;
                    auctionItem.IsActive = existingAuctionItem.IsActive;
                    auctionItem.IsSold = existingAuctionItem.IsSold;
                    auctionItem.WinnerId = existingAuctionItem.WinnerId;
                    auctionItem.LastBidTime = existingAuctionItem.LastBidTime;

                    bool hasStartTimeEntry = ModelState.TryGetValue(nameof(auctionItem.StartTime), out ModelStateEntry? startTimeEntry);
                    bool isStartTimeFieldValid = !hasStartTimeEntry || (startTimeEntry != null && !startTimeEntry.Errors.Any());

                    bool hasEndTimeEntry = ModelState.TryGetValue(nameof(auctionItem.EndTime), out ModelStateEntry? endTimeEntry);
                    bool isEndTimeFieldValid = !hasEndTimeEntry || (endTimeEntry != null && !endTimeEntry.Errors.Any());

                    if (isStartTimeFieldValid && isEndTimeFieldValid)
                    {
                        auctionItem.StartTime = auctionItem.StartTime.ToUniversalTime();
                        auctionItem.EndTime = auctionItem.EndTime.ToUniversalTime();
                    }
                    else
                    {
                        return View(auctionItem);
                    }

                    _context.Update(auctionItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionUpdatedSuccessfully") ?? "";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionItemExists(auctionItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            if (auctionItem.StartTime.Kind == DateTimeKind.Utc)
            {
                auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
            }
            if (auctionItem.EndTime.Kind == DateTimeKind.Utc)
            {
                auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
            }
            return View(auctionItem);
        }


        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (id == null)
            {
                return NotFound();
            }

            var auctionItem = await _context.AuctionItems
                .Include(a => a.Seller)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auctionItem == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserId(User);
            if (auctionItem.SellerId != currentUser)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UnauthorizedDeleteAttempt", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }

            if (auctionItem.Bids?.Any() == true || auctionItem.EndTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotDeleteActiveOrBiddedAuction", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItem.Id });
            }

            return View(auctionItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var auctionItem = await _context.AuctionItems
                                            .Include(a => a.Bids)
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (auctionItem == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserId(User);
            if (auctionItem.SellerId != currentUser)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UnauthorizedDeleteAttempt", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }

            if (auctionItem.Bids?.Any() == true || auctionItem.EndTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotDeleteActiveOrBiddedAuction", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItem.Id });
            }

            if (!string.IsNullOrEmpty(auctionItem.ImageUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, auctionItem.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.AuctionItems.Remove(auctionItem);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionDeletedSuccessfully", currentCulture) ?? "";
            return RedirectToAction(nameof(Index));
        }

        private bool AuctionItemExists(int id)
        {
            return _context.AuctionItems.Any(e => e.Id == id);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int auctionItemId, decimal bidAmount)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var auctionItem = await _context.AuctionItems
                .Include(a => a.Bids)
                .Include(a => a.CurrentBidder)
                .FirstOrDefaultAsync(a => a.Id == auctionItemId);

            if (auctionItem == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("AuctionNotFound", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }

            if (auctionItem.StartTime > DateTime.Now)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("AuctionNotStartedYet", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            if (auctionItem.EndTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("AuctionExpiredCannotBid", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null || currentUserId == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequiredToBid", currentCulture) ?? "";
                return RedirectToAction("Login", "Account");
            }

            if (auctionItem.SellerId == currentUserId)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotBidOnOwnAuction", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            if (bidAmount <= auctionItem.CurrentBid)
            {
                TempData["ErrorMessage"] = string.Format(SharedResources.ResourceManager.GetString("BidMustBeHigherThanCurrentPrice", currentCulture) ?? "", auctionItem.CurrentBid.ToString("C"));
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            if (bidAmount < auctionItem.CurrentBid + auctionItem.MinimumBidIncrement)
            {
                TempData["ErrorMessage"] = string.Format(SharedResources.ResourceManager.GetString("BidMustMeetMinimumIncrement", currentCulture) ?? "", auctionItem.MinimumBidIncrement.ToString("C"));
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            if (currentUser.VirtualBalance < bidAmount)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("InsufficientBalance", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItemId });
            }

            if (auctionItem.CurrentBidderId != null)
            {
                var previousBidder = await _userManager.FindByIdAsync(auctionItem.CurrentBidderId);
                if (previousBidder != null)
                {
                    previousBidder.VirtualBalance += auctionItem.CurrentBid;
                    await _userManager.UpdateAsync(previousBidder);
                }
            }

            currentUser.VirtualBalance -= bidAmount;
            await _userManager.UpdateAsync(currentUser);

            var bid = new Bid
            {
                AuctionItemId = auctionItemId,
                BidderId = currentUserId,
                Amount = bidAmount,
                BidTime = DateTime.Now
            };

            _context.Bids.Add(bid);
            auctionItem.CurrentBid = bidAmount;
            auctionItem.CurrentBidderId = currentUserId;
            auctionItem.LastBidTime = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("BidPlacedSuccessfully", currentCulture) ?? "";
            return RedirectToAction(nameof(Details), new { id = auctionItemId });
        }

        [Authorize]
        public async Task<IActionResult> WinningAuctions()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return RedirectToAction("Login", "Account");
            }

            var winningAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Where(a => a.WinnerId == currentUser.Id && a.EndTime <= DateTime.UtcNow)
                .OrderByDescending(a => a.EndTime)
                .ToListAsync();

            ViewData["Title"] = SharedResources.ResourceManager.GetString("WinningAuctions") ?? "";

            return View(winningAuctions);
        }

        [Authorize]
        public async Task<IActionResult> YourAuctions()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return RedirectToAction("Login", "Account");
            }

            var userAuctions = await _context.AuctionItems
                .Include(a => a.Seller)
                .Include(a => a.Bids)
                    .ThenInclude(b => b.Bidder)
                .Where(a => a.SellerId == currentUser.Id)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            DateTime nowUtc = DateTime.UtcNow;

            var upcomingAuctions = userAuctions
                .Where(item => item.StartTime > nowUtc)
                .OrderBy(item => item.StartTime)
                .ToList();

            var activeAuctions = userAuctions
                .Where(item => item.StartTime <= nowUtc && item.EndTime > nowUtc)
                .OrderBy(item => item.EndTime)
                .ToList();

            var expiredAuctions = userAuctions
                .Where(item => item.EndTime <= nowUtc)
                .OrderByDescending(item => item.EndTime)
                .ToList();

            ViewData["Title"] = SharedResources.ResourceManager.GetString("YourAuctions") ?? "";

            ViewBag.UpcomingAuctions = upcomingAuctions;
            ViewBag.ActiveAuctions = activeAuctions;
            ViewBag.ExpiredAuctions = expiredAuctions;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> YourBids()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return RedirectToAction("Login", "Account");
            }

            var userBids = await _context.Bids
                .Include(b => b.AuctionItem)
                    .ThenInclude(ai => ai!.Seller)
                .Where(b => b.BidderId == currentUser.Id)
                .OrderByDescending(b => b.BidTime)
                .ToListAsync();

            ViewData["Title"] = SharedResources.ResourceManager.GetString("YourBids") ?? "";

            return View(userBids);
        }
    }
}
