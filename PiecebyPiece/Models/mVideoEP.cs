using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mVideoEP
    {
        [Key]
        [Required()]
        public int vdoID { get; set; }

        
        public int lessonID { get; set; }

        [Required(ErrorMessage = "Please create video name")]
        [Display(Name = "Video Name")]
        public string vdoName { get; set; }


        public string? vdoFilePath { get; set; } // เก็บลิงก์ YouTube เช่น https://www.youtube.com/watch?v=abc123


        public string? epFilePath { get; set; }  // เก็บ path ของไฟล์ pdf 

        public float epProgress { get; set; }    // 0-100%

        // FK to Lesson
        [ForeignKey("mLESSON")]
        public mLESSON? Lesson { get; set; }
    }
}