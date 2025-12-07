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
    public class cTestController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cTestController(PiecebyPieceDBContext context)
        {
            _context = context;
        }


        // ------------------ Create ------------------
        public IActionResult Create()
        {
            ViewData["lessonID"] = new SelectList(_context.dLesson, "lessonID", "lessonName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int lessonID)
        {
            var existingTest = await _context.dTest.FirstOrDefaultAsync(t => t.lessonID == lessonID);
            if (existingTest != null)
            {
                return RedirectToAction("Details", "cLesson", new { id = lessonID });
            }

            var cTest = new mTEST
            {
                lessonID = lessonID
            };

            _context.dTest.Add(cTest);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "cLesson", new { id = lessonID });
        }



        private bool mTESTExists(int id)
        {
            return _context.dTest.Any(e => e.testID == id);
        }
    }
}
