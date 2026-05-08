using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Enrollments/MyEnrollments
        public async Task<IActionResult> MyEnrollments()
        {
            var userId = _userManager.GetUserId(User);

            var enrollments = await _context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            // Calculate real progress for each enrollment
            foreach (var enrollment in enrollments)
            {
                var totalLessons = enrollment.Course.Modules.SelectMany(m => m.Lessons).Count();
                if (totalLessons > 0)
                {
                    var completedLessons = await _context.StudentProgresses
                        .CountAsync(sp => sp.UserId == userId
                            && sp.Lesson.Module.CourseId == enrollment.CourseId
                            && sp.IsCompleted);
                    enrollment.CompletionPercentage = Math.Round((decimal)completedLessons / totalLessons * 100, 2);
                    enrollment.IsCompleted = enrollment.CompletionPercentage >= 100;
                    _context.CourseEnrollments.Update(enrollment);
                }
            }
            await _context.SaveChangesAsync();

            return View(enrollments);
        }

        // POST: Enrollments/Enroll/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userId = _userManager.GetUserId(User);

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var alreadyEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (!alreadyEnrolled)
            {
                var enrollment = new CourseEnrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now
                };
                _context.CourseEnrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Bạn đã đăng ký khóa học \"{course.Title}\" thành công!";
            }
            else
            {
                TempData["Warning"] = "Bạn đã đăng ký khóa học này rồi.";
            }

            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        // POST: Enrollments/Unenroll/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            var userId = _userManager.GetUserId(User);

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (enrollment != null)
            {
                // Also remove student progress for this course's lessons
                var lessonIds = await _context.Lessons
                    .Where(l => l.Module.CourseId == courseId)
                    .Select(l => l.Id)
                    .ToListAsync();

                var progresses = await _context.StudentProgresses
                    .Where(sp => sp.UserId == userId && lessonIds.Contains(sp.LessonId))
                    .ToListAsync();

                _context.StudentProgresses.RemoveRange(progresses);
                _context.CourseEnrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã huỷ đăng ký khóa học.";
            }

            return RedirectToAction(nameof(MyEnrollments));
        }

        // GET: Enrollments/Learn/5 (courseId)
        public async Task<IActionResult> Learn(int courseId, int? lessonId)
        {
            var userId = _userManager.GetUserId(User);

            // Check enrollment
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (enrollment == null)
            {
                TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            var course = await _context.Courses
                .Include(c => c.Modules.OrderBy(m => m.Order))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.Order))
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            // Get all lessons
            var allLessons = course.Modules.SelectMany(m => m.Lessons).ToList();

            Lesson currentLesson = null;
            if (lessonId.HasValue)
                currentLesson = allLessons.FirstOrDefault(l => l.Id == lessonId.Value);
            if (currentLesson == null)
                currentLesson = allLessons.FirstOrDefault();

            // Get completed lesson IDs for this user
            var completedLessonIds = await _context.StudentProgresses
                .Where(sp => sp.UserId == userId && sp.IsCompleted
                    && allLessons.Select(l => l.Id).Contains(sp.LessonId))
                .Select(sp => sp.LessonId)
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.CurrentLesson = currentLesson;
            ViewBag.CompletedLessonIds = completedLessonIds;
            ViewBag.TotalLessons = allLessons.Count;
            ViewBag.CompletedCount = completedLessonIds.Count;
            ViewBag.Enrollment = enrollment;

            return View();
        }

        // POST: Enrollments/MarkComplete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int lessonId, int courseId)
        {
            var userId = _userManager.GetUserId(User);

            var existing = await _context.StudentProgresses
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.LessonId == lessonId);

            if (existing == null)
            {
                _context.StudentProgresses.Add(new StudentProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedDate = DateTime.Now
                });
            }
            else
            {
                existing.IsCompleted = true;
                existing.CompletedDate = DateTime.Now;
                _context.StudentProgresses.Update(existing);
            }

            await _context.SaveChangesAsync();

            // Update enrollment completion percentage
            var allLessonIds = await _context.Lessons
                .Where(l => l.Module.CourseId == courseId)
                .Select(l => l.Id)
                .ToListAsync();

            var completedCount = await _context.StudentProgresses
                .CountAsync(sp => sp.UserId == userId
                    && allLessonIds.Contains(sp.LessonId)
                    && sp.IsCompleted);

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                enrollment.CompletionPercentage = allLessonIds.Count > 0
                    ? Math.Round((decimal)completedCount / allLessonIds.Count * 100, 2) : 0;
                enrollment.IsCompleted = enrollment.CompletionPercentage >= 100;
                _context.CourseEnrollments.Update(enrollment);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Đã đánh dấu bài học hoàn thành!";
            return RedirectToAction(nameof(Learn), new { courseId, lessonId });
        }

        public async Task<IActionResult> Certificate(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var enrollment = await _context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (enrollment == null || !enrollment.IsCompleted)
            {
                TempData["Error"] = "Bạn chưa hoàn thành khóa học này để nhận chứng chỉ.";
                return RedirectToAction(nameof(MyEnrollments));
            }

            return View(enrollment);
        }
    }
}
