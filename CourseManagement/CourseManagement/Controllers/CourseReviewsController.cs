using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    [Authorize]
    public class CourseReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Rating,Comment")] CourseReview review)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                
                // Check if user is enrolled in the course
                var isEnrolled = await _context.CourseEnrollments
                    .AnyAsync(e => e.CourseId == review.CourseId && e.UserId == userId);

                if (!isEnrolled)
                {
                    return BadRequest("Bạn phải đăng ký khóa học này để có thể đánh giá.");
                }

                // Check if user already reviewed
                var existingReview = await _context.CourseReviews
                    .FirstOrDefaultAsync(r => r.CourseId == review.CourseId && r.UserId == userId);

                if (existingReview != null)
                {
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    existingReview.CreatedAt = DateTime.Now;
                    _context.Update(existingReview);
                }
                else
                {
                    review.UserId = userId;
                    review.CreatedAt = DateTime.Now;
                    _context.Add(review);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Courses", new { id = review.CourseId });
            }
            return RedirectToAction("Details", "Courses", new { id = review.CourseId });
        }
    }
}
