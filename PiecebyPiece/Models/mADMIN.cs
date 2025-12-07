using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mADMIN
    {
        [Key]
        [Required()]
        public int adminID { get; set; }

        [Required, StringLength(50)]
        public string adminName { get; set; }

        [Required, StringLength(50)]
        public string adminSurname { get; set; }

        [Required, EmailAddress]
        public string adminEmail { get; set; }

        [Required(ErrorMessage = "Please create your password"), DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters")]
        public string adminPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Display(Name = "Confirm password")]
        [DataType(DataType.Password)]
        [Compare("adminPassword", ErrorMessage = "Incorrect password")]
        [NotMapped]
        public string adminConfirmPassword { get; set; }

    }
}
