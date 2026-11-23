using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SafeBite.Api.Dtos;
using SafeBite.Api.Models;
using SafeBite.Api.Repositories;

namespace SafeBite.Api.Controllers
{
    [ApiController]
    [Route("api/establishments")]
    public class EstablishmentsController : ControllerBase
    {
        private readonly IEstablishmentRepository _repo;
        private readonly IMapper _mapper;

        public EstablishmentsController(IEstablishmentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GET api/establishments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstablishmentReadDto>>> GetAll()
        {
            var items = await _repo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<EstablishmentReadDto>>(items));
        }

        // GET api/establishments/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EstablishmentReadDto>> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item is null) return NotFound();
            return Ok(_mapper.Map<EstablishmentReadDto>(item));
        }

        // GET api/establishments/5/badge
        [HttpGet("{id:int}/badge")]
        public async Task<ActionResult<BadgeDto>> GetBadge(int id)
        {
            var item = await _repo.GetWithInspectionsAsync(id);
            if (item is null) return NotFound();
            return Ok(_mapper.Map<BadgeDto>(item));
        }

        // POST api/establishments
        [HttpPost]
        public async Task<ActionResult<EstablishmentReadDto>> Create(EstablishmentCreateDto dto)
        {
            var entity = _mapper.Map<Establishment>(dto);
            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            var result = _mapper.Map<EstablishmentReadDto>(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.EstablishmentId }, result);
        }

        // PUT api/establishments/5  (replace the whole resource)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Replace(int id, EstablishmentUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();

            _mapper.Map(dto, entity);          // copy every field from the DTO onto the entity
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
            return NoContent();
        }

        // PATCH api/establishments/5  (change only some fields)
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, JsonPatchDocument<EstablishmentUpdateDto> patch)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return NotFound();

            var dto = _mapper.Map<EstablishmentUpdateDto>(entity);   // entity -> DTO
            patch.ApplyTo(dto, ModelState);                          // apply the client's changes
            if (!TryValidateModel(dto)) return ValidationProblem(ModelState);

            _mapper.Map(dto, entity);                                // DTO -> entity
            entity.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveAsync();
            return NoContent();
        }

        // DELETE api/establishments/5
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