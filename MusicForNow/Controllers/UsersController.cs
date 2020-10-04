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
    public class UsersController : Controller
    {
        private MusicalDBEntities db = new MusicalDBEntities();

        // GET: Users/Details/5
        [Authorize(Roles = "User")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
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

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            var salt = GenerateSalt();
            if (ModelState.IsValid)
            {
                user.Password = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(user.Password), salt));
                user.Salt = salt;
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        //Change Password ------------------------------
        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.Users.Find(id);
            if (user == null)
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
            var user = db.Users.Where(m => m.Email == User.Identity.Name).FirstOrDefault();
            //check if admin is null
            if (user == null)
            {
                return HttpNotFound();
            }
            //hashing old taken password
            var oldHashedPassword = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(CPVM.OldPassword), user.Salt));
            //Check if oldpassowrd is the user password
            if (oldHashedPassword == user.Password && !string.IsNullOrWhiteSpace(CPVM.NewPassword))
            {
                var NewHashedPassword = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(CPVM.NewPassword), user.Salt));
                user.Password = NewHashedPassword;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = user.ID });
            }

            return View();
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "User")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = user.ID });
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "User")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
