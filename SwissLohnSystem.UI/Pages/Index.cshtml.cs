using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.API.Data;
using System.Linq;

namespace SwissLohnSystem.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public int TotalCompanies { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalLohns { get; set; }
        public decimal TotalNetSalary { get; set; }
        public List<decimal> MonthlyNetSalaries { get; set; } = new();

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            TotalCompanies = _context.Companies.Count();
            TotalEmployees = _context.Employees.Count();
            TotalLohns = _context.Lohns.Count();
            TotalNetSalary = _context.Lohns.Sum(l => l.NetSalary);

            MonthlyNetSalaries = _context.Lohns
                .GroupBy(l => l.Month)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(l => l.NetSalary))
                .ToList();
        }
    }
}
