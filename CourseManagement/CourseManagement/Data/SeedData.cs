using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CourseManagement.Models;

namespace CourseManagement.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            Console.WriteLine(">>> Starting SeedData Initialization...");
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ── 1. Roles ──────────────────────────────────────────────
            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"> Role '{role}' created.");
                }
            }

            // ── 2. Admin user ─────────────────────────────────────────
            const string adminEmail = "admin@edumanage.vn";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("> Admin user created.");
                }
            }

            // ── 3. Demo Instructor ────────────────────────────────────
            const string instructorEmail = "instructor@edumanage.vn";
            var instructor = await userManager.FindByEmailAsync(instructorEmail);
            if (instructor == null)
            {
                instructor = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    FullName = "Nguyễn Văn Giảng",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(instructor, "Instructor@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(instructor, "Instructor");
                    Console.WriteLine("> Instructor user created.");
                }
            }

            // ── 4. Demo Student ───────────────────────────────────────
            const string studentEmail = "student@edumanage.vn";
            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                student = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "Trần Thị Học Sinh",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(student, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                    Console.WriteLine("> Student user created.");
                }
            }

            // Reload references after possible creation
            instructor = await userManager.FindByEmailAsync(instructorEmail);
            student = await userManager.FindByEmailAsync(studentEmail);

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
                Console.WriteLine("> Categories created.");
            }

            // ── 6. Courses + Modules + Lessons ───────────────────────
            try
            {
                if (!await context.Courses.AnyAsync())
                {
                    Console.WriteLine("> Seeding Courses...");
                    var catWeb = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Lập trình Web");
                    var catDB = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Cơ sở dữ liệu");
                    
                    if (catWeb != null && instructor != null)
                    {
                        var course1 = new Course
                        {
                            Title = "ASP.NET Core MVC - Từ Zero đến Hero",
                            Description = "Khóa học toàn diện về ASP.NET Core MVC 8. Bạn sẽ xây dựng ứng dụng web chuyên nghiệp với Entity Framework Core, Identity, và SQL Server.",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddMonths(6),
                            Price = 0,
                            IsActive = true,
                            InstructorId = instructor.Id,
                            CategoryId = catWeb.Id,
                            Modules = new List<Module>
                            {
                                new() { 
                                    Title = "Giới thiệu & Cài đặt", Order = 1,
                                    Lessons = new List<Lesson> {
                                        new() { Title = "Chào mừng đến với khóa học", Content = "<p>Chào mừng bạn!</p>", Order = 1 },
                                        new() { Title = "Cài đặt môi trường", Content = "<p>Hướng dẫn cài đặt VS 2022.</p>", Order = 2 }
                                    }
                                }
                            }
                        };
                        context.Courses.Add(course1);
                        Console.WriteLine("> Course 1 added.");
                    }

                    if (catDB != null && instructor != null)
                    {
                        var course2 = new Course
                        {
                            Title = "SQL Server cơ bản",
                            Description = "Học cách quản lý và truy vấn dữ liệu với SQL Server.",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddMonths(3),
                            Price = 199000,
                            IsActive = true,
                            InstructorId = instructor.Id,
                            CategoryId = catDB.Id
                        };
                        context.Courses.Add(course2);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception) { /* Log error if needed */ }
        }
    }
}
