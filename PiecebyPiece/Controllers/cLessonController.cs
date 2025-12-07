using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;

namespace PiecebyPiece.Controllers
{
    public class cLessonController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cLessonController(PiecebyPieceDBContext context)
        {
            _context = context;
        }




        // ------------------ Index ------------------
        public async Task<IActionResult> Index()
        {
            var piecebyPieceDBContext = _context.dLesson.Include(m => m.Course);
            return View(await piecebyPieceDBContext.ToListAsync());
        }



        // ------------------ Detail ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cLesson = await _context.dLesson
                .Include(m => m.Course)
                .Include(m => m.VideoEPs)
                .Include(m => m.Test)
                    .ThenInclude(m => m.Questions)
                .FirstOrDefaultAsync(m => m.lessonID == id);

            if (cLesson == null) return NotFound();

            return View(cLesson);
        }




        // ------------------ Create ------------------
        [HttpGet]
        public IActionResult Create(int id)
        {
            var course = _context.dCourse.FirstOrDefault(c => c.courseID == id);
            if (course == null) return NotFound();

            var cLesson = new mLESSON
            {
                courseID = id
            };

            ViewData["CourseName"] = course.courseName;

            return View(cLesson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("lessonID,courseID,lessonName,lessonDescription")] mLESSON cLesson)
        {
            if (ModelState.IsValid)
            {
                _context.dLesson.Add(cLesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = cLesson.lessonID });
            }

            if (cLesson.courseID == 0 || ViewData["courseID"] != null)
            {
                ViewData["courseID"] = new SelectList(_context.dCourse, "courseID", "courseName", cLesson.courseID);
            }
            else
            {
                var course = _context.dCourse.FirstOrDefault(c => c.courseID == cLesson.courseID);
                ViewData["CourseName"] = course?.courseName ?? "";
            }

            return View(cLesson);
        }




        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            var cLesson = await _context.dLesson
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.lessonID == id);
            if (cLesson == null)
            {
                return NotFound();
            }

            if (cLesson.Course != null)
            {
                ViewData["CourseName"] = cLesson.Course.courseName;
                ViewData["CourseID"] = cLesson.courseID;
            }

            return View(cLesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("lessonID,courseID,lessonName,lessonDescription,lessonProgress")] mLESSON cLesson)
        {
            if (id != cLesson.lessonID) return NotFound();


            if (ModelState.IsValid)
            {
                try
                {
                    var existingLesson = _context.dLesson.Local.FirstOrDefault(l => l.lessonID == cLesson.lessonID);

                    if (existingLesson != null)
                    {
                        _context.Entry(existingLesson).State = EntityState.Detached;
                    }
                    _context.Update(cLesson);
                    _context.Entry(cLesson).Reference(l => l.Course).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mLESSONExists(cLesson.lessonID)) return NotFound();

                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var currentCourse = await _context.dCourse.FirstOrDefaultAsync(c => c.courseID == cLesson.courseID);
            ViewData["CourseName"] = currentCourse?.courseName;
            ViewData["CourseID"] = cLesson.courseID;

            return View(cLesson);
        }



        // ------------------ Delete------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            return RedirectToAction("Details", "cCourse", new { id = id.Value });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.dLesson.FindAsync(id);
            if (lesson != null)
            {
                _context.dLesson.Remove(lesson);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }




        private bool mLESSONExists(int id)
        {
            return _context.dLesson.Any(e => e.lessonID == id);
        }
    }
}
