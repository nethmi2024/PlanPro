using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PlanPro.Models;

namespace PlanPro.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private Entities db = new Entities();

        // GET: Todo
        public ActionResult Index(string status, string priority)
        {
            var todoItems = db.TodoItems.Include(t => t.User).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                bool isCompleted = status == "completed";
                todoItems = todoItems.Where(t => t.IsCompleted == isCompleted);
            }

            if (!string.IsNullOrEmpty(priority))
            {
                todoItems = todoItems.Where(t => t.Priority.ToLower() == priority.ToLower());
            }

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;

            return View(todoItems.ToList());
        }


        // GET: Todo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = db.TodoItems.Find(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // GET: Todo/Create
        public ActionResult Create()
        {
            return View(new TodoItem());
        }


        // POST: Todo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,DueDate,IsCompleted,Priority,CreatedAt")] TodoItem todoItem)
        {
            if (ModelState.IsValid)
            {

                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

                todoItem.UserId = user.Id;
                todoItem.CreatedAt = DateTime.Now;
                db.TodoItems.Add(todoItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(todoItem);
        }


        // GET: Todo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = db.TodoItems.Find(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", todoItem.UserId);
            return View(todoItem);
        }

        // POST: Todo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,DueDate,IsCompleted,Priority,CreatedAt")] TodoItem editedItem)
        {
            if (ModelState.IsValid)
            {
                var existingItem = db.TodoItems.Find(editedItem.Id);
                if (existingItem == null)
                {
                    return HttpNotFound();
                }

                editedItem.UserId = existingItem.UserId;

                db.Entry(existingItem).CurrentValues.SetValues(editedItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(editedItem);
        }


        // GET: Todo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TodoItem todoItem = db.TodoItems.Find(id);
            if (todoItem == null)
            {
                return HttpNotFound();
            }
            return View(todoItem);
        }

        // POST: Todo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TodoItem todoItem = db.TodoItems.Find(id);
            db.TodoItems.Remove(todoItem);
            db.SaveChanges();
            return RedirectToAction("Index");
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
