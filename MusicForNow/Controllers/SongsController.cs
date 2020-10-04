using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using PagedList;
using System.Web.Mvc;
using MusicForNow.Models;
using System.IO;

namespace MusicForNow.Controllers
{
    public class SongsController : Controller
    {
        private MusicalDBEntities db = new MusicalDBEntities();

        // GET: Songs
        [Authorize(Roles = "Admin, User")]
        public ActionResult Index(string SingerID, string AlbumID, string SongName, int? Page_No)
        {
            int Size_Of_Page = 4;
            int No_Of_Page = (Page_No ?? 1);
            var songs = db.Songs.Include(s => s.Album1).Include(s => s.Artist1);
            //Dropdownlist for search artist and album
            ViewBag.SingerID = new SelectList(db.Artists, "ID", "Name");
            ViewBag.AlbumID = new SelectList(db.Albums, "Album_ID", "Album_Title");
            //search for song name
            if (!string.IsNullOrWhiteSpace(SongName))
            {
                songs = songs.Where(m => m.Name.Contains(SongName));
            }
            if (!string.IsNullOrWhiteSpace(SingerID))
            {
                songs = songs.Where(m => m.Artist == SingerID);
            }
            if (!string.IsNullOrWhiteSpace(AlbumID))
            {
                songs = songs.Where(m => m.Album == AlbumID);
            }

            return View(songs.ToList().ToPagedList(No_Of_Page, Size_Of_Page));
        }

        // GET: Songs/Details/5
        [Authorize(Roles = "Admin, User")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            song.Image = db.Albums.Find(song.Album).Image;
            return PartialView(song);
        }


        //For Playing Music
        public ActionResult Audio(string id)
        {
            byte[] song = db.Songs.Find(id).Song_File;
            // Add code to get data
            long fSize = song.Length;
            long startbyte = 0;
            long endbyte = fSize - 1;
            int statusCode = 200;
            if ((Request.Headers["Range"] != null))
            {
                //Get the actual byte range from the range header string, and set the starting byte.
                string[] range = Request.Headers["Range"].Split(new char[] { '=', '-' });
                startbyte = Convert.ToInt64(range[1]);
                if (range.Length > 2 && range[2] != "") endbyte = Convert.ToInt64(range[2]);
                //If the start byte is not equal to zero, that means the user is requesting partial content.
                if (startbyte != 0 || endbyte != fSize - 1 || range.Length > 2 && range[2] == "")
                { statusCode = 206; }//Set the status code of the response to 206 (Partial Content) and add a content range header.                                    
            }
            long desSize = endbyte - startbyte + 1;
            //Headers
            Response.StatusCode = statusCode;

            Response.ContentType = "audio/mp3";
            Response.AddHeader("Content-Accept", Response.ContentType);
            Response.AddHeader("Content-Length", desSize.ToString());
            Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
            //Data

            var stream = new MemoryStream(song, (int)startbyte, (int)desSize);

            return new FileStreamResult(stream, Response.ContentType);
        }

        // GET: Songs/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Album = new SelectList(db.Albums, "Album_ID", "Album_Title");
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name");
            return View();
        }

        // POST: Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Song song)
        {
            if (ModelState.IsValid)
            {
                song.Image = db.Albums.Find(song.Album).Image;
                db.Songs.Add(song);
                db.SaveChanges();
                Session["ID"] = song.Song_ID;
                return RedirectToAction("UploadSongs");
            }

            ViewBag.Album = new SelectList(db.Albums, "Album_ID", "Album_Title", song.Album);
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", song.Artist);
            return View(song);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadSongs()
        {
            var model = new FileDataVM();
            return View(model);
        }

        [HttpPost]
        public ActionResult UploadSongs(FileDataVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            byte[] uploadedFile = new byte[model.File.InputStream.Length];
            model.File.InputStream.Read(uploadedFile, 0, uploadedFile.Length);

            // now you could pass the byte array to your model and store wherever 
            // you intended to store it
            var song = db.Songs.Find(Session["ID"]);
            if (song == null)
            {
                return HttpNotFound();
            }
            song.Song_File = uploadedFile;
            db.Entry(song).State = EntityState.Modified;
            db.SaveChanges();


            return RedirectToAction("Index");
        }

        // GET: Songs/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            ViewBag.Album = new SelectList(db.Albums, "Album_ID", "Album_Title", song.Album);
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", song.Artist);
            return View(song);
        }

        // POST: Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Song song)
        {
            if (ModelState.IsValid)
            {
                db.Entry(song).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Album = new SelectList(db.Albums, "Album_ID", "Album_Title", song.Album);
            ViewBag.Artist = new SelectList(db.Artists, "ID", "Name", song.Artist);
            return View(song);
        }

        // GET: Songs/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Song song = db.Songs.Find(id);
            if (song == null)
            {
                return HttpNotFound();
            }
            return View(song);
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Song song = db.Songs.Find(id);
            db.Songs.Remove(song);
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
