using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicForNow.Models;

namespace MusicForNow.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ArtistsController : Controller
    {
        private MusicalDBEntities db = new MusicalDBEntities();

        // GET: Artists
        [Authorize(Roles = "Admin, User")]
        public ActionResult Index()
        {
            return View(db.Artists.ToList());
        }

        // GET: Artists/Details/5
        [Authorize(Roles = "Admin, User")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }
            return PartialView(artist);
        }

        // GET: Artists/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Artists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Artist artist)
        {
            if (ModelState.IsValid)
            {
                db.Artists.Add(artist);
                Session["ID"] = artist.ID;
                db.SaveChanges();
                return RedirectToAction("UploadImages");
            }

            return View(artist);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadImages()
        {
            var model = new FileDataVM();
            return View(model);
        }

        [HttpPost]
        public ActionResult UploadImages(FileDataVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            byte[] uploadedFile = new byte[model.File.InputStream.Length];
            model.File.InputStream.Read(uploadedFile, 0, uploadedFile.Length);
            // now you could pass the byte array to your model and store wherever 
            // you intended to store it
            var art = db.Artists.Find(Session["ID"]);
            art.Image = uploadedFile;
            db.Entry(art).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index");
        }

        // GET: Artists/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }
            Session["ID"] = artist.ID;
            return View(artist);
        }

        // POST: Artists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Artist artist)
        {
            if (ModelState.IsValid)
            {
                db.Entry(artist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(artist);
        }

        // GET: Artists/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);
            if (artist == null)
            {
                return HttpNotFound();
            }
            return View(artist);
        }

        // POST: Artists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Artist artist = db.Artists.Find(id);
            db.Artists.Remove(artist);
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
