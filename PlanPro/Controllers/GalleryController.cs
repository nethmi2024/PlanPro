using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PlanPro.Models;

namespace PlanPro.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folders = db.GalleryFolders
                            .Include(f => f.GalleryImages)
                            .Where(f => f.UserId == user.Id)
                            .ToList();

            return View(folders);
        }

        // GET: Gallery/CreateFolder
        public ActionResult CreateFolder()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFolder([Bind(Include = "FolderName")] GalleryFolder folder)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user == null)
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

                bool folderExists = db.GalleryFolders.Any(f => f.UserId == user.Id && f.FolderName == folder.FolderName);

                if (folderExists)
                {
                    TempData["FolderExists"] = true;
                    return View(folder); 
                }

                folder.UserId = user.Id;
                db.GalleryFolders.Add(folder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(folder);
        }


        // GET: Gallery/EditFolder/5
        public ActionResult EditFolder(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == id && f.UserId == user.Id);
            if (folder == null)
                return HttpNotFound();

            return View(folder);
        }

        // POST: Gallery/EditFolder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFolder([Bind(Include = "Id,FolderName")] GalleryFolder editedFolder)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user == null)
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

                var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == editedFolder.Id && f.UserId == user.Id);
                if (folder == null)
                    return HttpNotFound();

                folder.FolderName = editedFolder.FolderName;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(editedFolder);
        }

        // GET: Gallery/DeleteFolder/5
        public ActionResult DeleteFolder(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folder = db.GalleryFolders.Include(f => f.GalleryImages).FirstOrDefault(f => f.Id == id && f.UserId == user.Id);
            if (folder == null)
                return HttpNotFound();

            return View(folder);
        }

        // POST: Gallery/DeleteFolder/5
        [HttpPost, ActionName("DeleteFolder")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFolderConfirmed(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folder = db.GalleryFolders.Include(f => f.GalleryImages).FirstOrDefault(f => f.Id == id && f.UserId == user.Id);
            if (folder == null)
                return HttpNotFound();

            db.GalleryImages.RemoveRange(folder.GalleryImages);
            db.GalleryFolders.Remove(folder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Gallery/Folder/5
        public ActionResult Folder(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folder = db.GalleryFolders.Include(f => f.GalleryImages)
                                         .FirstOrDefault(f => f.Id == id && f.UserId == user.Id);

            if (folder == null)
                return HttpNotFound();

            return View(folder);
        }

        // GET: Gallery/CreateImage/5
        public ActionResult CreateImage(int? folderId)
        {
            if (folderId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == folderId && f.UserId == user.Id);
            if (folder == null)
                return HttpNotFound();

            var model = new GalleryImage { FolderId = folder.Id };
            return View(model);
        }

        // POST: Gallery/CreateImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateImage(GalleryImage image, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user == null)
                    return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

                var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == image.FolderId && f.UserId == user.Id);
                if (folder == null)
                    return HttpNotFound();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (upload != null && upload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(upload.FileName);
                    var extension = Path.GetExtension(fileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Only image files (JPG, JPEG, PNG, GIF) are allowed.");
                        return View(image);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please select an image file to upload.");
                }

                if (upload != null && upload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(upload.FileName);
                    var extension = Path.GetExtension(fileName);
                    var newFileName = Guid.NewGuid().ToString() + extension;
                    var path = Path.Combine(Server.MapPath("~/Uploads/GalleryImages"), newFileName);

                    Directory.CreateDirectory(Server.MapPath("~/Uploads/GalleryImages"));
                    upload.SaveAs(path);

                    image.ImagePath = "/Uploads/GalleryImages/" + newFileName;
                    image.ImageTitle = image.ImageTitle ?? fileName;

                    db.GalleryImages.Add(image);
                    db.SaveChanges();
                    return RedirectToAction("Folder", new { id = image.FolderId });
                }

                ModelState.AddModelError("", "Please select an image file to upload.");
            }

            return View(image);
        }

        // GET: Gallery/DeleteImage/5
        public ActionResult DeleteImage(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var image = db.GalleryImages.Include(i => i.GalleryFolder)
                                        .FirstOrDefault(i => i.Id == id && i.GalleryFolder.UserId == user.Id);

            if (image == null)
                return HttpNotFound();

            return View(image);
        }

        // POST: Gallery/DeleteImage/5
        [HttpPost, ActionName("DeleteImage")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImageConfirmed(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var image = db.GalleryImages.Include(i => i.GalleryFolder)
                                        .FirstOrDefault(i => i.Id == id && i.GalleryFolder.UserId == user.Id);
            if (image == null)
                return HttpNotFound();

            if (!string.IsNullOrEmpty(image.ImagePath))
            {
                var fullPath = Server.MapPath(image.ImagePath);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            int folderId = image.FolderId;
            db.GalleryImages.Remove(image);
            db.SaveChanges();
            return RedirectToAction("Folder", new { id = folderId });
        }

        public JsonResult GetFolderDetails(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == id && f.UserId == user.Id);

            if (folder == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new { folder.Id, folder.FolderName }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditFolderAjax(int Id, string FolderName)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            var folder = db.GalleryFolders.FirstOrDefault(f => f.Id == Id && f.UserId == user.Id);

            if (folder == null)
                return Json(new { success = false, message = "Folder not found." });

            bool exists = db.GalleryFolders.Any(f => f.UserId == user.Id && f.FolderName == FolderName && f.Id != Id);
            if (exists)
                return Json(new { success = false, message = "A folder with this name already exists." });

            folder.FolderName = FolderName;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteFolderAjax(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Unauthorized access." });

            var folder = db.GalleryFolders.Include(f => f.GalleryImages)
                                          .FirstOrDefault(f => f.Id == id && f.UserId == user.Id);

            if (folder == null)
                return Json(new { success = false, message = "Folder not found." });

            db.GalleryImages.RemoveRange(folder.GalleryImages);
            db.GalleryFolders.Remove(folder);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteImageAjax(int id)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Unauthorized access." });

            var image = db.GalleryImages.Include(i => i.GalleryFolder)
                                        .FirstOrDefault(i => i.Id == id && i.GalleryFolder.UserId == user.Id);

            if (image == null)
                return Json(new { success = false, message = "Image not found." });

            if (!string.IsNullOrEmpty(image.ImagePath))
            {
                var fullPath = Server.MapPath(image.ImagePath);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            db.GalleryImages.Remove(image);
            db.SaveChanges();

            return Json(new { success = true });
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
