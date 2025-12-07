using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mTEST
    {
        [Key]
        [Required()]
        public int testID { get; set; }
        public int lessonID { get; set; }
        public float testScore { get; set; } = 0;

        // Navigation
        [ForeignKey("mLESSON")]
        public mLESSON? Lesson { get; set; }
        public ICollection<mQUESTION>? Questions { get; set; }
    }
}
