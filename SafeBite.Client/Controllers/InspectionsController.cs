using Microsoft.AspNetCore.Mvc;
using SafeBite.Client.Models;
using SafeBite.Client.Services;

namespace SafeBite.Client.Controllers
{
    public class InspectionsController : Controller
    {
        private const string Resource = SafeBiteApiClient.Inspections;
        private readonly SafeBiteApiClient _api;

        public InspectionsController(SafeBiteApiClient api) => _api = api;

        public static readonly Dictionary<string, string> PatchableFields = new()
        {
            ["/status"] = "Status (Pass, Conditional Pass or Closed)",
            ["/infractionSummary"] = "Infraction summary",
            ["/observation"] = "Observation",
        };

        // GET (GetAll), optionally filtered to one establishment
        public async Task<IActionResult> Index(int? establishmentId, string? status)
        {
            try
            {
                var items = await _api.GetAllAsync<InspectionDto>(Resource);
                if (establishmentId is not null) items = items.Where(i => i.EstablishmentId == establishmentId).ToList();
                if (!string.IsNullOrWhiteSpace(status)) items = items.Where(i => i.Status == status).ToList();
                ViewData["EstablishmentId"] = establishmentId;
                ViewData["Status"] = status;
                return View(items);
            }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
            {
                ViewData["Error"] = ex.Message;
                return View(new List<InspectionDto>());
            }
        }

        // GET (GetById)
        public async Task<IActionResult> Details(int id)
        {
            var item = await _api.GetByIdAsync<InspectionDto>(Resource, id);
            if (item is null) return NotFound();
            // Show the owner responses for this inspection (filtered from GetAll on the client side)
            var responses = await _api.GetAllAsync<OwnerResponseDto>(SafeBiteApiClient.OwnerResponses);
            ViewData["Responses"] = responses.Where(r => r.InspectionId == id).ToList();
            return View(item);
        }

        // POST
        public IActionResult Create(int? establishmentId) =>
            View(new InspectionCreateDto { EstablishmentId = establishmentId ?? 1 });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InspectionCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var created = await _api.CreateAsync<InspectionDto>(Resource, dto);
                TempData["Message"] = $"POST succeeded: created inspection #{created?.InspectionId}.";
                return RedirectToAction(nameof(Details), new { id = created?.InspectionId });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // PUT
        public async Task<IActionResult> Edit(int id)
        {
            var i = await _api.GetByIdAsync<InspectionDto>(Resource, id);
            if (i is null) return NotFound();
            ViewData["Id"] = id;
            ViewData["EstablishmentName"] = i.EstablishmentName;
            return View(new InspectionUpdateDto
            {
                InspectionDate = i.InspectionDate, Status = i.Status, InfractionCount = i.InfractionCount,
                CrucialCount = i.CrucialCount, SignificantCount = i.SignificantCount, MinorCount = i.MinorCount,
                InfractionSummary = i.InfractionSummary, Observation = i.Observation
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InspectionUpdateDto dto)
        {
            ViewData["Id"] = id;
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _api.ReplaceAsync(Resource, id, dto);
                TempData["Message"] = $"PUT succeeded: inspection #{id} replaced.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // PATCH
        public async Task<IActionResult> Patch(int id)
        {
            var item = await _api.GetByIdAsync<InspectionDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Item"] = item;
            return View(new PatchFieldForm { Id = id, Field = "/status", Value = item.Status });
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
                    TempData["Message"] = $"PATCH succeeded: {PatchableFields[form.Field]} updated on inspection #{form.Id}.";
                    return RedirectToAction(nameof(Details), new { id = form.Id });
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["Item"] = await _api.GetByIdAsync<InspectionDto>(Resource, form.Id);
            return View(form);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _api.GetByIdAsync<InspectionDto>(Resource, id);
            return item is null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteAsync(Resource, id);
                TempData["Message"] = $"DELETE succeeded: inspection #{id} and its owner responses were removed.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
