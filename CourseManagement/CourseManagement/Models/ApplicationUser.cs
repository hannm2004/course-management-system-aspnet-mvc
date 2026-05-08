using Microsoft.AspNetCore.Identity;

namespace CourseManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }  
        public string? Address { get; set; }  

        public ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
        public ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();
    }
}
