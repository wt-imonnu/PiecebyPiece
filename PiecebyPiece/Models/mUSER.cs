using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace PiecebyPiece.Models
{
    public class mUSER
    {
        [Key]
        [Required()]
        public int userID { get; set; }

        [Required(ErrorMessage = "Please enter your name"), StringLength(50)]
        public string userName { get; set; }

        [Required(ErrorMessage = "Please enter your surname"), StringLength(50)]
        public string userSurname { get; set; }

        [Required(ErrorMessage = "Please enter your email"), EmailAddress]
        public string userEmail { get; set; }

        [Required(ErrorMessage = "Please create your password"), DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters")]
        public string userPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("userPassword", ErrorMessage = "Incorrect password")]
        [NotMapped]
        public string userConfirmPassword { get; set; }

        [Display(Name = "Upload Your Profile")]
        [NotMapped]
        public IFormFile? userPhoto { get; set; }

        public bool IsCreate => userID == 0;

        [Display(Name = "Photo Path")]
        public string? userPhotoPath { get; set; }





        // Navigation
        public ICollection<mENROLLMENT>? Enrollments { get; set; }
        public ICollection<mCERTIFICATE>? Certificates { get; set; }
        public ICollection<mLESSONPROGRESS>? LessonProgresses { get; set; }

    }
}
