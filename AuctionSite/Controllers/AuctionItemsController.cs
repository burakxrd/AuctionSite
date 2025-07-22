using Microsoft.AspNetCore.Mvc;
using AuctionSite.Data;
using AuctionSite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using AuctionSite.Resources;
using System.Globalization;
using SixLabors.ImageSharp;
using AuctionSite.Services;


namespace AuctionSite.Controllers
{
    public class AuctionItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AuctionService _auctionService;


        public AuctionItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<SharedResources> sharedLocalizer,
            IWebHostEnvironment webHostEnvironment,
            AuctionService auctionService)
        {
            _context = context;
            _userManager = userManager;
            _sharedLocalizer = sharedLocalizer;
            _webHostEnvironment = webHostEnvironment;
            _auctionService = auctionService;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>


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

        /// <summary>
        /// Searches for auction items based on the specified filters and sorting criteria. (AkA Search Method)
        /// </summary>
        /// <remarks>This method retrieves auction items that match the specified search criteria and
        /// sorting options. The filters and sorting parameters are passed to the underlying auction service for
        /// processing. The search results are displayed in a view, and the current search parameters are stored in <see
        /// cref="ViewData"/> for use in the view.</remarks>
        /// <param name="searchQuery">The text to search for in auction item titles or descriptions. Can be null or empty to include all items.</param>
        /// <param name="sortBy">The field by which to sort the results, such as "Price" or "DateCreated". Must match a valid sorting field.</param>
        /// <param name="sortOrder">The order in which to sort the results. Use <see langword="asc"/> for ascending or <see langword="desc"/>
        /// for descending.</param>
        /// <param name="minPrice">The minimum price of auction items to include in the results. Can be null to exclude this filter.</param>
        /// <param name="maxPrice">The maximum price of auction items to include in the results. Can be null to exclude this filter.</param>
        /// <param name="statusFilter">The status of auction items to include in the results, such as "Active" or "Closed". Can be null to include
        /// all statuses.</param>
        /// <returns>An <see cref="IActionResult"/> containing the filtered and sorted list of auction items. The result is
        /// displayed in a view.</returns>
        public async Task<IActionResult> Search(string searchQuery, string sortBy, string sortOrder, decimal? minPrice, decimal? maxPrice, string statusFilter)
        {
            var auctionItems = await _auctionService.GetFilteredAndSortedAuctionsAsync(
                searchQuery, sortBy, sortOrder, minPrice, maxPrice, statusFilter);

            ViewData["CurrentSearchQuery"] = searchQuery;
            ViewData["SortBy"] = sortBy;
            ViewData["SortOrder"] = sortOrder;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["StatusFilter"] = statusFilter;

            return View(auctionItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auctionItem"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
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

            if (!ModelState.IsValid)
            {
                if (auctionItem.StartTime.Kind == DateTimeKind.Utc) auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
                if (auctionItem.EndTime.Kind == DateTimeKind.Utc) auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
                return View(auctionItem);
            }

            var (success, errorMessage, createdItem) = await _auctionService.CreateAuctionItemAsync(auctionItem, imageFile, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionCreatedSuccessfully", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                if (auctionItem.StartTime.Kind == DateTimeKind.Utc) auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
                if (auctionItem.EndTime.Kind == DateTimeKind.Utc) auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
                return View(auctionItem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

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

            if (auctionItem.EndTime <= DateTime.Now || auctionItem.Bids?.Any() == true)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("CannotEditActiveOrBiddedAuction", currentCulture) ?? "";
                return RedirectToAction(nameof(Details), new { id = auctionItem.Id });
            }

            auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
            auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();

            return View(auctionItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auctionItem"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartingPrice,MinimumBidIncrement,StartTime,EndTime")] AuctionItem auctionItem, IFormFile? imageFile)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                if (auctionItem.StartTime.Kind == DateTimeKind.Utc) auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
                if (auctionItem.EndTime.Kind == DateTimeKind.Utc) auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
                return View(auctionItem);
            }

            var (success, errorMessage) = await _auctionService.UpdateAuctionItemAsync(id, auctionItem, imageFile, currentUser!);

            if (success)
            {
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionUpdatedSuccessfully", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                if (auctionItem.StartTime.Kind == DateTimeKind.Utc) auctionItem.StartTime = auctionItem.StartTime.ToLocalTime();
                if (auctionItem.EndTime.Kind == DateTimeKind.Utc) auctionItem.EndTime = auctionItem.EndTime.ToLocalTime();
                return View(auctionItem);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>


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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || auctionItem.SellerId != currentUser.Id)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequired") ?? "";
                return Unauthorized();
            }

            var (success, errorMessage) = await _auctionService.DeleteAuctionItemAsync(id, currentUser);

            if (success)
            {
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AuctionDeletedSuccessfully", currentCulture) ?? "";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        private bool AuctionItemExists(int id)
        {
            return _context.AuctionItems.Any(e => e.Id == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="auctionItemId"></param>
        /// <param name="bidAmount"></param>
        /// <returns></returns>

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

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
        ///
        ///
        ///

    }
}