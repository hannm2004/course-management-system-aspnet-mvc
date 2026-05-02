using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class Category
    {
        public int Id { get; set; }  

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }  

        [StringLength(500)]
        public string Description { get; set; }  


        public ICollection<Course> Courses { get; set; }  
    }
}
