using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiecebyPiece.Filters;
using PiecebyPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PiecebyPiece.Controllers
{
    [ServiceFilter(typeof(AdminAuthorizeFilter))]
    public class cAdminPanelController : Controller
    {
        private readonly PiecebyPieceDBContext _context;

        public cAdminPanelController(PiecebyPieceDBContext context)
        {
            _context = context;
        }


        // GET: cAdminPanel
        public async Task<IActionResult> Index(string cSearch)
        {
            int currentLoggedInAdminId = GetCurrentAdminId();
            ViewBag.CurrentAdminId = currentLoggedInAdminId;

            var adminQuery = _context.dAdmin.AsQueryable();
            if (!string.IsNullOrEmpty(cSearch))
            {
                string search = cSearch.Trim().ToLower();

                adminQuery = adminQuery.Where(a =>

                    a.adminID.ToString().Contains(search) ||
                    a.adminName.ToLower().Contains(search) ||
                    a.adminSurname.ToLower().Contains(search) ||
                    (a.adminName + " " + a.adminSurname).ToLower().Contains(search) ||
                    a.adminEmail.ToLower().Contains(search)
                );

                ViewBag.CurrentSearch = cSearch;
            }
            else
            {
                ViewBag.CurrentSearch = "";
            }

            var admins = await adminQuery.ToListAsync();

            return View(admins);
        }




        // GET: cAdminPanel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mADMIN = await _context.dAdmin
                .FirstOrDefaultAsync(m => m.adminID == id);
            if (mADMIN == null)
            {
                return NotFound();
            }

            return View(mADMIN);
        }

        // GET: cAdminPanel/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("adminID,adminName,adminSurname,adminEmail,adminPassword,adminConfirmPassword")] mADMIN mADMIN)
        {
            if (ModelState.IsValid)
            {
                bool adminEmailExists = await _context.dAdmin.AnyAsync(a => a.adminEmail == mADMIN.adminEmail);
                bool userEmailExists = await _context.dUser.AnyAsync(u => u.userEmail == mADMIN.adminEmail);

                if (adminEmailExists || userEmailExists)
                {
                    ModelState.AddModelError("adminEmail", "อีเมลนี้มีผุ้ใช้อื่นใช้แล้ว กรุณาใช้อีเมลอื่น.");
                    return View(mADMIN);
                }

                _context.Add(mADMIN);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mADMIN);
        }

        // GET: cAdminPanel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mADMIN = await _context.dAdmin.FindAsync(id);
            if (mADMIN == null)
            {
                return NotFound();
            }
            return View(mADMIN);
        }

        // POST: cAdminPanel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("adminID,adminName,adminSurname,adminEmail,adminPassword,adminConfirmPassword")] mADMIN mADMIN)
        {
            if (id != mADMIN.adminID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mADMIN);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!mADMINExists(mADMIN.adminID))
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
            return View(mADMIN);
        }

        // GET: cAdminPanel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mADMIN = await _context.dAdmin
                .FirstOrDefaultAsync(m => m.adminID == id);
            if (mADMIN == null)
            {
                return NotFound();
            }

            return View(mADMIN);
        }

        // ใน cAdminPanel Controller

        // ------------------ Delete (POST) ------------------
        // ใน cAdminPanel Controller
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int currentLoggedInAdminId = GetCurrentAdminId();

            string errorReason = null; // ใช้ตัวแปรนี้เก็บโค้ด error

            // 1. ตรวจสอบเงื่อนไขห้ามลบ
            if (id == 1)
            {
                errorReason = "Admin1"; // Flag: ห้ามลบ Admin ID: 1
            }
            else if (id == currentLoggedInAdminId)
            {
                errorReason = "Self"; // Flag: ห้ามลบบัญชีตัวเอง
            }

            // ถ้ามี Error ให้ Redirect พร้อม Query String
            if (errorReason != null)
            {
                // Redirect ไปที่ Index พร้อม Query String ?error=Admin1 หรือ ?error=Self
                return RedirectToAction(nameof(Index), new { error = errorReason });
            }

            // 2. ดำเนินการลบ (ถ้าไม่มี Error)
            var cAdmin = await _context.dAdmin.FindAsync(id);
            if (cAdmin != null)
            {
                _context.dAdmin.Remove(cAdmin);
            }
            await _context.SaveChangesAsync();

            // (ไม่มีการแจ้งเตือนความสำเร็จ)

            return RedirectToAction(nameof(Index)); // Redirect ธรรมดา
        }

        private int GetCurrentAdminId()
        {

            if (HttpContext == null) return 0;


            int? adminIdNullable = HttpContext.Session.GetInt32("adminID");

            if (adminIdNullable.HasValue)
            {
                return adminIdNullable.Value;
            }


            return 0;
        }






        private bool mADMINExists(int id)
        {
            return _context.dAdmin.Any(e => e.adminID == id);
        }
    }
}
