using AuctionSite.Data;
using AuctionSite.Models;
using AuctionSite.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization; 
using SixLabors.ImageSharp;
using System.Globalization;
using Microsoft.Extensions.Logging;


namespace AuctionSite.Services
{
    public class AuctionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private readonly ILogger<AuctionService> _logger; 


        public AuctionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IStringLocalizer<SharedResources> localizer,
            ILogger<AuctionService> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of auction items based on various filters and sorting options. (AkA Search Method)
        /// </summary>
        public async Task<List<AuctionItem>> GetFilteredAndSortedAuctionsAsync(
            string searchQuery, string sortBy, string sortOrder, decimal? minPrice, decimal? maxPrice, string statusFilter)
        {
            IQueryable<AuctionItem> auctionItems = _context.AuctionItems
                                                    .Include(a => a.Seller)
                                                    .Include(a => a.Bids);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                string cleanedSearchQuery = new string(searchQuery.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
                string lowerCaseSearchQuery = cleanedSearchQuery.ToLower();

                string currentLanguageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                int searchLanguageLcid = 1033;

                if (currentLanguageCode.Equals("tr", StringComparison.OrdinalIgnoreCase))
                {
                    searchLanguageLcid = 1055;
                }

                auctionItems = auctionItems.Where(a =>
                    (a.Name != null && (EF.Functions.Contains(a.Name, $"FORMSOF(INFLECTIONAL, \"{cleanedSearchQuery}\")", searchLanguageLcid) || a.Name.ToLower().Contains(lowerCaseSearchQuery))) ||
                    (a.Description != null && (EF.Functions.Contains(a.Description, $"FORMSOF(INFLECTIONAL, \"{cleanedSearchQuery}\")", searchLanguageLcid) || a.Description.ToLower().Contains(lowerCaseSearchQuery)))
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

            return await auctionItems.ToListAsync();
        }

        /// <summary>
        /// Creates a new auction item with the provided details and image file. (AkA Create Auction Item Method)
        /// </summary>
        public async Task<(bool success, string errorMessage, AuctionItem? createdItem)> CreateAuctionItemAsync(
            AuctionItem auctionItem, IFormFile? imageFile, ApplicationUser currentUser)
        {
            if (auctionItem.StartTime == DateTime.MinValue)
            {
                return (false, _localizer["StartTimeCannotBeMinValue"].Value, null);
            }
            if (auctionItem.EndTime == DateTime.MinValue)
            {
                return (false, _localizer["EndTimeCannotBeMinValue"].Value, null);
            }

            auctionItem.StartTime = auctionItem.StartTime.ToUniversalTime();
            auctionItem.EndTime = auctionItem.EndTime.ToUniversalTime();

            if (auctionItem.StartTime < DateTime.UtcNow)
            {
                return (false, _localizer["StartTimeCannotBeInPast"].Value, null);
            }

            if (auctionItem.EndTime <= auctionItem.StartTime)
            {
                return (false, _localizer["EndTimeMustBeAfterStartTime"].Value, null);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return (false, _localizer["InvalidImageExtension"].Value, null);
                }

                if (!imageFile.ContentType.StartsWith("image/"))
                {
                    return (false, _localizer["InvalidImageMimeType"].Value, null);
                }

                try
                {
                    using (var stream = imageFile.OpenReadStream())
                    {
                        await Image.LoadAsync(stream);
                        stream.Position = 0;
                    }
                }
                catch (UnknownImageFormatException)
                {
                    return (false, _localizer["CorruptedImage"].Value, null);
                }
                catch (Exception)
                {
                    return (false, _localizer["UnexpectedErrorUploadingImage"].Value, null); 
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "auction_items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
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

            auctionItem.SellerId = currentUser.Id;
            auctionItem.CurrentBid = auctionItem.StartingPrice;
            auctionItem.IsActive = true;
            auctionItem.IsSold = false;
            auctionItem.WinnerId = null;

            _context.Add(auctionItem);
            await _context.SaveChangesAsync();

            return (true, string.Empty, auctionItem);
        }

        /// <summary>
        /// Updates an existing auction item with the provided details and image file. (AkA Update Auction Item Method)
        /// </summary>
        public async Task<(bool success, string errorMessage)> UpdateAuctionItemAsync(
            int id, AuctionItem auctionItem, IFormFile? imageFile, ApplicationUser currentUser)
        {
            if (id != auctionItem.Id)
            {
                return (false, _localizer["InvalidAuctionId"].Value); 
            }

            var existingAuctionItem = await _context.AuctionItems
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(m => m.Id == id);

            if (existingAuctionItem == null)
            {
                return (false, _localizer["AuctionNotFound"].Value);
            }

            if (existingAuctionItem.SellerId != currentUser.Id)
            {
                return (false, _localizer["UnauthorizedEdit"].Value);
            }

            if (existingAuctionItem.EndTime.ToLocalTime() <= DateTime.Now || _context.Bids.Any(b => b.AuctionItemId == id))
            {
                return (false, _localizer["CannotEditActiveOrBiddedAuction"].Value);
            }

            if (auctionItem.StartTime == DateTime.MinValue)
            {
                return (false, _localizer["StartTimeCannotBeMinValue"].Value);
            }
            if (auctionItem.EndTime == DateTime.MinValue)
            {
                return (false, _localizer["EndTimeCannotBeMinValue"].Value);
            }

            auctionItem.StartTime = auctionItem.StartTime.ToUniversalTime();
            auctionItem.EndTime = auctionItem.EndTime.ToUniversalTime();

            if (auctionItem.EndTime <= auctionItem.StartTime)
            {
                return (false, _localizer["EndTimeMustBeAfterStartTime"].Value);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return (false, _localizer["InvalidImageExtension"].Value);
                }

                if (!imageFile.ContentType.StartsWith("image/"))
                {
                    return (false, _localizer["InvalidImageMimeType"].Value);
                }

                try
                {
                    using (var stream = imageFile.OpenReadStream())
                    {
                        await Image.LoadAsync(stream);
                        stream.Position = 0;
                    }
                }
                catch (UnknownImageFormatException)
                {
                    return (false, _localizer["CorruptedImage"].Value);
                }
                catch (Exception)
                {
                    return (false, _localizer["UnexpectedErrorUploadingImage"].Value);
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "auction_items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(existingAuctionItem.ImageUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingAuctionItem.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
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

            _context.Update(auctionItem);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }

        /// <summary>
        /// Deletes an auction item if the current user is the seller and certain conditions are met. (AkA Delete Method)
        /// </summary>
        public async Task<(bool success, string errorMessage)> DeleteAuctionItemAsync(int id, ApplicationUser currentUser)
        {
            var auctionItem = await _context.AuctionItems
                                            .Include(a => a.Bids)
                                            .FirstOrDefaultAsync(m => m.Id == id);

            if (auctionItem == null)
            {
                return (false, _localizer["AuctionNotFound"].Value);
            }

            if (auctionItem.SellerId != currentUser.Id)
            {
                return (false, _localizer["UnauthorizedDeleteAttempt"].Value);
            }

            if (auctionItem.Bids?.Any() == true || auctionItem.EndTime <= DateTime.Now)
            {
                return (false, _localizer["CannotDeleteActiveOrBiddedAuction"].Value);
            }

            if (!string.IsNullOrEmpty(auctionItem.ImageUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, auctionItem.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        return (false, _localizer["ImageDeletionErrorMessage", ex.Message].Value);
                    }
                }
            }

            _context.AuctionItems.Remove(auctionItem);
            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }


        /// <summary>
        /// Places a bid on an auction item, handling all validations and financial transactions.
        /// </summary>
        public async Task<(bool success, string errorMessage)> PlaceBidAsync(int auctionItemId, decimal bidAmount, ApplicationUser currentUser, string currentUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var auctionItem = await _context.AuctionItems
                                                .Include(a => a.Bids)
                                                .Include(a => a.CurrentBidder)
                                                .FirstOrDefaultAsync(a => a.Id == auctionItemId);

                if (auctionItem == null)
                {
                    return (false, _localizer["AuctionNotFound"].Value);
                }

                if (auctionItem.StartTime > DateTime.Now)
                {
                    return (false, _localizer["AuctionNotStartedYet"].Value);
                }

                if (auctionItem.EndTime <= DateTime.Now)
                {
                    return (false, _localizer["AuctionExpiredCannotBid"].Value);
                }

                if (auctionItem.SellerId == currentUserId)
                {
                    return (false, _localizer["CannotBidOnOwnAuction"].Value);
                }

                if (bidAmount <= auctionItem.CurrentBid)
                {
                    return (false, string.Format(_localizer["BidMustBeHigherThanCurrentPrice"].Value, auctionItem.CurrentBid.ToString("C")));
                }

                if (bidAmount < auctionItem.CurrentBid + auctionItem.MinimumBidIncrement)
                {
                    return (false, string.Format(_localizer["BidMustMeetMinimumIncrement"].Value, auctionItem.MinimumBidIncrement.ToString("C")));
                }

                if (currentUser.VirtualBalance < bidAmount)
                {
                    return (false, _localizer["InsufficientBalance"].Value);
                }

                if (auctionItem.CurrentBidderId != null)
                {
                    var previousBidder = await _userManager.FindByIdAsync(auctionItem.CurrentBidderId);
                    if (previousBidder != null)
                    {
                        previousBidder.VirtualBalance += auctionItem.CurrentBid;
                        var updateResult = await _userManager.UpdateAsync(previousBidder);
                        if (!updateResult.Succeeded) throw new Exception("Previous bidder balance update failed.");
                    }
                }

                currentUser.VirtualBalance -= bidAmount;
                var currentUserUpdateResult = await _userManager.UpdateAsync(currentUser);
                if (!currentUserUpdateResult.Succeeded) throw new Exception("Current user balance update failed.");

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
                _context.Update(auctionItem);

                await _context.SaveChangesAsync(); 

                await transaction.CommitAsync();

                return (true, string.Empty); 
            }
            catch (DbUpdateConcurrencyException ex) 
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Concurrency error placing bid for auction {AuctionItemId} by user {UserId}", auctionItemId, currentUserId); 
                return (false, _localizer["ConcurrencyErrorPlacingBid"].Value);
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error placing bid for auction {AuctionItemId} by user {UserId}", auctionItemId, currentUserId);
                return (false, _localizer["UnexpectedErrorPlacingBid"].Value);
            }
        }
        ///
        ///
        /// <summary>
    }
}