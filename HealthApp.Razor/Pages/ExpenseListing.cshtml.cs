using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HealthApp.Domain;
using HealthApp.Razor.Data;

namespace HealthApp.Razor.Pages
{
    public class ExpenseListingModel : PageModel
    {
        private readonly HealthApp.Razor.Data.ApplicationDbContext _context;

        public ExpenseListingModel(HealthApp.Razor.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Expense> Expense { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Expense = await _context.Expenses.ToListAsync();
        }
    }
}
