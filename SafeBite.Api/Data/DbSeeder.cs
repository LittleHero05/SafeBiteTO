using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Models;

namespace SafeBite.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(SafeBiteDbContext db, string folder)
        {
            if (await db.Establishments.AnyAsync())
                return; // already seeded

            var now = DateTime.UtcNow;

            var establishments = Read<Establishment>(Path.Combine(folder, "establishments.csv"));
            foreach (var e in establishments)
            {
                e.PostalCode = string.IsNullOrWhiteSpace(e.PostalCode) ? null : e.PostalCode;
                e.Phone = string.IsNullOrWhiteSpace(e.Phone) ? null : e.Phone;
                e.CreatedAt = now;
            }

            var inspections = Read<Inspection>(Path.Combine(folder, "inspections.csv"));
            foreach (var i in inspections)
                i.CreatedAt = now;

            var responses = Read<OwnerResponse>(Path.Combine(folder, "owner_responses.csv"));

            // Parents first, so the foreign keys exist
            await InsertWithIdsAsync(db, "Establishments", establishments);
            await InsertWithIdsAsync(db, "Inspections", inspections);
            await InsertWithIdsAsync(db, "OwnerResponses", responses);
        }

        private static List<T> Read<T>(string path)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,     // CSV doesn't have every property (e.g. UpdatedAt)
                MissingFieldFound = null,
                IgnoreReferences = true     // don't try to read navigation properties
            };
            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<T>().ToList();
        }

        // Keep our own IDs (1, 2, 3...) so the foreign keys in the CSVs still match
        private static async Task InsertWithIdsAsync<T>(SafeBiteDbContext db, string table, List<T> rows)
            where T : class
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            db.Set<T>().AddRange(rows);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [" + table + "] ON");
            await db.SaveChangesAsync();
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [" + table + "] OFF");
            await tx.CommitAsync();
            db.ChangeTracker.Clear();
        }
    }
}