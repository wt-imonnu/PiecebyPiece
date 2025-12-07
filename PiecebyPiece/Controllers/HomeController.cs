using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace PiecebyPiece.Controllers
{
    public class HomeController : Controller
    {
        private readonly PiecebyPieceDBContext _context;
        public HomeController(PiecebyPieceDBContext context)
        {
            _context = context;
        }


        //---------- Home ----------
        public async Task<IActionResult> Home()
        {
            var courses = await _context.dCourse.ToListAsync();
            return View(courses);
        }



        //---------- SingUp ----------
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(mUSER cUser, IFormFile userPhoto)
        {
            if (cUser.userPhoto == null || cUser.userPhoto.Length == 0)
            {
                ModelState.AddModelError("userPhoto", "Please upload your profile image.");
            }
            if (ModelState.IsValid)
            {
                if (cUser.userPhoto != null && cUser.userPhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cUser.userPhoto.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        cUser.userPhoto.CopyTo(stream);
                    }

                    cUser.userPhotoPath = "/uploads/" + uniqueFileName;
                }

                _context.dUser.Add(cUser);
                _context.SaveChanges();

                // Login user immediately
                HttpContext.Session.SetInt32("userID", cUser.userID);
                HttpContext.Session.SetString("userName", cUser.userName);

                if (!string.IsNullOrEmpty(cUser.userPhotoPath))
                {
                    HttpContext.Session.SetString("userPhotoPath", cUser.userPhotoPath);
                }

                return RedirectToAction("Success", new { id = cUser.userID });
            }
            return View(cUser);
        }

        public IActionResult Success(int id)
        {
            var cUser = _context.dUser.FirstOrDefault(u => u.userID == id);
            return View(cUser);
        }


        //---------- About Us ----------
        public IActionResult AboutUs()
        {
            return View();
        }


        //---------- Contact Us ----------
        public IActionResult ContactUs()
        {
            return View();
        }



    }
}
