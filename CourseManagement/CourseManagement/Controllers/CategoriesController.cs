using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string search)
        {
            ViewBag.Search = search;
            var query = _context.Categories.Include(c => c.Courses).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search) || c.Description.Contains(search));

            var categories = await query.OrderBy(c => c.Name).ToListAsync();
            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Courses)
                .ThenInclude(co => co.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();
            return View(category);
        }

        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.Include(c => c.Courses).FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.Courses != null && category.Courses.Any())
                {
                    TempData["Error"] = "Không thể xóa danh mục này vì đang có khóa học thuộc danh mục. Hãy xóa hoặc chuyển các khóa học sang danh mục khác trước.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
