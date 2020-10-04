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
    public class AlbumsController : Controller
    {
        private MusicalDBEntities db = new MusicalDBEntities();

        // GET: Albums
        [Authorize(Roles = "Admin, User")]
        public ActionResult Index()
        {
            var albums = db.Albums.Include(a => a.Artist1);
            return View(albums.ToList());
        }

        // GET: Albums/Details/5
        [Authorize(Roles = "Admin, User")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            return PartialView(album);
        }

        // GET: Albums/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name");
            return View();
        }

        // POST: Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Album album)
        {
            if (ModelState.IsValid)
            {
                db.Albums.Add(album);
                Session["ID"] = album.Album_ID;
                db.SaveChanges();
                return RedirectToAction("UploadImages", "Albums");
            }

            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", album.Artist);
            return View(album);
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
            var alb = db.Albums.Find(Session["ID"]);
            alb.Image = uploadedFile;
            db.Entry(alb).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index");
        }


        // GET: Albums/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", album.Artist);
            Session["ID"] = album.Album_ID;
            return View(album);
        }

        // POST: Albums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Album album)
        {
            if (ModelState.IsValid)
            {
                db.Entry(album).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", album.Artist);
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Album album = db.Albums.Find(id);
            if (album == null)
            {
                return HttpNotFound();
            }
            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Album album = db.Albums.Find(id);
            db.Albums.Remove(album);
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
