using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class CourseReview
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
