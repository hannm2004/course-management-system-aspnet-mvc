using CourseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            // Base query for courses (Admin sees all, Employee sees their own)
            var courseQuery = _context.Courses.AsQueryable();
            if (!isAdmin)
            {
                courseQuery = courseQuery.Where(c => c.InstructorId == userId);
            }

            var totalCourses = await courseQuery.CountAsync();
            var totalEnrollments = await _context.CourseEnrollments
                .Where(e => isAdmin || e.Course.InstructorId == userId)
                .CountAsync();
            
            var totalStudents = await _context.CourseEnrollments
                .Where(e => isAdmin || e.Course.InstructorId == userId)
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();

            var recentEnrollments = await _context.CourseEnrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .Where(e => isAdmin || e.Course.InstructorId == userId)
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(5)
                .ToListAsync();

            var popularCourses = await courseQuery
                .Include(c => c.CourseEnrollments)
                .OrderByDescending(c => c.CourseEnrollments.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalEnrollments = totalEnrollments;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.RecentEnrollments = recentEnrollments;
            ViewBag.PopularCourses = popularCourses;

            return View();
        }
    }
}
