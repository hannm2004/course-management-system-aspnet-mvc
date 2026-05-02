namespace CourseManagement.Models
{
    public class StudentProgress
    {
        public int Id { get; set; }

        
        public string UserId { get; set; }
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; } 

        
        public ApplicationUser User { get; set; }
        public Lesson Lesson { get; set; }
    }
}
