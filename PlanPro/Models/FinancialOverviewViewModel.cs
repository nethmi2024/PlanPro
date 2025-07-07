using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlanPro.Models
{
    public class FinancialOverviewViewModel
    {
        public decimal MonthlyIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public List<FinancialGoal> Goals { get; set; }
        public List<Expense> Expenses { get; set; }

        public decimal CurrentBalance => Math.Max(0, MonthlyIncome - TotalExpenses);

    }

}