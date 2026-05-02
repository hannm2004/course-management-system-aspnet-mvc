using Microsoft.AspNetCore.Identity;

namespace CourseManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        
        public string FullName { get; set; }  
        public string Address { get; set; }  

        public ICollection<CourseEnrollment> CourseEnrollments { get; set; }  
        public ICollection<Course> CreatedCourses { get; set; }  
        public ICollection<StudentProgress> StudentProgresses { get; set; }  
    }
}
