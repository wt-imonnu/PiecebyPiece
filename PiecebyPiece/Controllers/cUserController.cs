using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace PiecebyPiece.Controllers
{
    public class cUserController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cUserController(PiecebyPieceDBContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------
        public async Task<IActionResult> Index(string cSearch)
        {
            var userQuery = _context.dUser.AsQueryable();
            if (!string.IsNullOrEmpty(cSearch))
            {
                string search = cSearch.Trim().ToLower();

                userQuery = userQuery.Where(a =>

                    a.userID.ToString().Contains(search) ||
                    a.userName.ToLower().Contains(search) ||
                    a.userSurname.ToLower().Contains(search) ||
                    (a.userName + " " + a.userSurname).ToLower().Contains(search) ||
                    a.userEmail.ToLower().Contains(search)
                );

                ViewBag.CurrentSearch = cSearch;
            }
            else
            {
                ViewBag.CurrentSearch = "";
            }

            var cUser = await userQuery.ToListAsync();

            return View(cUser);
        }

        // ------------------ Login ------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string userEmail, string userPassword)
        {
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword))
            {
                ModelState.AddModelError("", "Please enter email and password");
                return View();
            }
            var admin = _context.dAdmin
                        .FirstOrDefault(a => a.adminEmail == userEmail
                                          && a.adminPassword == userPassword);

            if (admin != null)
            {
                HttpContext.Session.SetString("adminName", admin.adminName);
                HttpContext.Session.SetInt32("adminID", admin.adminID);
                HttpContext.Session.SetString("adminEmail", admin.adminEmail);
                return RedirectToAction("Index", "cEnrollment");
            }

            var user = _context.dUser
                        .FirstOrDefault(u => u.userEmail == userEmail
                                          && u.userPassword == userPassword);

            if (user != null)
            {
                HttpContext.Session.SetString("userName", user.userName);
                HttpContext.Session.SetInt32("userID", user.userID);

                bool hasEnrollment = _context.dEnrollment.Any(e => e.userID == user.userID);

                if (hasEnrollment)
                {
                    return RedirectToAction("MyCourse", "cUserPersp");
                }
                else
                {
                    return RedirectToAction("CourseLibrary", "cUserPersp");
                }
            }


            ViewBag.Error = "Incorrect email or password. Please try again.";
            return View();
        }




        // ------------------ Logout ------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Home", "Home");
        }




        // ------------------ Detail ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cUser = await _context.dUser
                .FirstOrDefaultAsync(m => m.userID == id);

            if (cUser == null) return NotFound();

            return View(cUser);
        }





        // ------------------ Create ------------------
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(mUSER cUser)
        {
            // ✔ บังคับรูปเฉพาะตอน Create
            if (cUser.userPhoto == null || cUser.userPhoto.Length == 0)
            {
                ModelState.AddModelError("userPhoto", "Please upload your profile image.");
            }

            if (ModelState.IsValid)
            {
                bool userEmailExists = await _context.dUser.AnyAsync(u => u.userEmail == cUser.userEmail);
                bool adminEmailExists = await _context.dAdmin.AnyAsync(a => a.adminEmail == cUser.userEmail);

                if (userEmailExists || adminEmailExists)
                {
                    ModelState.AddModelError("userEmail", "This email is already in use. Please use a different email address");
                    return View(cUser);
                }
                if (cUser.userPhoto != null && cUser.userPhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cUser.userPhoto.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await cUser.userPhoto.CopyToAsync(stream);
                    }

                    cUser.userPhotoPath = "/uploads/" + uniqueFileName;
                }

                _context.Add(cUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(cUser);
        }





        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cUser = await _context.dUser.FindAsync(id);
            if (cUser == null) return NotFound();

            return View(cUser);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, mUSER cUser, IFormFile? userPhoto)
        {
            if (id != cUser.userID) return NotFound();

            var existingUser = await _context.dUser.AsNoTracking()
                .FirstOrDefaultAsync(x => x.userID == id);

            if (existingUser == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (userPhoto != null && userPhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(userPhoto.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await userPhoto.CopyToAsync(stream);
                    }

                    cUser.userPhotoPath = "/uploads/" + uniqueFileName;
                }
                else
                {
                    cUser.userPhotoPath = existingUser.userPhotoPath;
                }

                _context.Update(cUser);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(cUser);
        }




        // ------------------ Delete ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cUser = await _context.dUser
                .FirstOrDefaultAsync(m => m.userID == id);
            if (cUser == null)
            {
                return NotFound();
            }
            return View(cUser);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cUser = await _context.dUser.FindAsync(id);

            if (cUser != null)
            {

                _context.dUser.Remove(cUser);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        private bool UserExists(int id)
        {
            return _context.dUser.Any(e => e.userID == id);
        }
    }
}
