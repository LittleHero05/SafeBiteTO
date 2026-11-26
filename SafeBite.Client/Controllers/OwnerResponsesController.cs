using Microsoft.AspNetCore.Mvc;
using SafeBite.Client.Models;
using SafeBite.Client.Services;

namespace SafeBite.Client.Controllers
{
    // The "restaurant owner" side of the app: owners read their inspection and publish a response.
    public class OwnerResponsesController : Controller
    {
        private const string Resource = SafeBiteApiClient.OwnerResponses;
        private readonly SafeBiteApiClient _api;

        public OwnerResponsesController(SafeBiteApiClient api) => _api = api;

        public static readonly Dictionary<string, string> PatchableFields = new()
        {
            ["/message"] = "Message",
            ["/ownerName"] = "Owner name",
        };

        // GET (GetAll)
        public async Task<IActionResult> Index()
        {
            try
            {
                var items = await _api.GetAllAsync<OwnerResponseDto>(Resource);
                return View(items.OrderByDescending(r => r.CreatedAt).ToList());
            }
            catch (Exception ex) when (ex is ApiException or HttpRequestException or TaskCanceledException)
            {
                ViewData["Error"] = ex.Message;
                return View(new List<OwnerResponseDto>());
            }
        }

        // GET (GetById)
        public async Task<IActionResult> Details(int id)
        {
            var item = await _api.GetByIdAsync<OwnerResponseDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Inspection"] = await _api.GetByIdAsync<InspectionDto>(SafeBiteApiClient.Inspections, item.InspectionId);
            return View(item);
        }

        // POST
        public IActionResult Create(int? inspectionId) =>
            View(new OwnerResponseCreateDto { InspectionId = inspectionId ?? 0 });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OwnerResponseCreateDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try
            {
                var created = await _api.CreateAsync<OwnerResponseDto>(Resource, dto);
                TempData["Message"] = $"POST succeeded: response #{created?.OwnerResponseId} published.";
                return RedirectToAction(nameof(Details), new { id = created?.OwnerResponseId });
            }
            catch (ApiException ex)
            {
                // e.g. 400 "Inspection 9999 does not exist."
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // PUT
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _api.GetByIdAsync<OwnerResponseDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Id"] = id;
            return View(new OwnerResponseUpdateDto { OwnerName = item.OwnerName, Message = item.Message });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OwnerResponseUpdateDto dto)
        {
            ViewData["Id"] = id;
            if (!ModelState.IsValid) return View(dto);
            try
            {
                await _api.ReplaceAsync(Resource, id, dto);
                TempData["Message"] = $"PUT succeeded: response #{id} replaced.";
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
            var item = await _api.GetByIdAsync<OwnerResponseDto>(Resource, id);
            if (item is null) return NotFound();
            ViewData["Item"] = item;
            return View(new PatchFieldForm { Id = id, Field = "/message", Value = item.Message });
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
                    await _api.PatchAsync(Resource, form.Id, new[] { new JsonPatchOperation("replace", form.Field, form.Value?.Trim()) });
                    TempData["Message"] = $"PATCH succeeded: {PatchableFields[form.Field]} updated on response #{form.Id}.";
                    return RedirectToAction(nameof(Details), new { id = form.Id });
                }
                catch (ApiException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["Item"] = await _api.GetByIdAsync<OwnerResponseDto>(Resource, form.Id);
            return View(form);
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _api.GetByIdAsync<OwnerResponseDto>(Resource, id);
            return item is null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _api.DeleteAsync(Resource, id);
                TempData["Message"] = $"DELETE succeeded: response #{id} removed.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
