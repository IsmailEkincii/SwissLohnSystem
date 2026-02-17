using System.Threading;
using System.Threading.Tasks;
using SwissLohnSystem.API.Data;

namespace SwissLohnSystem.API.Data.Seed
{
    public static class CompanySettingsSeeder
    {
        // Controller bunu çağırır
        public static Task EnsureCompanyDefaultsAsync(ApplicationDbContext db, int companyId, CancellationToken ct = default)
            => SettingsSeeder.SeedForCompanyAsync(db, companyId, ct);

        // Bazı yerlerde ct yollamadan çağırmak istersen
        public static Task EnsureCompanyDefaultsAsync(ApplicationDbContext db, int companyId)
            => SettingsSeeder.SeedForCompanyAsync(db, companyId, CancellationToken.None);
    }
}
