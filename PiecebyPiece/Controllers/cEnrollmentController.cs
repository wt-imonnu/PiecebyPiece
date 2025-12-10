using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiecebyPiece.Controllers
{
    public class cEnrollmentController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cEnrollmentController(PiecebyPieceDBContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------
        public async Task<IActionResult> Index(string cSearch, List<string> cFilter, string sortOrder, string enrollStatusFilter)
        {
            var enrollmentQuery = _context.dEnrollment
                .Include(m => m.Course)
                .Include(m => m.User)
                .AsQueryable();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentStatusFilter = enrollStatusFilter;

            if (cFilter != null && cFilter.Any())
            {
                enrollmentQuery = enrollmentQuery.Where(e => cFilter.Contains(e.Course.courseSubject));
                ViewBag.SelectedSubjects = cFilter;
            }
            else
            {
                ViewBag.SelectedSubjects = new List<string>();
            }
            if (!string.IsNullOrEmpty(cSearch))
            {
                string search = cSearch.Trim().ToLower();
                enrollmentQuery = enrollmentQuery.Where(e =>
                    e.enrollID.ToString().Contains(search) ||
                    e.Course.courseID.ToString().Contains(search) ||
                    e.User.userID.ToString().Contains(search) ||
                    e.Course.courseName.ToLower().Contains(search) ||
                    e.User.userName.ToLower().Contains(search) ||
                    e.User.userSurname.ToLower().Contains(search) ||
                    (e.User.userName + " " + e.User.userSurname).ToLower().Contains(search)
                );
                ViewBag.CurrentSearch = cSearch;
            }
            else
            {
                ViewBag.CurrentSearch = "";
            }
            if (!string.IsNullOrEmpty(enrollStatusFilter) && enrollStatusFilter != "All")
            {
                enrollmentQuery = enrollmentQuery.Where(e => e.enrollStatus == enrollStatusFilter);
            }
            switch (sortOrder)
            {
                case "time_asc":
                    enrollmentQuery = enrollmentQuery.OrderBy(e => e.enrollTime);
                    break;
                case "time_desc":
                case null:
                    enrollmentQuery = enrollmentQuery.OrderByDescending(e => e.enrollTime);
                    break;
                default:
                    enrollmentQuery = enrollmentQuery.OrderByDescending(e => e.enrollID);
                    break;
            }

            var enrollments = await enrollmentQuery.ToListAsync();

            return View(enrollments);
        }




        // ------------------ Details ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mENROLLMENT = await _context.dEnrollment
                .Include(m => m.Course)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.enrollID == id);
            if (mENROLLMENT == null)
            {
                return NotFound();
            }

            return View(mENROLLMENT);
        }



        // ------------------ Create ------------------
        public IActionResult Create()
        {
            ViewData["courseID"] = new SelectList(_context.dCourse
                .Select(c => new {
                    c.courseID,
                    Display = $"#{c.courseID} | {c.courseName}"
                }), "courseID", "Display");

            ViewData["userID"] = new SelectList(_context.dUser
                .Select(u => new {
                    u.userID,
                    Display = $"#{u.userID} | {u.userName}"
                }), "userID", "Display");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("enrollID,enrollStatus,userID,courseID")] mENROLLMENT mENROLLMENT)
        {
            bool alreadyEnrolled = await _context.dEnrollment
                .AnyAsync(e => e.userID == mENROLLMENT.userID && e.courseID == mENROLLMENT.courseID);

            if (alreadyEnrolled)
            {
                ViewBag.EnrollError = "This user is already enrolled in the selected course.";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // 1. ใช้ courseID ที่ถูกส่งมา ดึงข้อมูล Course ทั้งหมด
                    var course = await _context.dCourse
                                            .FirstOrDefaultAsync(c => c.courseID == mENROLLMENT.courseID);

                    if (course != null)
                    {
                        // 2. เติม courseName ลงใน mENROLLMENT Model ก่อนบันทึก
                        // 💡 บรรทัดนี้คือการแก้ไขหลัก
                        mENROLLMENT.courseName = course.courseName;
                    }
                    // ถ้า course เป็น null (ไม่น่าจะเกิดขึ้น) จะไม่บันทึกค่า

                    mENROLLMENT.enrollTime = DateTime.Now;
                    _context.Add(mENROLLMENT);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Logic การสร้าง SelectList ซ้ำเมื่อเกิดข้อผิดพลาด
            ViewData["courseID"] = new SelectList(_context.dCourse
                .Select(c => new { c.courseID, Display = $"#{c.courseID} | {c.courseName}" }),
                "courseID", "Display", mENROLLMENT.courseID);

            ViewData["userID"] = new SelectList(_context.dUser
                .Select(u => new { u.userID, Display = $"#{u.userID} | {u.userName}" }),
                "userID", "Display", mENROLLMENT.userID);

            return View(mENROLLMENT);
        }



        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mENROLLMENT = await _context.dEnrollment.FindAsync(id);
            if (mENROLLMENT == null) return NotFound();


            ViewData["courseID"] = new SelectList(_context.dCourse
                .Select(c => new {
                    c.courseID,
                    Display = $"#{c.courseID} | {c.courseName}"
                }), "courseID", "Display", mENROLLMENT.courseID);

            ViewData["userID"] = new SelectList(_context.dUser
                .Select(u => new {
                    u.userID,
                    Display = $"#{u.userID} | {u.userName}"
                }), "userID", "Display", mENROLLMENT.userID);

            return View(mENROLLMENT);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("enrollID,enrollTime,enrollStatus,userID,courseID,courseName")] mENROLLMENT mENROLLMENT)
        {
            if (id != mENROLLMENT.enrollID) return NotFound();

            bool alreadyEnrolled = await _context.dEnrollment
                .AnyAsync(e => e.userID == mENROLLMENT.userID
                               && e.courseID == mENROLLMENT.courseID
                               && e.enrollID != mENROLLMENT.enrollID);

            if (alreadyEnrolled)
            {
                ViewBag.EnrollError = "This user is already enrolled in the selected course.";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(mENROLLMENT);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!mENROLLMENTExists(mENROLLMENT.enrollID)) return NotFound();
                        else throw;
                    }
                }
            }
            ViewData["courseID"] = new SelectList(_context.dCourse
                .Select(c => new { c.courseID, Display = $"#{c.courseID} | {c.courseName}" }),
                "courseID", "Display", mENROLLMENT.courseID);

            ViewData["userID"] = new SelectList(_context.dUser
                .Select(u => new { u.userID, Display = $"#{u.userID} | {u.userName}" }),
                "userID", "Display", mENROLLMENT.userID);

            return View(mENROLLMENT);
        }




        // ------------------ Delete ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mENROLLMENT = await _context.dEnrollment
                .Include(m => m.Course)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.enrollID == id);
            if (mENROLLMENT == null)
            {
                return NotFound();
            }

            return View(mENROLLMENT);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cEnrollment = await _context.dEnrollment.FindAsync(id);
            if (cEnrollment != null)
            {
                _context.dEnrollment.Remove(cEnrollment);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        private bool mENROLLMENTExists(int id)
        {
            return _context.dEnrollment.Any(e => e.enrollID == id);
        }
    }
}
