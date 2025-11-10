using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace SwissLohnSystem.UI.Pages.Firmen
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Firma> FirmenListe { get; set; }

        public async Task OnGetAsync()
        {
            FirmenListe = await _context.Firmen
                                .Include(f => f.Mitarbeiter)
                                .Select(f => new Firma
                                {
                                    Id = f.Id,
                                    Name = f.Name,
                                    Adresse = f.Adresse,
                                    SteuerNummer = f.SteuerNummer,
                                    MitarbeiterCount = f.Mitarbeiter.Count
                                })
                                .ToListAsync();
        }
    }

    public class Firma
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Adresse { get; set; }
        public string SteuerNummer { get; set; }
        public int MitarbeiterCount { get; set; }
    }
}
