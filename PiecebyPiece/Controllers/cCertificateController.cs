using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PiecebyPiece.Controllers
{
    public class cCertificateController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cCertificateController(PiecebyPieceDBContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string cSearch, List<string> cFilter)
        {
            var certificates = _context.dCertificate
                .Include(cert => cert.Enrollment)
                    .ThenInclude(enroll => enroll.Course) // ดึง Course
                .Include(cert => cert.Enrollment)
                    .ThenInclude(enroll => enroll.User) // ดึง User
                .AsQueryable();

            if (!string.IsNullOrEmpty(cSearch))
            {
                string searchLower = cSearch.ToLower();

                // 💡 เราจะใช้การค้นหาผ่าน Navigation Property แทนฟิลด์ที่ถูกลบออกไป
                certificates = certificates.Where(cert =>
                    cert.Enrollment.Course.courseName.ToLower().Contains(searchLower) || // ค้นหาจาก Course
                    cert.Enrollment.User.userName.ToLower().Contains(searchLower) ||     // ค้นหาจาก User
                    cert.Enrollment.User.userSurname.ToLower().Contains(searchLower)
                );

                if (int.TryParse(cSearch, out int searchId))
                {
                    // รวมตรรกะการค้นหาด้วย ID เข้าไป
                    certificates = certificates.Where(cert =>
                        cert.cerID == searchId ||
                        cert.enrollID == searchId ||
                        cert.Enrollment.userID == searchId
                    );
                }
            }

            if (cFilter != null && cFilter.Count > 0)
            {
                // การกรองด้วย Course Subject (ยังคงถูกต้อง)
                certificates = certificates.Where(cert => cFilter.Contains(cert.Enrollment.Course.courseSubject));
            }

            ViewBag.SelectedSubjects = cFilter ?? new List<string>();

            return View(await certificates.ToListAsync());
        }



        // ------------------ Details ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCERTIFICATE = await _context.dCertificate
                .Include(m => m.Enrollment)
                .FirstOrDefaultAsync(m => m.cerID == id);
            if (mCERTIFICATE == null)
            {
                return NotFound();
            }

            return View(mCERTIFICATE);
        }



        // ------------------ Create ------------------
        public IActionResult Create()
        {
            ViewData["enrollID"] = new SelectList(_context.dEnrollment, "enrollID", "enrollID");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEnrollmentData(int enrollId)
        {
            bool certificateExists = await _context.dCertificate
                .AnyAsync(c => c.enrollID == enrollId);

            if (certificateExists)
            {
                return Json(new
                {
                    error = $"Enrollment ID: #{enrollId} already has a certificate issued. Please edit or delete the existing certificate, or choose a different enrollment."
                });
            }

            var enrollment = await _context.dEnrollment
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.enrollID == enrollId);
            if (enrollment == null)
            {
                return NotFound();
            }

            var data = new
            {

                courseID = enrollment.courseID,
                courseName = enrollment.courseName,
                userName = enrollment.User?.userName,
                userSurname = enrollment.User?.userSurname
            };
            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // 💡 เราไม่จำเป็นต้อง Bind courseName, userName, userSurname เพราะเราจะดึงมาเติมเอง
        // แต่ต้องแน่ใจว่าฟอร์มส่ง enrollID มา
        public async Task<IActionResult> Create([Bind("cerID,cerDate,enrollID")] mCERTIFICATE mCERTIFICATE)
        {
            bool certificateExists = await _context.dCertificate.AnyAsync(c => c.enrollID == mCERTIFICATE.enrollID);

            if (certificateExists)
            {
                ModelState.AddModelError(string.Empty,
                $"Enrollment ID: #{mCERTIFICATE.enrollID} already has a certificate issued. Please edit or delete the existing certificate, or  choose a different enrollment.");
            }

            if (ModelState.IsValid && !certificateExists)
            {
                // 1. ดึงข้อมูล Enrollment ที่เกี่ยวข้อง
                var enrollment = await _context.dEnrollment
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.enrollID == mCERTIFICATE.enrollID);

                if (enrollment == null)
                {
                    ModelState.AddModelError("enrollID", "Enrollment ID not found.");
                }
                else
                {
                    // 2. เติมข้อมูลที่ขาดหายไปลงใน mCERTIFICATE ก่อนบันทึก

                    // เติม userID (สมมติว่า mCERTIFICATE มี property ชื่อ userID)
                    // mCERTIFICATE.userID = enrollment.userID; 

                    // เติม courseName และ userName/userSurname (ถ้าไม่ได้มาจากฟอร์ม)
                    mCERTIFICATE.courseName = enrollment.courseName;
                    mCERTIFICATE.userName = enrollment.User?.userName;
                    mCERTIFICATE.userSurname = enrollment.User?.userSurname;

                    // 3. บันทึก
                    _context.Add(mCERTIFICATE);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // ถ้ามี Error (ModelState ไม่ Valid หรือ Enrollment ID ไม่ถูกต้อง)
            ViewData["enrollID"] = new SelectList(_context.dEnrollment, "enrollID", "enrollID", mCERTIFICATE.enrollID);
            return View(mCERTIFICATE);
        }





        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCERTIFICATE = await _context.dCertificate.FindAsync(id);
            if (mCERTIFICATE == null)
            {
                return NotFound();
            }
            ViewData["enrollID"] = new SelectList(_context.dEnrollment, "enrollID", "enrollID", mCERTIFICATE.enrollID);
            return View(mCERTIFICATE);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("cerID,cerGrade,cerDate,enrollID,courseName,userName,userSurname")] mCERTIFICATE mCERTIFICATE)
        {
            if (id != mCERTIFICATE.cerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mCERTIFICATE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["enrollID"] = new SelectList(_context.dEnrollment, "enrollID", "enrollID", mCERTIFICATE.enrollID);
            return View(mCERTIFICATE);
        }



        // ------------------ Delete ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            var certificate = await _context.dCertificate.FindAsync(id);
            if (certificate != null)
            {
                _context.dCertificate.Remove(certificate);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }




        //----------------Certificate Gennerator----------------
        public async Task<IActionResult> Certificate(int enrollID)
        {
   
            int? userID = HttpContext.Session.GetInt32("userID");
            if (userID == null)
            {
  
                return RedirectToAction("Login", "cUser");
            }

          
            var cert = await _context.dCertificate
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.User)
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(c =>
                    c.enrollID == enrollID &&
                    c.Enrollment.userID == userID
                );

            if (cert == null)
            {
 
                return NotFound();
            }

            return View(cert);
        }


        //----------------My Certificate----------------
        public async Task<IActionResult> MyCertificate()
        {
            var cPersp = GetLoggedInUserFromSession();

            int? currentUserID = HttpContext.Session.GetInt32("userID");
            if (!currentUserID.HasValue)
            {
                return RedirectToAction("Login", "cUser");
            }

            var user = await _context.dUser.FirstOrDefaultAsync(u => u.userID == currentUserID.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            var userCertificates = await _context.dCertificate
                                    .Include(c => c.Enrollment)
                                    .ThenInclude(e => e.Course)
                                    .Where(c => c.Enrollment != null && c.Enrollment.userID == user.userID)
                                    .OrderByDescending(c => c.cerDate)
                                    .ToListAsync();
            return View(userCertificates);

        }


        //----------------- Navbar -------------------
        private mUSER? GetLoggedInUserFromSession()
        {
            var userID = HttpContext.Session.GetInt32("userID");
            if (userID.HasValue)
            {
                var user = _context.dUser.FirstOrDefault(u => u.userID == userID.Value);
                return user;
            }
            return null;
        }
    }
}
