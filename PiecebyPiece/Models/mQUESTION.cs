using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mQUESTION
    {
        [Key]
        [Required()]
        public int questionID { get; set; }

        public int testID { get; set; }

        [Required, StringLength(200)]
        public string? questionText { get; set; }

        [Required, StringLength(200)]
        public string? choiceA { get; set; }

        [Required, StringLength(200)]
        public string? choiceB { get; set; }

        [StringLength(200)]
        public string? choiceC { get; set; }

        [StringLength(200)]
        public string? choiceD { get; set; }

        [Required, StringLength(1)]
        public string correctAnswer { get; set; }

        [NotMapped]
        public IFormFile? questionPhoto { get; set; }

        public string? questionPhotoPath { get; set; }

        // Navigation
        [ForeignKey("testID")]
        public mTEST? Test { get; set; }
    }
}
