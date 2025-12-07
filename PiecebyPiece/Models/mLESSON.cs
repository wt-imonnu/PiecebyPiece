using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mLESSON
    {
        [Key]
        [Required()]
        public int lessonID { get; set; }

        [ForeignKey("mCOURSE")]
        public int courseID { get; set; }

        [Required, StringLength(100)]
        public string lessonName { get; set; }

        public string? lessonDescription { get; set; }

        public float lessonProgress { get; set; } = 0;


        // Navigation
        public mCOURSE? Course { get; set; }
        public ICollection<mVideoEP>? VideoEPs { get; set; }
        public mTEST? Test { get; set; }
        public ICollection<mLESSONPROGRESS>? LessonProgresses { get; set; }
    }
}
