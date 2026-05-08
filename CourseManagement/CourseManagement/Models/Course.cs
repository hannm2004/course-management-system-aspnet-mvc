using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CourseManagement.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề khóa học không được để trống")]
        [StringLength(200)]
        public string Title { get; set; }  

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }  

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Ngày kết thúc")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]  
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; } 

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true; 

    
        public string InstructorId { get; set; }  
        public int CategoryId { get; set; }  

        
        [ForeignKey("InstructorId")]
        public ApplicationUser Instructor { get; set; } 

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }  

        public ICollection<Module> Modules { get; set; }  
        public ICollection<CourseEnrollment> CourseEnrollments { get; set; }
        public ICollection<CourseReview> CourseReviews { get; set; }
    }
}
