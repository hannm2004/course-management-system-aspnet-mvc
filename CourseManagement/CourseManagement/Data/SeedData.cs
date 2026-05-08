using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Models;

namespace CourseManagement.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager  = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context      = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ── 1. Roles ──────────────────────────────────────────────
            foreach (var role in new[] { "Admin", "Employee", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── 2. Admin user ─────────────────────────────────────────
            const string adminEmail = "admin@edumanage.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    FullName       = "System Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // ── 3. Demo Instructor ────────────────────────────────────
            const string instructorEmail = "instructor@edumanage.vn";
            var instructor = await userManager.FindByEmailAsync(instructorEmail);
            if (instructor == null)
            {
                instructor = new ApplicationUser
                {
                    UserName       = instructorEmail,
                    Email          = instructorEmail,
                    FullName       = "Nguyễn Văn Giảng",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(instructor, "Instructor@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(instructor, "Employee");
            }

            // ── 4. Demo Student ───────────────────────────────────────
            const string studentEmail = "student@edumanage.vn";
            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                student = new ApplicationUser
                {
                    UserName       = studentEmail,
                    Email          = studentEmail,
                    FullName       = "Trần Thị Học Sinh",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(student, "Student@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(student, "Customer");
            }

            // Reload references after possible creation
            instructor = await userManager.FindByEmailAsync(instructorEmail);
            student    = await userManager.FindByEmailAsync(studentEmail);

            // ── 5. Categories ─────────────────────────────────────────
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Lập trình Web",     Description = "HTML, CSS, JavaScript, ASP.NET, React, Vue..." },
                    new() { Name = "Cơ sở dữ liệu",     Description = "SQL Server, MySQL, MongoDB, PostgreSQL..." },
                    new() { Name = "Lập trình di động",  Description = "Android, iOS, React Native, Flutter..." },
                    new() { Name = "Trí tuệ nhân tạo",   Description = "Machine Learning, Deep Learning, NLP..." },
                    new() { Name = "DevOps & Cloud",     Description = "Docker, Kubernetes, AWS, Azure, CI/CD..." },
                    new() { Name = "Thiết kế UI/UX",     Description = "Figma, Adobe XD, Design System..." },
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // ── 6. Courses + Modules + Lessons ───────────────────────
            if (!await context.Courses.AnyAsync())
            {
                var catWeb  = await context.Categories.FirstAsync(c => c.Name == "Lập trình Web");
                var catDB   = await context.Categories.FirstAsync(c => c.Name == "Cơ sở dữ liệu");
                var catMobile = await context.Categories.FirstAsync(c => c.Name == "Lập trình di động");

                // Course 1: ASP.NET Core
                var course1 = new Course
                {
                    Title        = "ASP.NET Core MVC - Từ Zero đến Hero",
                    Description  = "Khóa học toàn diện về ASP.NET Core MVC 8. Bạn sẽ xây dựng ứng dụng web chuyên nghiệp với Entity Framework Core, Identity, và SQL Server.",
                    StartDate    = new DateTime(2025, 6, 1),
                    EndDate      = new DateTime(2025, 12, 31),
                    Price        = 0,
                    ImageUrl     = "",
                    IsActive     = true,
                    InstructorId = instructor!.Id,
                    CategoryId   = catWeb.Id,
                    Modules      = new List<Module>
                    {
                        new()
                        {
                            Title       = "Giới thiệu & Cài đặt môi trường",
                            Description = "Làm quen với ASP.NET Core và cài đặt các công cụ cần thiết",
                            Order       = 1,
                            Lessons     = new List<Lesson>
                            {
                                new() { Title = "ASP.NET Core là gì?",           Content = "Giới thiệu tổng quan về ASP.NET Core, lịch sử phát triển và ưu điểm so với ASP.NET Framework.", Order = 1 },
                                new() { Title = "Cài đặt Visual Studio 2022",    Content = "Hướng dẫn tải và cài đặt Visual Studio 2022 Community Edition với workload ASP.NET.", Order = 2 },
                                new() { Title = "Cài đặt SQL Server & SSMS",     Content = "Tải SQL Server Developer Edition và SQL Server Management Studio.", Order = 3 },
                                new() { Title = "Tạo project đầu tiên",          Content = "Tạo project ASP.NET Core MVC mới và tìm hiểu cấu trúc thư mục.", Order = 4 },
                            }
                        },
                        new()
                        {
                            Title       = "MVC Pattern & Routing",
                            Description = "Hiểu sâu về mô hình MVC và cơ chế routing trong ASP.NET Core",
                            Order       = 2,
                            Lessons     = new List<Lesson>
                            {
                                new() { Title = "Mô hình MVC là gì?",            Content = "Phân tích Model - View - Controller và luồng xử lý request.", Order = 1 },
                                new() { Title = "Convention-based Routing",       Content = "Cấu hình routing theo quy ước mặc định và tùy chỉnh.", Order = 2 },
                                new() { Title = "Attribute Routing",              Content = "Sử dụng [Route], [HttpGet], [HttpPost] attribute.", Order = 3 },
                                new() { Title = "Tag Helpers trong Views",        Content = "asp-controller, asp-action, asp-route và các tag helper phổ biến.", Order = 4 },
                                new() { Title = "ViewBag, ViewData, TempData",   Content = "Truyền dữ liệu từ Controller sang View bằng 3 cách khác nhau.", Order = 5 },
                            }
                        },
                        new()
                        {
                            Title       = "Entity Framework Core",
                            Description = "ORM mạnh mẽ để làm việc với cơ sở dữ liệu",
                            Order       = 3,
                            Lessons     = new List<Lesson>
                            {
                                new() { Title = "Cài đặt EF Core packages",      Content = "Cài đặt Microsoft.EntityFrameworkCore.SqlServer và Tools qua NuGet.", Order = 1 },
                                new() { Title = "Code-First Migrations",         Content = "Tạo DbContext, DbSet và chạy Add-Migration, Update-Database.", Order = 2 },
                                new() { Title = "LINQ Queries",                  Content = "Where, OrderBy, Include, Select, FirstOrDefault, ToListAsync.", Order = 3 },
                                new() { Title = "CRUD với EF Core",              Content = "Thêm, sửa, xóa dữ liệu qua context và SaveChangesAsync.", Order = 4 },
                            }
                        },
                    }
                };

                // Course 2: SQL Server
                var course2 = new Course
                {
                    Title        = "SQL Server cơ bản đến nâng cao",
                    Description  = "Nắm vững SQL Server từ cơ bản đến nâng cao. Học cách thiết kế cơ sở dữ liệu, viết truy vấn tối ưu và quản trị SQL Server.",
                    StartDate    = new DateTime(2025, 7, 1),
                    EndDate      = new DateTime(2025, 12, 31),
                    Price        = 199000,
                    ImageUrl     = "",
                    IsActive     = true,
                    InstructorId = instructor.Id,
                    CategoryId   = catDB.Id,
                    Modules      = new List<Module>
                    {
                        new()
                        {
                            Title   = "Cơ bản về SQL",
                            Order   = 1,
                            Lessons = new List<Lesson>
                            {
                                new() { Title = "SELECT, FROM, WHERE",    Content = "Câu lệnh SELECT cơ bản và điều kiện lọc.", Order = 1 },
                                new() { Title = "JOIN các bảng",          Content = "INNER JOIN, LEFT JOIN, RIGHT JOIN, FULL JOIN.", Order = 2 },
                                new() { Title = "GROUP BY & HAVING",      Content = "Nhóm dữ liệu và lọc sau nhóm.", Order = 3 },
                            }
                        },
                        new()
                        {
                            Title   = "Stored Procedures & Functions",
                            Order   = 2,
                            Lessons = new List<Lesson>
                            {
                                new() { Title = "Tạo Stored Procedure",  Content = "CREATE PROCEDURE, EXEC, tham số IN/OUT.", Order = 1 },
                                new() { Title = "Scalar Functions",      Content = "CREATE FUNCTION RETURNS scalar value.", Order = 2 },
                                new() { Title = "Table-Valued Functions", Content = "Trả về tập kết quả dạng bảng.", Order = 3 },
                            }
                        }
                    }
                };

                // Course 3: Flutter
                var course3 = new Course
                {
                    Title        = "Lập trình Flutter - Xây dựng App đa nền tảng",
                    Description  = "Học Flutter từ đầu để xây dựng ứng dụng di động đẹp cho cả Android và iOS chỉ với một codebase Dart.",
                    StartDate    = new DateTime(2025, 8, 1),
                    EndDate      = new DateTime(2026, 6, 30),
                    Price        = 299000,
                    ImageUrl     = "",
                    IsActive     = true,
                    InstructorId = instructor.Id,
                    CategoryId   = catMobile.Id,
                    Modules      = new List<Module>
                    {
                        new()
                        {
                            Title   = "Dart Programming Language",
                            Order   = 1,
                            Lessons = new List<Lesson>
                            {
                                new() { Title = "Dart cơ bản: biến, kiểu dữ liệu", Content = "var, String, int, double, bool, List, Map trong Dart.", Order = 1 },
                                new() { Title = "Hàm và OOP trong Dart",            Content = "Class, constructor, inheritance, mixins.", Order = 2 },
                                new() { Title = "Async/Await và Future",             Content = "Lập trình bất đồng bộ trong Dart.", Order = 3 },
                            }
                        },
                        new()
                        {
                            Title   = "Flutter Widgets cơ bản",
                            Order   = 2,
                            Lessons = new List<Lesson>
                            {
                                new() { Title = "Stateless vs Stateful Widget",  Content = "Phân biệt và khi nào dùng từng loại.", Order = 1 },
                                new() { Title = "Layout Widgets",                Content = "Column, Row, Stack, Container, Expanded.", Order = 2 },
                                new() { Title = "Navigation & Routes",           Content = "Navigator.push, named routes, GoRouter.", Order = 3 },
                            }
                        }
                    }
                };

                await context.Courses.AddRangeAsync(course1, course2, course3);
                await context.SaveChangesAsync();

                // ── 7. Demo Enrollments ──────────────────────────────────
                if (!await context.CourseEnrollments.AnyAsync())
                {
                    var enrollments = new List<CourseEnrollment>
                    {
                        new() { UserId = student.Id, CourseId = course1.Id, EnrollmentDate = DateTime.Now.AddDays(-5), CompletionPercentage = 45 },
                        new() { UserId = student.Id, CourseId = course2.Id, EnrollmentDate = DateTime.Now.AddDays(-2), CompletionPercentage = 10 },
                    };
                    await context.CourseEnrollments.AddRangeAsync(enrollments);
                    
                    // Mark some lessons as complete for course 1
                    var course1Lessons = course1.Modules.SelectMany(m => m.Lessons).Take(3).ToList();
                    foreach(var lesson in course1Lessons)
                    {
                        await context.StudentProgresses.AddAsync(new StudentProgress
                        {
                            UserId = student.Id,
                            LessonId = lesson.Id,
                            IsCompleted = true,
                            CompletedDate = DateTime.Now.AddDays(-3)
                        });
                    }
                    
                    await context.SaveChangesAsync();
                }

                // ── 8. Demo Reviews ──────────────────────────────────────
                if (!await context.CourseReviews.AnyAsync())
                {
                    var reviews = new List<CourseReview>
                    {
                        new() { CourseId = course1.Id, UserId = student.Id, Rating = 5, Comment = "Khóa học rất chi tiết và dễ hiểu. Giảng viên dạy rất nhiệt tình!", CreatedAt = DateTime.Now.AddDays(-2) },
                        new() { CourseId = course2.Id, UserId = student.Id, Rating = 4, Comment = "Nội dung tốt, tuy nhiên phần nâng cao hơi nhanh.", CreatedAt = DateTime.Now.AddDays(-1) }
                    };
                    await context.CourseReviews.AddRangeAsync(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
