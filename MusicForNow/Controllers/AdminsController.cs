using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicForNow.Models;
using System.Security.Cryptography;
using System.Text;

namespace MusicForNow.Controllers
{
    public class AdminsController : Controller
    {
        private MusicalDBEntities db = new MusicalDBEntities();
        
        // GET: Admins/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        //Hashing methods ---------------------------------------------
        public static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomNumber = new byte[32];

                rng.GetBytes(randomNumber);

                return randomNumber;

            }
        }
        public static byte[] ComputeHMAC_SHA256(byte[] data, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(data);
            }
        }


        // GET: Admins/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Admin admin)
        {
            var salt = GenerateSalt();
            if (ModelState.IsValid)
            {
                admin.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(admin.Password), salt));
                admin.Salt = salt;
                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(admin);
        }

        //Change Password ------------------------------
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var admin_tbl = db.Admins.Find(id);
            if (admin_tbl == null)
            {
                return HttpNotFound();
            }
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordVM CPVM)
        {
            if (string.IsNullOrWhiteSpace(CPVM.OldPassword) || string.IsNullOrWhiteSpace(CPVM.NewPassword))
            {
                return HttpNotFound();
            }
            var admin = db.Admins.Where(m => m.Email == User.Identity.Name).FirstOrDefault();
            //check if admin is null
            if (admin == null)
            {
                return HttpNotFound();
            }
            //hashing old taken password
            var oldHashedPassword = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(CPVM.OldPassword), admin.Salt));
            //Check if oldpassowrd is the user password
            if (oldHashedPassword == admin.Password && !string.IsNullOrWhiteSpace(CPVM.NewPassword))
            {
                var NewHashedPassword = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(CPVM.NewPassword), admin.Salt));
                admin.Password = NewHashedPassword;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = admin.ID });
            }

            return View();
        }


        // GET: Admins/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = admin.ID });
            }
            return View(admin);
        }

        // GET: Admins/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Admin admin = db.Admins.Find(id);
            db.Admins.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
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
