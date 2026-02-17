using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Data.Seed
{
    public static class SettingsSeeder
    {
        // =========================================================
        // Startup seed: tüm firmalara default bas (idempotent)
        // =========================================================
        public static async Task SeedAsync(ApplicationDbContext db, CancellationToken ct = default)
        {
            var companyIds = await db.Companies
                .AsNoTracking()
                .Select(c => c.Id)
                .ToListAsync(ct);

            foreach (var companyId in companyIds)
            {
                await SeedForCompanyAsync(db, companyId, ct);
            }
        }

        // =========================================================
        // Company bazlı: default key’leri garanti eder (idempotent)
        // =========================================================
        public static async Task SeedForCompanyAsync(ApplicationDbContext db, int companyId, CancellationToken ct = default)
        {
            if (companyId <= 0) return;

            var existing = await db.Settings
                .Where(s => s.CompanyId == companyId)
                .ToDictionaryAsync(s => s.Name, StringComparer.OrdinalIgnoreCase, ct);

            var defaults = GetCompanyDefaultSettings();

            var now = DateTime.UtcNow;

            foreach (var d in defaults)
            {
                if (existing.TryGetValue(d.Name, out var cur))
                {
                    // Var olanı sadece değişiklik varsa güncelle
                    var newVal = d.Value;          // default value
                    var newDesc = d.Description;   // default desc

                    var changed =
                        !string.Equals(cur.Value, newVal, StringComparison.Ordinal) ||
                        !string.Equals(cur.Description, newDesc, StringComparison.Ordinal);

                    if (changed)
                    {
                        cur.Value = newVal;
                        cur.Description = newDesc;
                        cur.UpdatedAt = now;
                    }
                }
                else
                {
                    db.Settings.Add(new Setting
                    {
                        CompanyId = companyId,
                        Name = d.Name,
                        Value = d.Value,
                        Description = d.Description,
                        UpdatedAt = now
                    });
                }
            }

            // Burada SaveChanges yapıyoruz (caller isterse ekstra SaveChanges çağırabilir ama şart değil)
            await db.SaveChangesAsync(ct);
        }

        // =========================================================
        // Helpers
        // =========================================================
        private static List<SeedItem> GetCompanyDefaultSettings()
        {
            return new List<SeedItem>
            {
                // ---------- Rounding ----------
                new("INTERMEDIATE_ROUND_STEP", ToInv(0.01m), "Zwischenrundung (z.B. 0.01 oder 0.05)"),
                new("FINAL_ROUND_STEP",        ToInv(0.01m), "Endrundung (z.B. 0.01 oder 0.05)"),
                new("WITHHOLDING_ROUND_STEP",  ToInv(0.05m), "QST Rundung (typisch 0.05)"),

                // ---------- AHV / IV / EO ----------
                new("AHV_AN_RATE", ToInv(0.0435m), "AHV/IV/EO Arbeitnehmer (0..1)"),
                new("AHV_AG_RATE", ToInv(0.0435m), "AHV/IV/EO Arbeitgeber (0..1)"),

                // ---------- ALV ----------
                new("ALV_RATE_TOTAL",  ToInv(0.022m),  "ALV Gesamt (0..1)"),
                new("ALV_AN_SHARE",    ToInv(0.5m),    "ALV Arbeitnehmer Anteil (0..1)"),
                new("ALV_AG_SHARE",    ToInv(0.5m),    "ALV Arbeitgeber Anteil (0..1)"),
                new("ALV_ANNUAL_CAP",  ToInv(148200m), "ALV Jahrescap (CHF)"),

                // ALV2
                new("ALV2_RATE_TOTAL", ToInv(0.0m), "ALV2 Gesamt (0 wenn yok)"),
                new("ALV2_AN_SHARE",   ToInv(0.5m), "ALV2 Arbeitnehmer Anteil (0..1)"),
                new("ALV2_AG_SHARE",   ToInv(0.5m), "ALV2 Arbeitgeber Anteil (0..1)"),

                // ---------- UVG ----------
                new("UVG_CAP_ANNUAL",           ToInv(148200m), "UVG Jahrescap (CHF)"),
                new("UVG_NBU_MIN_WEEKLY_HOURS", ToInv(8m),      "NBU için min haftalık saat"),
                new("UVG_NBU_AN_RATE",          ToInv(0.01m),   "NBU Arbeitnehmer Rate (0..1)"),
                new("UVG_BU_AG_RATE",           ToInv(0.01m),   "BU Arbeitgeber Rate (0..1)"),

                // ---------- FAK (AG) ----------
                new("FAK_AG_RATE", ToInv(0.02m), "FAK Arbeitgeber Rate (0..1)"),

                // ---------- KTG (gender-based) ----------
                new("KTG_M_AN_RATE", ToInv(0.0m), "KTG Mann Arbeitnehmer Rate (0..1)"),
                new("KTG_M_AG_RATE", ToInv(0.0m), "KTG Mann Arbeitgeber Rate (0..1)"),
                new("KTG_F_AN_RATE", ToInv(0.0m), "KTG Frau Arbeitnehmer Rate (0..1)"),
                new("KTG_F_AG_RATE", ToInv(0.0m), "KTG Frau Arbeitgeber Rate (0..1)"),

                // ---------- AHV Verwaltungskosten (VK) ----------
                new("AHV_ADMIN_COST_RATE", ToInv(0.0m), "VK: (AHV AN + AHV AG) * Rate (0..1)"),

                // ---------- Canteen ----------
                new("CANTEEN_DAY_RATE", ToInv(0.0m), "Kantine pro Tag (CHF)"),

                // ---------- 13. Monatslohn ----------
                new("THIRTEENTH_ENABLED", "false", "13. Monatslohn aktiv? (true/false)"),
                new("THIRTEENTH_PRORATE", "false", "13. Monatslohn auf 12 verteilen? (true/false)")
            };
        }

        private static string ToInv(decimal d) => d.ToString(CultureInfo.InvariantCulture);

        private sealed record SeedItem(string Name, string Value, string? Description);
    }
}
