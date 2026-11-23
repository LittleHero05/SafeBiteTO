using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SafeBite.Api.Dtos;
using SafeBite.Api.Models;
using SafeBite.Api.Repositories;

namespace SafeBite.Api.Controllers
{
    [ApiController]
    [Route("api/owner-responses")]
    public class OwnerResponsesController : ControllerBase
    {
        private readonly IOwnerResponseRepository _repo;
        private readonly IInspectionRepository _inspections;
        private readonly IMapper _mapper;

        public OwnerResponsesController(IOwnerResponseRepository repo, IInspectionRepository inspections, IMapper mapper)
        {
            _repo = repo;
            _inspections = inspections;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OwnerResponseReadDto>>> GetAll() =>
            Ok(_mapper.Map<IEnumerable<OwnerResponseReadDto>>(await _repo.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OwnerResponseReadDto>> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(_mapper.Map<OwnerResponseReadDto>(item));
        }

        [HttpPost]
        public async Task<ActionResult<OwnerResponseReadDto>> Create(OwnerResponseCreateDto dto)
        {
            if (await _inspections.GetByIdAsync(dto.InspectionId) is null)
                return BadRequest($"Inspection {dto.InspectionId} does not exist.");

            var entity = _mapper.Map<OwnerResponse>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.OwnerResponseId },
                                   _mapper.Map<OwnerResponseReadDto>(entity));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Replace(int id, OwnerResponseUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, JsonPatchDocument<OwnerResponseUpdateDto> patch)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();
            var dto = _mapper.Map<OwnerResponseUpdateDto>(entity);
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