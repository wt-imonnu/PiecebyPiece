using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Models;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace PiecebyPiece.Controllers
{
    public class cUserPerspController : Controller
    {
        private readonly PiecebyPieceDBContext _context;
        public cUserPerspController(PiecebyPieceDBContext context)
        {
            _context = context;
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


        //----------------- Course Library -------------------
        public IActionResult CourseLibrary(string? subject, string cSearch, List<string> cFilter)
        {
            var cPersp = GetLoggedInUserFromSession();

            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "cUser");

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
                return RedirectToAction("Login", "cUser");

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // search+filter
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

            // FINAL RESULT
            var courseList = ccCourse.ToList();

            // Enrolled check
            var enrolledCourseIDs = _context.dEnrollment
                .Where(e => e.userID == user.userID)
                .Select(e => e.courseID)
                .ToHashSet();

            ViewBag.EnrolledCourseIDs = enrolledCourseIDs;

            return View(courseList);
        }


        //---------------- Course Details ---------------
        public async Task<IActionResult> CourseDetails(int? id)
        {
            var cNav = GetLoggedInUserFromSession();

            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            // ส่งค่าไป View
            ViewBag.HasCourse = enrollCount > 0;

            
            if (id == null) return NotFound();

            var cCourse = await _context.dCourse
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.courseID == id);

            if (cCourse == null) return NotFound();

            return View(cCourse);
        }

        //GoToMyCourseCondition
        public IActionResult GoToMyCourse()
        {
            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "cUser");

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
                return RedirectToAction("Login", "cUser");

            // เช็คว่ามีคอร์สไหม
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            if (enrollCount > 0)
            {
                return RedirectToAction("MyCourse");
            }
            else
            {
                TempData["NoCourse"] = "Please enroll at least one course.";
                return RedirectToAction("CourseLibrary");
            }
        }

        public IActionResult EnrollCourse(int courseID)
        {
            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
                return RedirectToAction("Login", "cUser");

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
                return RedirectToAction("Login", "cUser");

            // เช็คว่าลงแล้วหรือยัง
            var alreadyEnrolled = _context.dEnrollment.Any(e =>
                e.courseID == courseID && e.userID == user.userID);

            if (!alreadyEnrolled)
            {
                var enroll = new mENROLLMENT
                {
                    courseID = courseID,
                    userID = user.userID,
                    enrollTime = DateTime.Now
                };

                _context.Add(enroll);
                _context.SaveChanges();
            }

            return RedirectToAction("MyCourse");
        }



        //----------------- MyCourse -------------------
        public async Task<IActionResult> MyCourse(int? courseID, int? index = 0)
        {
            var cNav = GetLoggedInUserFromSession();

            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            // ส่งค่าไป View
            ViewBag.HasCourse = enrollCount > 0;

            if (enrollCount == 0)
            {
                TempData["NoCourse"] = true;
                return RedirectToAction("CourseLibrary", "cUserPersp");
            }

            // ✅ ถ้ามี courseID ส่งมาจะทำ enroll
            if (courseID.HasValue)
            {
                var alreadyEnroll = _context.dEnrollment
                    .Any(e => e.userID == user.userID
                           && e.courseID == courseID.Value
                           && e.enrollStatus == "In Progress");

                if (!alreadyEnroll)
                {
                    var course = _context.dCourse.FirstOrDefault(c => c.courseID == courseID.Value);
                    if (course != null)
                    {
                        var enroll = new mENROLLMENT
                        {
                            userID = user.userID,
                            courseID = course.courseID,
                            courseName = course.courseName,
                            enrollStatus = "In Progress",
                            enrollTime = DateTime.Now
                        };
                        _context.dEnrollment.Add(enroll);
                        _context.SaveChanges();
                    }
                }
                // หลัง enroll → index = courseID ของคอร์สล่าสุด เพื่อเลือกคอร์สนั้น
                index = _context.dEnrollment
                        .Where(e => e.userID == user.userID)
                        .OrderBy(e => e.enrollTime)
                        .ToList()
                        .FindIndex(e => e.courseID == courseID.Value);
            }

            // ✅ ดึงคอร์สที่ user enroll แล้ว
            var enrolledCourses = await _context.dEnrollment // 👈 เพิ่ม await
                                  .Include(uc => uc.Course)
                                  .ThenInclude(c => c.Lessons)
                                  .Where(uc => uc.userID == user.userID)
                                  .Select(uc => uc.Course)
                                  .ToListAsync();

            if (!enrolledCourses.Any())
                return View(enrolledCourses);

            // 🔄 วน index ให้ไม่เกินขอบเขต
            int total = enrolledCourses.Count;
            if (!index.HasValue) index = 0;
            if (index < 0) index = total - 1;
            else if (index >= total) index = 0;
            var currentCourse = enrolledCourses[(int)index]; // คอร์สที่กำลังแสดงผล

            var userProgress = await _context.dLessonProgress
                .Where(p => p.userID == user.userID && p.courseID == currentCourse.courseID)
                .ToDictionaryAsync(p => p.lessonID, p => p.isPassedTest);

            ViewBag.UserProgress = userProgress;
            // ----------------------------------------------------------------

            ViewBag.CurrentIndex = index;
            ViewBag.CourseList = enrolledCourses;

            return View(enrolledCourses);
        }



        //----------------- Lesson -------------------
        public async Task<IActionResult> LearnLesson(int? lessonID, int? vdoID)
        {
            var cPersp = GetLoggedInUserFromSession();
            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            // ส่งค่าไป View
            ViewBag.HasCourse = enrollCount > 0;


            if (lessonID == null)
                return NotFound();


            var lesson = await _context.dLesson
                .Include(l => l.Course)
                .Include(l => l.VideoEPs)
                .Include(l => l.Test)
                .FirstOrDefaultAsync(l => l.lessonID == lessonID);

            if (lesson == null)
                return NotFound();

            var prevLesson = await _context.dLesson
                .Where(l => l.courseID == lesson.courseID
                         && l.lessonID < lesson.lessonID)
                .OrderByDescending(l => l.lessonID)
                .FirstOrDefaultAsync();

            ViewBag.PrevLessonID = prevLesson?.lessonID;


            if (vdoID == null)
            {
                var firstEP = lesson.VideoEPs.FirstOrDefault();
                ViewBag.CurrentEPID = firstEP?.vdoID;
            }
            else
            {
                ViewBag.CurrentEPID = vdoID;
            }


            return View(lesson);
        }


        //---------------- DownloadFile ----------------
        public IActionResult DownloadDocument(int id)
        {
            var video = _context.dVideoEP.FirstOrDefault(v => v.vdoID == id);

            if (video == null || string.IsNullOrEmpty(video.epFilePath))
            {
                return NotFound("File not found");
            }

            return Redirect(video.epFilePath);
        }



        //----------------- Test -------------------
        public IActionResult TestStart(int testID)
        {
            var cNav = GetLoggedInUserFromSession();

            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            // ส่งค่าไป View
            ViewBag.HasCourse = enrollCount > 0;

            var test = _context.dTest
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Course)   // ⭐ โหลด Course ด้วย
                .Include(t => t.Questions)
                .FirstOrDefault(t => t.testID == testID);

            if (test == null) return NotFound();

            return View(test);
        }


        [HttpPost]
        public async Task<IActionResult> Submit(int testID)
        {
            var cPersp = GetLoggedInUserFromSession();

            var userName = HttpContext.Session.GetString("userName");
            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);
            if (user == null)
            {
                ViewBag.HasCourse = false;
                return RedirectToAction("Login", "cUser");
            }

            ViewData["UserName"] = user.userName;
            ViewData["UserSurname"] = user.userSurname;
            ViewData["UserID"] = user.userID;
            ViewData["UserPhotoPath"] = user.userPhotoPath;

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            // ส่งค่าไป View
            ViewBag.HasCourse = enrollCount > 0;

            // --- 1. ตรวจสอบ Test และ User ---

            // ดึง Test พร้อม Lesson ที่เกี่ยวข้อง
            var test = await _context.dTest
                .Include(t => t.Questions)
                .Include(t => t.Lesson)
                .FirstOrDefaultAsync(t => t.testID == testID);

            if (test == null) return NotFound();

            // ดึง User และตรวจสอบ Login อย่างครบถ้วน
            int? userID = HttpContext.Session.GetInt32("userID");
            if (!userID.HasValue)
                return RedirectToAction("Login", "cUser");

            var userS = await _context.dUser.FirstOrDefaultAsync(u => u.userID == userID.Value);
            if (userS == null)
                return RedirectToAction("Login", "cUser");


            // --- 2. ตรวจคำตอบและคำนวณคะแนน ---
            var questionIDs = Request.Form["questionID"];
            int total = questionIDs.Count;
            int correct = 0;

            foreach (var qidStr in questionIDs)
            {
                int qid = int.Parse(qidStr);
                string userAnswer = Request.Form[$"selectedAnswer[{qid}]"];
                var q = test.Questions.First(x => x.questionID == qid);

                if (q.correctAnswer == userAnswer)
                    correct++;
            }

            double percent = (double)correct / total * 100;
            bool passStatus = percent >= 80;


            // --- 3. ค้นหา EnrollmentID ที่ถูกต้อง ---
            // ต้องรู้ว่าผู้ใช้กำลังทำ Test สำหรับการลงทะเบียน (Enrollment) ไหน
            var enrollment = await _context.dEnrollment
                .FirstOrDefaultAsync(e => e.userID == userID.Value && e.courseID == test.Lesson.courseID);

            if (enrollment == null)
                return RedirectToAction("MyCourse"); // ควรกลับไปหน้าคอร์สของฉันถ้าไม่มี Enrollment


            // --- 4. อัปเดต dLessonProgress (พร้อม enrollID/courseID) ---
            var progress = await _context.dLessonProgress
                .FirstOrDefaultAsync(p => p.userID == userID.Value
                                       && p.lessonID == test.lessonID
                                       && p.enrollID == enrollment.enrollID); // ต้องใช้ enrollID

            if (progress == null)
            {
                progress = new mLESSONPROGRESS
                {
                    userID = user.userID,
                    lessonID = test.lessonID,
                    enrollID = enrollment.enrollID,        // 👈 เพิ่ม enrollID
                    courseID = test.Lesson.courseID,       // 👈 เพิ่ม courseID
                    progressPercent = (float)percent,
                    isPassedTest = passStatus,             // 👈 ใช้ passStatus
                    lastUpdate = DateTime.Now
                };
                _context.dLessonProgress.Add(progress);
            }
            else
            {
                progress.progressPercent = (float)percent;
                progress.isPassedTest = passStatus;
                progress.lastUpdate = DateTime.Now;
                _context.dLessonProgress.Update(progress);
            }

            // ⭐ ลบ: ไม่จำเป็นต้องอัปเดต Lesson.lessonProgress
            // test.Lesson.lessonProgress = 100;

            await _context.SaveChangesAsync();


            // --- 5. ตรวจสอบ Certificate และ Redirect ---
            if (passStatus)
            {
                // ตรวจสอบว่าผ่านครบทุกบทเรียนหรือไม่ และสร้าง Certificate ถ้าครบ
                await CheckAndCreateCertificate(enrollment.enrollID);
            }

            // --- 6. กำหนดค่า ViewBag สำหรับ View ---
            var nextLesson = await _context.dLesson
                .Where(l => l.courseID == test.Lesson.courseID && l.lessonID > test.lessonID)
                .OrderBy(l => l.lessonID)
                .FirstOrDefaultAsync();

            ViewBag.NextLessonID = nextLesson?.lessonID;
            ViewBag.TestID = test.testID;
            ViewBag.ScorePercent = percent;
            ViewBag.Result = passStatus ? "You Pass" : "You Fail";
            ViewBag.LessonID = test.lessonID;
            ViewBag.CourseID = test.Lesson.courseID;




            return View("TestResult");
        }


        // ------------------ EditProfile ------------------
        public async Task<IActionResult> EditProfile(int? id)
        {
            if (id == null) return NotFound();

            var cUser = await _context.dUser.FindAsync(id);
            if (cUser == null) return NotFound();

            return View(cUser);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int id, mUSER cUser, IFormFile? userPhoto)
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

                return RedirectToAction("AfterEditProfileRedirect");
            }

            return View(cUser);
        }

        public IActionResult AfterEditProfileRedirect()
        {
            var userName = HttpContext.Session.GetString("userName");

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);

            if (user == null)
            {
                return RedirectToAction("Login", "cUser");
            }

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            if (enrollCount == 0)
            {
                TempData["NoCourse"] = true;
                return RedirectToAction("CourseLibrary", "cUserPersp");
            }

            return RedirectToAction("MyCourse", "cUserPersp");
        }


        public IActionResult CancelEditProfile()
        {
            var userName = HttpContext.Session.GetString("userName");

            if (string.IsNullOrEmpty(userName))
            {
                // ถ้าไม่ล็อกอิน → ไปหน้า Login
                return RedirectToAction("Login", "cUser");
            }

            var user = _context.dUser.FirstOrDefault(u => u.userName == userName);

            if (user == null)
            {
                return RedirectToAction("Login", "cUser");
            }

            // นับจำนวนคอร์สที่ user enroll
            var enrollCount = _context.dEnrollment.Count(e => e.userID == user.userID);

            if (enrollCount == 0)
            {
                TempData["NoCourse"] = true; // ให้แจ้งเตือนในหน้า CourseLibrary
                return RedirectToAction("CourseLibrary", "cUserPersp");
            }

            return RedirectToAction("MyCourse", "cUserPersp");
        }




        //-----------------------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task CheckAndCreateCertificate(int enrollID)
        {
            // 1. ดึง Enrollment และ Course ที่เกี่ยวข้อง
            var enrollment = await _context.dEnrollment
                .Include(e => e.Course) // ต้อง Include Course
                .Include(e => e.User)   // ต้อง Include User สำหรับชื่อ
                .FirstOrDefaultAsync(e => e.enrollID == enrollID);

            if (enrollment?.Course == null || enrollment.User == null) return;

            // 2. นับจำนวน Lesson ทั้งหมดในคอร์ส
            int totalLessons = await _context.dLesson
                .CountAsync(l => l.courseID == enrollment.courseID);

            // 3. นับจำนวน Lesson ที่ผู้ใช้ผ่าน Test แล้ว (ภายใต้ Enrollment นี้)
            int passedTests = await _context.dLessonProgress
                .CountAsync(lp => lp.enrollID == enrollID && lp.isPassedTest == true);

            // 4. ตรวจสอบเงื่อนไขการสร้าง Certificate
            if (totalLessons > 0 && totalLessons == passedTests)
            {
                // 5. ตรวจสอบว่า Certificate ถูกสร้างไปแล้วหรือยัง
                bool cerExists = await _context.dCertificate.AnyAsync(c => c.enrollID == enrollID);

                if (!cerExists)
                {
                    // 6. สร้าง Certificate ใหม่
                    var certificate = new mCERTIFICATE
                    {
                        enrollID = enrollID,
                        cerDate = DateTime.Now,
                        courseName = enrollment.courseName,
                        userName = enrollment.User.userName,
                        userSurname = enrollment.User.userSurname,
                        // ไม่จำเป็นต้องกำหนด userID ใน mCERTIFICATE เพราะใช้ enrollID เชื่อมโยge
                    };

                    _context.dCertificate.Add(certificate);
                    enrollment.enrollStatus = "Completed"; // อัปเดตสถานะ enrollment
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
