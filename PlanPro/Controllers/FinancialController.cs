using PlanPro.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PlanPro.Controllers
{
    [Authorize]
    public class FinancialController : Controller
    {
        private readonly Entities db = new Entities();

        public ActionResult Index()
        {

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {

                return new HttpUnauthorizedResult();
            }

            int userId = user.Id;

            var income = db.MonthlyIncomes
                .Where(i => i.UserId == userId && i.Month.Month == DateTime.Now.Month)
                .Select(i => i.Amount)
                .FirstOrDefault();

            var expenses = db.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == DateTime.Now.Month)
                .ToList();

            var goals = db.FinancialGoals
                .Where(g => g.UserId == userId)
                .ToList();

            var viewModel = new FinancialOverviewViewModel
            {
                MonthlyIncome = income,
                TotalExpenses = expenses.Sum(e => e.Amount),
                Goals = goals,
                Expenses = expenses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIncome(decimal MonthlyIncome)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return new HttpUnauthorizedResult();
            }

            if (MonthlyIncome <= 0)
            {
                ModelState.AddModelError("MonthlyIncome", "Please enter a positive amount.");
                return RedirectToAction("Index");
            }

            var existingIncome = db.MonthlyIncomes
                .FirstOrDefault(i => i.UserId == user.Id && i.Month.Year == DateTime.Now.Year && i.Month.Month == DateTime.Now.Month);

            if (existingIncome != null)
            {

                existingIncome.Amount = MonthlyIncome;
            }
            else
            {

                var income = new MonthlyIncome
                {
                    UserId = user.Id,
                    Amount = MonthlyIncome,
                    Month = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                };
                db.MonthlyIncomes.Add(income);
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddExpense(Expense expense)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return new HttpUnauthorizedResult();
            }
            expense.UserId = user.Id;

            if (ModelState.IsValid)
            {
                db.Expenses.Add(expense);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGoal(FinancialGoal goal)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return new HttpUnauthorizedResult();
            }
            goal.UserId = user.Id;

            if (ModelState.IsValid)
            {
                db.FinancialGoals.Add(goal);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExpense(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpUnauthorizedResult();

            var expense = db.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == user.Id);
            if (expense != null)
            {
                db.Expenses.Remove(expense);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGoal(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpUnauthorizedResult();

            var goal = db.FinancialGoals.FirstOrDefault(g => g.Id == id && g.UserId == user.Id);
            if (goal != null)
            {
                db.FinancialGoals.Remove(goal);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteIncome()
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpUnauthorizedResult();

            var income = db.MonthlyIncomes.FirstOrDefault(i => i.UserId == user.Id && i.Month.Year == DateTime.Now.Year && i.Month.Month == DateTime.Now.Month);
            if (income != null)
            {
                db.MonthlyIncomes.Remove(income);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditExpense(Expense updatedExpense)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null) return new HttpUnauthorizedResult();

            var expense = db.Expenses.FirstOrDefault(e => e.Id == updatedExpense.Id && e.UserId == user.Id);
            if (expense != null)
            {
                expense.Amount = updatedExpense.Amount;
                expense.Category = updatedExpense.Category;
                expense.Date = updatedExpense.Date;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGoal(FinancialGoal updatedGoal)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null) return new HttpUnauthorizedResult();

            var goal = db.FinancialGoals.FirstOrDefault(g => g.Id == updatedGoal.Id && g.UserId == user.Id);
            if (goal != null)
            {
                goal.GoalName = updatedGoal.GoalName;
                goal.TargetAmount = updatedGoal.TargetAmount;
                goal.Deadline = updatedGoal.Deadline;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Charts(DateTime? startDate, DateTime? endDate)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return new HttpUnauthorizedResult();
            }

            var today = DateTime.Now;
            DateTime defaultStart = new DateTime(today.Year, today.Month, 1);
            DateTime defaultEnd = defaultStart.AddMonths(1).AddDays(-1);

            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = defaultStart;
                endDate = defaultEnd;
            }

            var expensesQuery = db.Expenses
                .Where(e => e.UserId == user.Id && e.Date >= startDate.Value && e.Date <= endDate.Value);

            var incomeQuery = db.MonthlyIncomes
                .Where(i => i.UserId == user.Id && i.Month >= startDate.Value && i.Month <= endDate.Value);

            var filteredExpenses = expensesQuery.ToList();
            var filteredIncome = incomeQuery.ToList();

            var viewModel = new FinancialOverviewViewModel
            {
                MonthlyIncome = filteredIncome.Sum(i => i.Amount),
                TotalExpenses = filteredExpenses.Sum(e => e.Amount),
                Expenses = filteredExpenses,
                Goals = user.FinancialGoals.ToList()
            };

            return View(viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
