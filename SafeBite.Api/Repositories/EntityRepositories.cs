using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Data;
using SafeBite.Api.Models;

namespace SafeBite.Api.Repositories
{
    // Establishments: extra method for the badge (needs the inspections too)
    public interface IEstablishmentRepository : IRepository<Establishment>
    {
        Task<Establishment?> GetWithInspectionsAsync(int id);
    }

    public class EstablishmentRepository : Repository<Establishment>, IEstablishmentRepository
    {
        public EstablishmentRepository(SafeBiteDbContext context) : base(context) { }

        public async Task<Establishment?> GetWithInspectionsAsync(int id) =>
            await _set.AsNoTracking()
                      .Include(e => e.Inspections)
                      .FirstOrDefaultAsync(e => e.EstablishmentId == id);
    }

    // Inspections: include the establishment name for flattened DTOs
    public interface IInspectionRepository : IRepository<Inspection> { }

    public class InspectionRepository : Repository<Inspection>, IInspectionRepository
    {
        public InspectionRepository(SafeBiteDbContext context) : base(context) { }

        public override async Task<IEnumerable<Inspection>> GetAllAsync() =>
            await _set.AsNoTracking().Include(i => i.Establishment)
                      .OrderByDescending(i => i.InspectionDate).ToListAsync();

        public override async Task<Inspection?> GetByIdAsync(int id) =>
            await _set.Include(i => i.Establishment)
                .FirstOrDefaultAsync(i => i.InspectionId == id);
    }

    // Owner responses
    public interface IOwnerResponseRepository : IRepository<OwnerResponse> { }

    public class OwnerResponseRepository : Repository<OwnerResponse>, IOwnerResponseRepository
    {
        public OwnerResponseRepository(SafeBiteDbContext context) : base(context) { }
    }
}