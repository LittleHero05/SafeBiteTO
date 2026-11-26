using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SafeBite.Client.Models;
using SafeBite.Client.Services;

namespace SafeBite.Client.Controllers
{
    // The mock food delivery listing from the proposal: restaurant cards, each with its SafeBite badge.
    public class HomeController : Controller
    {
        private const int PageSize = 12;
        private readonly SafeBiteApiClient _api;

        public HomeController(SafeBiteApiClient api) => _api = api;

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var all = (await _api.GetAllAsync<EstablishmentDto>(SafeBiteApiClient.Establishments))
                          .OrderBy(e => e.EstablishmentId).ToList();
                var totalPages = Math.Max(1, (int)Math.Ceiling(all.Count / (double)PageSize));
                page = Math.Clamp(page, 1, totalPages);

                // One badge request per card on this page, sent in parallel
                var onPage = all.Skip((page - 1) * PageSize).Take(PageSize);
                var badges = await Task.WhenAll(onPage.Select(e => _api.GetBadgeAsync(e.EstablishmentId)));

                ViewData["Page"] = page;
                ViewData["TotalPages"] = totalPages;
                return View(badges.Where(b => b is not null).Select(b => b!).ToList());
            }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
            {
                ViewData["Error"] = ex.Message;
                return View(new List<BadgeDto>());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
