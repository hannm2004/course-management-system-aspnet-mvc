using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    [Authorize]
    public class ModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModulesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Modules/Create?courseId=5
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền thêm module vào khóa học này.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            ViewBag.Course = course;
            ViewBag.NextOrder = (await _context.Modules.Where(m => m.CourseId == courseId).MaxAsync(m => (int?)m.Order) ?? 0) + 1;
            return View(new Module { CourseId = courseId });
        }

        // POST: Modules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Order,CourseId")] Module module)
        {
            var course = await _context.Courses.FindAsync(module.CourseId);
            if (course == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền thêm module vào khóa học này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            if (ModelState.IsValid)
            {
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm module thành công!";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Course = course;
            return View(module);
        }

        // GET: Modules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa module này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Course = module.Course;
            return View(module);
        }

        // POST: Modules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Order,CourseId")] Module module)
        {
            if (id != module.Id) return NotFound();

            var course = await _context.Courses.FindAsync(module.CourseId);
            if (course == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa module này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Modules.Update(module);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật module thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Modules.Any(m => m.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Course = course;
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xóa module này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            var courseId = module.CourseId;
            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa module thành công!";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }
}
