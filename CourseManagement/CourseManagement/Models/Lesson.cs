using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Content { get; set; }  

        [Display(Name = "Video URL")]
        [DataType(DataType.Url)]
        public string VideoUrl { get; set; }  

        [Required]
        public int Order { get; set; }

        
        public int ModuleId { get; set; }

        
        public Module Module { get; set; }
        public ICollection<StudentProgress> StudentProgresses { get; set; }
    }
}
