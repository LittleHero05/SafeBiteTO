using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SafeBite.Api.Dtos;
using SafeBite.Api.Models;
using SafeBite.Api.Repositories;

namespace SafeBite.Api.Controllers
{
    [ApiController]
    [Route("api/inspections")]
    public class InspectionsController : ControllerBase
    {
        private readonly IInspectionRepository _repo;
        private readonly IEstablishmentRepository _establishments;
        private readonly IMapper _mapper;

        public InspectionsController(IInspectionRepository repo, IEstablishmentRepository establishments, IMapper mapper)
        {
            _repo = repo;
            _establishments = establishments;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspectionReadDto>>> GetAll() =>
            Ok(_mapper.Map<IEnumerable<InspectionReadDto>>(await _repo.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InspectionReadDto>> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(_mapper.Map<InspectionReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult<InspectionReadDto>> Create(InspectionCreateDto dto)
        {
            if (await _establishments.GetByIdAsync(dto.EstablishmentId) is null)
                return BadRequest($"Establishment {dto.EstablishmentId} does not exist.");

            var entity = _mapper.Map<Inspection>(dto);
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            var created = await _repo.GetByIdAsync(entity.InspectionId);   // reload with establishment name
            return CreatedAtAction(nameof(GetById), new { id = entity.InspectionId },
                                   _mapper.Map<InspectionReadDto>(created));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Replace(int id, InspectionUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, JsonPatchDocument<InspectionUpdateDto> patch)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();
            var dto = _mapper.Map<InspectionUpdateDto>(entity);
            patch.ApplyTo(dto, ModelState);
            if (!TryValidateModel(dto)) return ValidationProblem(ModelState);
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();
            _repo.Delete(entity);
            await _repo.SaveAsync();
            return NoContent();
        }
    }
}