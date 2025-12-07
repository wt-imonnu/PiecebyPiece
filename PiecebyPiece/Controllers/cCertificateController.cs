using DinkToPdf;
using DinkToPdf.Contracts;
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
using static DinkToPdf.GlobalSettings;

namespace PiecebyPiece.Controllers
{
    public class cCertificateController : Controller
    {
        private readonly PiecebyPieceDBContext _context;
        private readonly IConverter _converter;

        private readonly ITempDataProvider _tempDataProvider;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public cCertificateController(
                                        PiecebyPieceDBContext context,
                                        IConverter converter,
                                        ITempDataProvider tempDataProvider,
                                        ICompositeViewEngine viewEngine,
                                        IWebHostEnvironment hostingEnvironment,
                                        IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _converter = converter;
            _tempDataProvider = tempDataProvider;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<IActionResult> Index(string cSearch, List<string> cFilter)
        {
            var certificates = _context.dCertificate
                .Include(cert => cert.Enrollment)
                .ThenInclude(enroll => enroll.Course)
                .Include(cert => cert.Enrollment)
                .ThenInclude(enroll => enroll.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cSearch))
            {
                string searchLower = cSearch.ToLower();

                if (int.TryParse(cSearch, out int searchId))
                {
                    certificates = certificates.Where(cert =>
                        cert.cerID == searchId ||
                        cert.enrollID == searchId ||
                        cert.Enrollment.userID == searchId ||
                        cert.courseName.ToLower().Contains(searchLower) ||
                        cert.userName.ToLower().Contains(searchLower) ||
                        cert.userSurname.ToLower().Contains(searchLower) ||
                        cert.Enrollment.Course.courseName.ToLower().Contains(searchLower)
                    );
                }
                else
                {
                    certificates = certificates.Where(cert =>
                        cert.courseName.ToLower().Contains(searchLower) ||
                        cert.userName.ToLower().Contains(searchLower) ||
                        cert.userSurname.ToLower().Contains(searchLower) ||
                        cert.Enrollment.Course.courseName.ToLower().Contains(searchLower)
                    );
                }
            }

            if (cFilter != null && cFilter.Count > 0)
            {
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
        public async Task<IActionResult> Create([Bind("cerID,cerDate,enrollID,courseName,userName,userSurname")] mCERTIFICATE mCERTIFICATE)
        {
            bool certificateExists = await _context.dCertificate.AnyAsync(c => c.enrollID == mCERTIFICATE.enrollID);

            if (certificateExists)
            {
                ModelState.AddModelError(string.Empty,
                $"Enrollment ID: #{mCERTIFICATE.enrollID} already has a certificate issued. Please edit or delete the existing certificate, or  choose a different enrollment.");
            }

            if (ModelState.IsValid && !certificateExists)
            {
                _context.Add(mCERTIFICATE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
                    if (!mCERTIFICATEExists(mCERTIFICATE.cerID))
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




        //----------------Download----------------
        public async Task<IActionResult> Download(int enrollID)
        {
            var certificate = await _context.dCertificate
                .Include(c => c.Enrollment)
                .FirstOrDefaultAsync(c => c.enrollID == enrollID);
            if (certificate == null)
                return NotFound("Certificate data not found.");

            int? currentUserID = HttpContext.Session.GetInt32("userID");
            if (!currentUserID.HasValue || certificate.Enrollment?.userID != currentUserID)
            {
                return RedirectToAction("Login", "cUser");
            }

            string htmlContent = await RenderViewToStringAsync("CertificatePDF", certificate);


            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = DinkToPdf.PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                    DPI = 300,
                },
                Objects = {
                    new ObjectSettings()
                    {
                        HtmlContent = htmlContent,
                        PagesCount = true,
                        WebSettings = { DefaultEncoding = "utf-8" },
                    }
                }

            };

            byte[] pdf = _converter.Convert(doc);

            string fullName = $"{certificate.userName} {certificate.userSurname}";
            string fileName = $"Certificate_{fullName}_{certificate.cerID}.pdf";

            return File(pdf, "application/pdf", fileName);
        }


        // ------------------ User Certificate List ------------------
        public async Task<IActionResult> UserCertificateList()
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
                                    .Where(c => c.Enrollment != null && c.Enrollment.userID == user.userID)
                                    .OrderByDescending(c => c.cerDate)
                                    .ToListAsync();
            return View(userCertificates);

        }



        // -----------------------------------
        private bool mCERTIFICATEExists(int id)
        {
            return _context.dCertificate.Any(e => e.cerID == id);
        }
        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = this.HttpContext;
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"Unable to find view '{viewName}'. The following locations were searched: {string.Join(",", viewResult.SearchedLocations)}");
            }

            using (var writer = new StringWriter())
            {
                var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(httpContext, _tempDataProvider),
                    writer,
                    new HtmlHelperOptions()
                );
                await viewResult.View.RenderAsync(viewContext);
                return writer.ToString();
            }
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
