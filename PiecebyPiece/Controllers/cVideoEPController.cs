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
    public class cVideoEPController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cVideoEPController(PiecebyPieceDBContext context)
        {
            _context = context;
        }

        // ------------------ Index ------------------
        public async Task<IActionResult> Index()
        {
            var piecebyPieceDBContext = _context.dVideoEP.Include(m => m.Lesson);
            return View(await piecebyPieceDBContext.ToListAsync());
        }



        // ------------------ Details ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cVideoEP = await _context.dVideoEP
                .Include(m => m.Lesson)
                .FirstOrDefaultAsync(m => m.vdoID == id);
            if (cVideoEP == null)
            {
                return NotFound();
            }

            return View(cVideoEP);
        }



        // ------------------ Create ------------------
        [HttpGet]
        public IActionResult Create(int lessonID)
        {
            var cLesson = _context.dLesson.FirstOrDefault(x => x.lessonID == lessonID);
            if (cLesson == null)
            {
                return NotFound();
            }

            ViewData["lessonName"] = cLesson.lessonName;

            var cVideoEP = new mVideoEP
            {
                lessonID = lessonID
            };

            return View(cVideoEP);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("vdoID,lessonID,vdoName,vdoFilePath,epFilePath,epProgress")] mVideoEP cVideoEP)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cVideoEP);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "cLesson", new { id = cVideoEP.lessonID });
            }

            return View(cVideoEP);
        }



        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cVideoEP = await _context.dVideoEP
                .Include(ep => ep.Lesson)
                .FirstOrDefaultAsync(ep => ep.vdoID == id);

            if (cVideoEP == null)
            {
                return NotFound();
            }

            if (cVideoEP.Lesson != null)
            {
                ViewData["LessonName"] = cVideoEP.Lesson.lessonName;
            }
            else
            {
                ViewData["LessonName"] = "Lesson Not Found";
            }
            return View(cVideoEP);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("vdoID,lessonID,vdoName,vdoFilePath,epFilePath,epProgress")] mVideoEP cVideoEP)
        {
            if (id != cVideoEP.vdoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cVideoEP);
                    _context.Entry(cVideoEP).Reference(e => e.Lesson).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mVideoEPExists(cVideoEP.vdoID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var lesson = await _context.dLesson.FindAsync(cVideoEP.lessonID);
            if (lesson != null)
            {
                ViewData["LessonName"] = lesson.lessonName;
            }
            return View(cVideoEP);

        }

        // ------------------ Delete ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            return RedirectToAction("Details", "cLesson", new { id = id.Value });
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cVideoEP = await _context.dVideoEP
                .FirstOrDefaultAsync(ep => ep.vdoID == id);

            if (cVideoEP != null)
            {
                _context.dVideoEP.Remove(cVideoEP);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool mVideoEPExists(int id)
        {
            return _context.dVideoEP.Any(e => e.vdoID == id);
        }



    }
}
