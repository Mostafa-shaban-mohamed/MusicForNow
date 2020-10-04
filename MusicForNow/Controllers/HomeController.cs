using MusicForNow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MusicForNow.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult LogIn()
        {
            return PartialView();
        }

        //Hashing methods ---------------------------------------------
        public static byte[] ComputeHMAC_SHA256(byte[] data, byte[] salt)
        {
            using (var hmac = new HMACSHA256(salt))
            {
                return hmac.ComputeHash(data);
            }
        }

        [HttpPost]
        public ActionResult LogIn(UserModel model)
        {
            using (MusicalDBEntities db = new MusicalDBEntities())
            {
                Admin Ad = db.Admins.FirstOrDefault(st => st.Email == model.Email);
                User Us = db.Users.FirstOrDefault(st => st.Email == model.Email);

                //Confirmation booleans
                bool IsValidAdmin = false; bool IsValidUser = false;

                if (Ad != null)
                {
                    var pass_std = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(model.Password), Ad.Salt));
                    IsValidAdmin = db.Admins.Any(user => user.Email.ToLower() == model.Email.ToLower() && user.Password == pass_std);
                }
                else if (Us != null)
                {
                    var pass_lec = Convert.ToBase64String(ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(model.Password), Us.Salt));
                    IsValidUser = db.Users.Any(user => user.Email.ToLower() == model.Email.ToLower() && user.Password == pass_lec);
                }

                if (IsValidAdmin)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    Session["UserID"] = Ad.ID;
                    //Go to profile page
                    return RedirectToAction("Index", "Home");
                }

                if (IsValidUser)
                {
                    FormsAuthentication.SetAuthCookie(model.Email, false);
                    Session["UserID"] = Us.ID;
                    //Go to profile page
                    return RedirectToAction("Index", "Home");
                }
                
                ModelState.AddModelError("", "invalid Username or Password");
                return View("LogIn");
            }
        }
    }
}