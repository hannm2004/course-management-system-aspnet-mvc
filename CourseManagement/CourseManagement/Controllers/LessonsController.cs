using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Lessons/Create?moduleId=5
        public async Task<IActionResult> Create(int moduleId)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền thêm bài học vào module này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Module = module;
            ViewBag.NextOrder = (await _context.Lessons.Where(l => l.ModuleId == moduleId).MaxAsync(l => (int?)l.Order) ?? 0) + 1;
            return View(new Lesson { ModuleId = moduleId });
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,VideoUrl,Order,ModuleId")] Lesson lesson)
        {
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == lesson.ModuleId);

            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền thêm bài học vào module này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            if (ModelState.IsValid)
            {
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm bài học thành công!";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Module = module;
            return View(lesson);
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (lesson.Module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bài học này.";
                return RedirectToAction("Details", "Courses", new { id = lesson.Module.CourseId });
            }

            ViewBag.Module = lesson.Module;
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,VideoUrl,Order,ModuleId")] Lesson lesson)
        {
            if (id != lesson.Id) return NotFound();

            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == lesson.ModuleId);

            if (module == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bài học này.";
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Lessons.Update(lesson);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật bài học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lessons.Any(l => l.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.Module = module;
            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (lesson.Module.Course.InstructorId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xóa bài học này.";
                return RedirectToAction("Details", "Courses", new { id = lesson.Module.CourseId });
            }

            var courseId = lesson.Module.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa bài học thành công!";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }
}
