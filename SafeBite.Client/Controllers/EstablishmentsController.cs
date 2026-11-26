using Microsoft.AspNetCore.Mvc;
using SafeBite.Client.Models;
using SafeBite.Client.Services;

namespace SafeBite.Client.Controllers
{
    // Admin pages for establishments. Each action calls one of the six API methods.
    public class EstablishmentsController : Controller
    {
        private const string Resource = SafeBiteApiClient.Establishments;
        private readonly SafeBiteApiClient _api;

        public EstablishmentsController(SafeBiteApiClient api) => _api = api;

        // Fields the PATCH page lets you change (JSON Patch path -> label)
        public static readonly Dictionary<string, string> PatchableFields = new()
        {
            ["/name"] = "Name",
            ["/street"] = "Street",
            ["/postalCode"] = "Postal code",
            ["/phone"] = "Phone",
        };

        // GET (GetAll)
        public async Task<IActionResult> Index(string? search)
        {
            try
            {
                var items = await _api.GetAllAsync<EstablishmentDto>(Resource);
                if (!string.IsNullOrWhiteSpace(search))
                    items = items.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewData["Search"] = search;
                return View(items.OrderBy(e => e.EstablishmentId).ToList());
            }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
            {
                ViewData["Error"] = ex.Message;
                return View(new List<EstablishmentDto>());
            }
        }

        // GET (GetById) + the badge endpoint
        public async Task<IActionResult> Details(int id)
        {
            var item = await _api.GetByIdAsync<EstablishmentDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Badge"] = await _api.GetBadgeAsync(id);
            return View(item);
        }

        // POST
        public IActionResult Create() => View(new EstablishmentCreateDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EstablishmentCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var created = await _api.CreateAsync<EstablishmentDto>(Resource, dto);
                TempData["Message"] = $"POST succeeded: created establishment #{created?.EstablishmentId}.";
                return RedirectToAction(nameof(Details), new { id = created?.EstablishmentId });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // PUT (replace every field)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _api.GetByIdAsync<EstablishmentDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Id"] = id;
            return View(new EstablishmentUpdateDto
            {
                Name = item.Name, Street = item.Street, PostalCode = item.PostalCode,
                Phone = item.Phone, Latitude = item.Latitude, Longitude = item.Longitude
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EstablishmentUpdateDto dto)
        {
            ViewData["Id"] = id;
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _api.ReplaceAsync(Resource, id, dto);
                TempData["Message"] = $"PUT succeeded: establishment #{id} replaced.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // PATCH (change one field)
        public async Task<IActionResult> Patch(int id)
        {
            var item = await _api.GetByIdAsync<EstablishmentDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Item"] = item;
            return View(new PatchFieldForm { Id = id, Field = "/phone", Value = item.Phone });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Patch(PatchFieldForm form)
        {
            if (!PatchableFields.ContainsKey(form.Field))
                ModelState.AddModelError(nameof(form.Field), "Pick a field.");
            if (ModelState.IsValid)
            {
                try
                {
                    var value = string.IsNullOrWhiteSpace(form.Value) ? null : form.Value.Trim();
                    await _api.PatchAsync(Resource, form.Id, new[] { new JsonPatchOperation("replace", form.Field, value) });
                    TempData["Message"] = $"PATCH succeeded: {PatchableFields[form.Field]} updated on establishment #{form.Id}.";
                    return RedirectToAction(nameof(Details), new { id = form.Id });
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["Item"] = await _api.GetByIdAsync<EstablishmentDto>(Resource, form.Id);
            return View(form);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _api.GetByIdAsync<EstablishmentDto>(Resource, id);
            return item is null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteAsync(Resource, id);
                TempData["Message"] = $"DELETE succeeded: establishment #{id} and its inspections were removed.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
