using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace PiecebyPiece.Models
{
    public class mCOURSE
    {
        [Key]
        [Required()]
        public int courseID { get; set; }

        [Required, StringLength(100)]
        public string? courseName { get; set; }

        [Required]
        [StringLength(100)]
        public string? courseSubject { get; set; }

        public string? courseDescription { get; set; }


        [Display(Name = "Upload Course Photo")]
        [NotMapped]
        public IFormFile? coursePhoto { get; set; }

        [Display(Name = "Photo Path")]
        public string? coursePhotoPath { get; set; }

        public bool IsCreate => courseID == 0;   // หรือ courseId, courseID แล้วแต่ชื่อ



        // Navigation
        public ICollection<mLESSON>? Lessons { get; set; }
        public ICollection<mENROLLMENT>? Enrollments { get; set; }
    }
}
