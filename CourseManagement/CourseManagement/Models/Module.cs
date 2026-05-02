using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Module
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Display(Name = "Thứ tự")]
        public int Order { get; set; }  

        
        public int CourseId { get; set; }

        
        public Course Course { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}
