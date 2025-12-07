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
    public class cQuestionController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cQuestionController(PiecebyPieceDBContext context)
        {
            _context = context;
        }

        // ------------------ Details ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var test = await _context.dTest
                .Include(t => t.Lesson)
                .FirstOrDefaultAsync(t => t.testID == id);
            if (test != null && test.Lesson != null)
            {
                ViewData["CurrentLessonId"] = test.lessonID;
            }

            var cQuestion = await _context.dQuestion
                .Include(m => m.Test)
                .FirstOrDefaultAsync(m => m.questionID == id);
            if (cQuestion == null)
            {
                return NotFound();
            }

            return View(cQuestion);
        }

        // ------------------ Create ------------------
        public async Task<IActionResult> Create(int testId)
        {

            var test = await _context.dTest
                .Include(m => m.Lesson)
                .FirstOrDefaultAsync(t => t.testID == testId);

            if (test == null)
            {
                return NotFound();
            }
            ViewData["LessonName"] = test.Lesson.lessonName;
            ViewData["LessonID"] = test.lessonID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(mQUESTION cQuestion)
        {
            if (ModelState.IsValid)
            {
                if (cQuestion.questionPhoto != null && cQuestion.questionPhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(cQuestion.questionPhoto.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var savePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await cQuestion.questionPhoto.CopyToAsync(stream);
                    }
                    cQuestion.questionPhotoPath = "/uploads/" + uniqueFileName;
                }
                _context.Add(cQuestion);
                await _context.SaveChangesAsync();

                var test = await _context.dTest.FirstOrDefaultAsync(t => t.testID == cQuestion.testID);
                if (test != null)
                {
                    return RedirectToAction("Details", "cLesson", new { id = test.lessonID });
                }
                return RedirectToAction("Details", "cLesson");
            }

            ViewData["testID"] = new SelectList(_context.dTest, "testID", "testID", cQuestion.testID);
            return View(cQuestion);
        }



        // ------------------ Edit ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cQuestion = await _context.dQuestion
                .Include(q => q.Test)
                .ThenInclude(t => t.Lesson)
                .FirstOrDefaultAsync(q => q.questionID == id);

            if (cQuestion == null) return NotFound();

            if (cQuestion.Test?.Lesson != null)
            {
                ViewData["LessonName"] = cQuestion.Test.Lesson.lessonName;
                ViewData["LessonID"] = cQuestion.Test.Lesson.lessonID;
            }
            return View(cQuestion);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("questionID,questionText,correctAnswer,questionScore,questionPhoto,questionPhotoPath,choiceA,choiceB,choiceC,choiceD,testID,lessonID")] mQUESTION cQuestion, IFormFile? questionPhoto)
        {
            if (id != cQuestion.questionID) return NotFound();

            var existingQuestion = await _context.dQuestion.AsNoTracking()
                .FirstOrDefaultAsync(x => x.questionID == id);
            if (existingQuestion == null) return NotFound();

            ModelState.Remove("questionPhoto");

            if (ModelState.IsValid)
            {
                if (questionPhoto != null && questionPhoto.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(questionPhoto.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await questionPhoto.CopyToAsync(stream);
                    }
                    cQuestion.questionPhotoPath = "/uploads/" + uniqueFileName;
                }
                else
                {
                    cQuestion.questionPhotoPath = existingQuestion.questionPhotoPath;
                }

                try
                {
                    _context.Update(cQuestion);
                    _context.Entry(cQuestion).Reference(e => e.Test).IsModified = false;
                    if (questionPhoto == null || questionPhoto.Length == 0) { }

                    await _context.SaveChangesAsync();
                    var targetLessonId = await _context.dTest
                                           .Where(t => t.testID == cQuestion.testID)
                                           .Select(t => t.lessonID)
                                           .FirstOrDefaultAsync();

                    if (targetLessonId != 0)
                    {
                        return RedirectToAction("Details", "cLesson", new { id = targetLessonId });
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mQUESTIONExists(cQuestion.questionID)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Save failed due to an error: " + ex.Message);
                    var currentTest = await _context.dTest.Include(t => t.Lesson).FirstOrDefaultAsync(t => t.testID == cQuestion.testID);
                    if (currentTest?.Lesson != null) ViewData["LessonName"] = currentTest.Lesson.lessonName;
                    return View(cQuestion);
                }
            }
            var currentTest_Fallback = await _context.dTest.Include(t => t.Lesson).FirstOrDefaultAsync(t => t.testID == cQuestion.testID);
            if (currentTest_Fallback?.Lesson != null)
            {
                ViewData["LessonName"] = currentTest_Fallback.Lesson.lessonName;
                ViewData["LessonID"] = currentTest_Fallback.Lesson.lessonID;
            }
            return View(cQuestion);
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
            var testInfo = await _context.dQuestion
                .Where(q => q.questionID == id)
                .Select(q => new { q.testID, LessonId = q.Test.lessonID })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            int lessonIdToRedirect = testInfo?.LessonId ?? 0;

            var cQuestion = await _context.dQuestion.FindAsync(id);

            if (cQuestion != null)
            {
                _context.dQuestion.Remove(cQuestion);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true, lessonId = lessonIdToRedirect });
        }




        private bool mQUESTIONExists(int id)
        {
            return _context.dQuestion.Any(e => e.questionID == id);
        }
    }
}
