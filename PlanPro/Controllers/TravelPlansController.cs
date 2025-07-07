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
    public class TravelPlansController : Controller
    {
        private Entities db = new Entities();

        // GET: TravelPlans
        public ActionResult Index()
        {
            var travelPlans = db.TravelPlans.Include(t => t.User);
            return View(travelPlans.ToList());
        }

        // GET: TravelPlans/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TravelPlan travelPlan = db.TravelPlans.Find(id);
            if (travelPlan == null)
            {
                return HttpNotFound();
            }
            return View(travelPlan);
        }

        // GET: TravelPlans/Create
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Destination,StartDate,EndDate,Notes")] TravelPlan travelPlan)
        {
            if (ModelState.IsValid)
            {

                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                }

                travelPlan.UserId = user.Id;
                db.TravelPlans.Add(travelPlan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(travelPlan);
        }


        // GET: TravelPlans/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TravelPlan travelPlan = db.TravelPlans.Find(id);
            if (travelPlan == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", travelPlan.UserId);
            return View(travelPlan);
        }

        // POST: TravelPlans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,Destination,StartDate,EndDate,Notes")] TravelPlan travelPlan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(travelPlan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "FullName", travelPlan.UserId);
            return View(travelPlan);
        }

        // GET: TravelPlans/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TravelPlan travelPlan = db.TravelPlans.Find(id);
            if (travelPlan == null)
            {
                return HttpNotFound();
            }
            return View(travelPlan);
        }

        // POST: TravelPlans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TravelPlan travelPlan = db.TravelPlans.Find(id);
            db.TravelPlans.Remove(travelPlan);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: TravelPlans/Itinerary/5
        public ActionResult Itinerary(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itineraries = db.Itineraries
                .Include(i => i.DayPlans)
                .Where(i => i.TravelPlanId == id)
                .ToList();

            ViewBag.TravelPlanId = id;
            return View(itineraries);
        }

        // GET: TravelPlans/AddItinerary
        public ActionResult AddItinerary(int? travelPlanId)
        {
            if (travelPlanId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itinerary = new Itinerary
            {
                TravelPlanId = travelPlanId.Value
            };

            return View(itinerary);
        }

        // POST: TravelPlans/AddItinerary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItinerary([Bind(Include = "TravelPlanId,Title,Description")] Itinerary itinerary)
        {
            if (ModelState.IsValid)
            {
                db.Itineraries.Add(itinerary);
                db.SaveChanges();
                return RedirectToAction("Itinerary", new { id = itinerary.TravelPlanId });
            }
            ViewBag.TravelPlanId = itinerary.TravelPlanId;
            return View(itinerary);
        }

        // GET: TravelPlans/AddDayPlan/5
        public ActionResult AddDayPlan(int? itineraryId)
        {
            if (itineraryId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itinerary = db.Itineraries.Find(itineraryId);
            if (itinerary == null)
                return HttpNotFound();

            var travelPlan = db.TravelPlans.Find(itinerary.TravelPlanId);
            if (travelPlan == null)
                return HttpNotFound();

            ViewBag.ItineraryId = itineraryId;
            ViewBag.TravelPlanId = itinerary.TravelPlanId;
            ViewBag.StartDate = travelPlan.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = travelPlan.EndDate.ToString("yyyy-MM-dd");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDayPlan([Bind(Include = "ItineraryId,Date,Activities")] DayPlan dayPlan)
        {
            var itinerary = db.Itineraries.Find(dayPlan.ItineraryId);
            if (itinerary == null)
                return HttpNotFound();

            var travelPlan = db.TravelPlans.Find(itinerary.TravelPlanId);
            if (travelPlan == null)
                return HttpNotFound();

            if (dayPlan.Date < travelPlan.StartDate || dayPlan.Date > travelPlan.EndDate)
            {
                ModelState.AddModelError("Date", "Date must be between the Travel Plan's Start and End Dates.");
            }

            if (ModelState.IsValid)
            {
                db.DayPlans.Add(dayPlan);
                db.SaveChanges();
                return RedirectToAction("Itinerary", new { id = itinerary.TravelPlanId });
            }

            ViewBag.ItineraryId = dayPlan.ItineraryId;
            ViewBag.TravelPlanId = itinerary.TravelPlanId;
            ViewBag.StartDate = travelPlan.StartDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = travelPlan.EndDate.ToString("yyyy-MM-dd");

            return View(dayPlan);
        }


        // GET: TravelPlans/EditItinerary/5
        public ActionResult EditItinerary(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itinerary = db.Itineraries.Find(id);
            if (itinerary == null)
                return HttpNotFound();

            return PartialView("_EditItineraryPartial", itinerary);
        }

        // POST: TravelPlans/EditItinerary/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItinerary([Bind(Include = "Id,TravelPlanId,Title,Description")] Itinerary itinerary)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itinerary).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Itinerary", new { id = itinerary.TravelPlanId });
            }
            return View(itinerary);
        }

        // GET: TravelPlans/EditDayPlan/5
        public ActionResult EditDayPlan(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var dayPlan = db.DayPlans.Find(id);
            if (dayPlan == null)
                return HttpNotFound();

            return PartialView("_EditDayPlanPartial", dayPlan);
        }

        // POST: TravelPlans/EditDayPlan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDayPlan([Bind(Include = "Id,ItineraryId,Date,Activities")] DayPlan dayPlan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dayPlan).State = EntityState.Modified;
                db.SaveChanges();

                var itinerary = db.Itineraries.Find(dayPlan.ItineraryId);
                return RedirectToAction("Itinerary", new { id = itinerary.TravelPlanId });
            }
            return View(dayPlan);
        }

        // POST: TravelPlans/DeleteItinerary/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteItinerary(int id)
        {
            var itinerary = db.Itineraries.Find(id);
            if (itinerary != null)
            {
                int travelPlanId = itinerary.TravelPlanId;
                db.Itineraries.Remove(itinerary);
                db.SaveChanges();
                return RedirectToAction("Itinerary", new { id = travelPlanId });
            }
            return HttpNotFound();
        }

        // POST: TravelPlans/DeleteDayPlan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDayPlan(int id)
        {
            var dayPlan = db.DayPlans.Find(id);
            if (dayPlan != null)
            {
                var itinerary = db.Itineraries.Find(dayPlan.ItineraryId);
                db.DayPlans.Remove(dayPlan);
                db.SaveChanges();
                return RedirectToAction("Itinerary", new { id = itinerary.TravelPlanId });
            }
            return HttpNotFound();
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