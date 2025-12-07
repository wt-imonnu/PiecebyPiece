using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PiecebyPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace PiecebyPiece.Controllers
{
    public class cCourseController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cCourseController(PiecebyPieceDBContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------ (++ search ++filter)
        public async Task<IActionResult> Index(string cSearch, List<string> cFilter)
        {
            var ccCourse = _context.dCourse
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.VideoEPs)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cSearch))
            {
                ccCourse = ccCourse.Where(c =>
                    c.courseName.Contains(cSearch) ||
                    c.courseSubject.Contains(cSearch) ||
                    c.courseDescription.Contains(cSearch) ||

                    // lesson level
                    c.Lessons.Any(l =>
                        l.lessonName.Contains(cSearch) ||
                        l.lessonDescription.Contains(cSearch) ||

                        // videoEP level
                        l.VideoEPs.Any(v => v.vdoName.Contains(cSearch))
                    )
                );
            }

            if (cFilter != null && cFilter.Count > 0)
            {
                ccCourse = ccCourse.Where(c => cFilter.Contains(c.courseSubject));
            }

            ViewBag.SelectedSubjects = cFilter ?? new List<string>();

            return View(await ccCourse.ToListAsync());
        }




        // ------------------ Detail ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cCourse = await _context.dCourse
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.courseID == id);

            if (cCourse == null) return NotFound();

            return View(cCourse);
        }



        // ------------------ Create ------------------
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(mCOURSE cCourse)
        {
            if (cCourse.IsCreate && cCourse.coursePhoto == null)
            {
                ModelState.AddModelError("coursePhoto", "Please upload thumbnail");
            }

            if (ModelState.IsValid)
            {
                if (cCourse.coursePhoto != null && cCourse.coursePhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cCourse.coursePhoto.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await cCourse.coursePhoto.CopyToAsync(stream);
                    }

                    cCourse.coursePhotoPath = "/uploads/" + uniqueFileName;
                }

                _context.Add(cCourse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cCourse);
        }



        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cCourse = await _context.dCourse
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.courseID == id);

            if (cCourse == null) return NotFound();

            return View(cCourse);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("courseID,courseName,courseSubject,courseDescription,coursePhotoPath")] mCOURSE cCourse, IFormFile? coursePhoto)
        {
            if (id != cCourse.courseID) return NotFound();
            var existingCourse = await _context.dCourse.AsNoTracking()
                .FirstOrDefaultAsync(x => x.courseID == id);

            if (existingCourse == null) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (coursePhoto != null && coursePhoto.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(coursePhoto.FileName);
                        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await coursePhoto.CopyToAsync(stream);
                        }

                        cCourse.coursePhotoPath = "/uploads/" + uniqueFileName;
                    }
                    else
                    {
                        cCourse.coursePhotoPath = existingCourse.coursePhotoPath;
                    }

                    _context.Update(cCourse);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = cCourse.courseID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mCOURSEExists(cCourse.courseID)) return NotFound();
                    else throw;
                }
            }

            return View(cCourse);
        }




        // ------------------ Delete ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cCourse = await _context.dCourse.FindAsync(id);
            if (cCourse != null)
            {
                _context.dCourse.Remove(cCourse);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.dCourse.FindAsync(id);
            if (course != null)
            {
                _context.dCourse.Remove(course);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }




        private bool mCOURSEExists(int id)
        {
            return _context.dCourse.Any(e => e.courseID == id);
        }
    }


}
