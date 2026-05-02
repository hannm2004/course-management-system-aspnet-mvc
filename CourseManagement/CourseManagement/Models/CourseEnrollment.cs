using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class CourseEnrollment
    {
        public int Id { get; set; }

        
        public string UserId { get; set; }
        public int CourseId { get; set; }

        [Display(Name = "Ngày đăng ký")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã hoàn thành")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Tiến độ (%)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal CompletionPercentage { get; set; } = 0;

        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
