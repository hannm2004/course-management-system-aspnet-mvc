using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourseManagement.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
        [StringLength(100, ErrorMessage = "Tên khóa học không được vượt quá 100 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mô tả khóa học")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, 100000000, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }
        
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
