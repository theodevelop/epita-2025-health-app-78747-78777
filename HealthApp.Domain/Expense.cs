using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Domain
{
    public class Expense
    {
        public int Id { get; set; }
        public string? Description { get; set; } = string.Empty;
        public decimal Value { get; set; }

    }
}
