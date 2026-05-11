using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // IRONCLAD FORCE SEED
                if (!await _context.Courses.AnyAsync()) 
                {
                    // 1. Clear everything
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM CourseEnrollments; DELETE FROM Lessons; DELETE FROM Modules; DELETE FROM Courses; DELETE FROM Categories;");

                    // 2. Insert 3 Categories via SQL
                    await _context.Database.ExecuteSqlRawAsync("INSERT INTO Categories (Name, Description) VALUES (N'Lập trình Web', N'ASP.NET, React...');");
                    await _context.Database.ExecuteSqlRawAsync("INSERT INTO Categories (Name, Description) VALUES (N'Cơ sở dữ liệu', N'SQL Server, NoSQL...');");
                    await _context.Database.ExecuteSqlRawAsync("INSERT INTO Categories (Name, Description) VALUES (N'Di động', N'Flutter, Android, iOS...');");
                    
                    // 3. IMPORTANT: Re-fetch categories from DB directly
                    var cats = await _context.Categories.FromSqlRaw("SELECT * FROM Categories").OrderBy(c => c.Id).ToListAsync();
                    var instructorId = await _context.Users.Select(u => u.Id).FirstOrDefaultAsync();

                    if (instructorId != null && cats.Count >= 3)
                    {
                        // --- COURSE 1: ASP.NET ---
                        await _context.Database.ExecuteSqlRawAsync($@"
                            INSERT INTO Courses (Title, Description, StartDate, EndDate, Price, IsActive, InstructorId, CategoryId, ImageUrl)
                            VALUES (N'ASP.NET Core MVC 8 Pro', N'Lập trình Web hiện đại', GETDATE(), DATEADD(month, 6, GETDATE()), 0, 1, '{instructorId}', {cats[0].Id}, 'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800')
                        ");

                        // --- COURSE 2: SQL Server ---
                        await _context.Database.ExecuteSqlRawAsync($@"
                            INSERT INTO Courses (Title, Description, StartDate, EndDate, Price, IsActive, InstructorId, CategoryId, ImageUrl)
                            VALUES (N'SQL Server Mastery', N'Quản trị Database chuyên sâu', GETDATE(), DATEADD(month, 4, GETDATE()), 299000, 1, '{instructorId}', {cats[1].Id}, 'https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=800')
                        ");

                        // --- COURSE 3: Flutter ---
                        await _context.Database.ExecuteSqlRawAsync($@"
                            INSERT INTO Courses (Title, Description, StartDate, EndDate, Price, IsActive, InstructorId, CategoryId, ImageUrl)
                            VALUES (N'Flutter for Beginners', N'Phát triển App đa nền tảng', GETDATE(), DATEADD(month, 5, GETDATE()), 150000, 1, '{instructorId}', {cats[2].Id}, 'https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800')
                        ");

                        // Populate some modules/lessons via EF for better reliability
                        var allCourses = await _context.Courses.ToListAsync();
                        foreach(var c in allCourses) {
                            var m = new Module { Title = "Chương 1: Mở đầu", Order = 1, CourseId = c.Id };
                            await _context.Modules.AddAsync(m);
                            await _context.SaveChangesAsync();
                            await _context.Lessons.AddAsync(new Lesson { Title = "Bài 1: Giới thiệu khóa học", Content = "<p>Chào mừng bạn!</p>", Order = 1, ModuleId = m.Id });
                        }
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.SeedError = ex.Message;
            }

            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalStudents = await _context.Users.CountAsync();
            ViewBag.TotalEnrollments = await _context.CourseEnrollments.CountAsync();

            var featured = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.Id)
                .Take(8)
                .ToListAsync();

            ViewBag.FeaturedCourses = featured;

            return View();
        }

        public async Task<IActionResult> SeedData()
        {
            try
            {
                await CourseManagement.Data.SeedData.InitializeAsync(HttpContext.RequestServices);
                return Content("Seed data successful! Go to home page to check.");
            }
            catch (Exception ex)
            {
                return Content("Error seeding data: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
