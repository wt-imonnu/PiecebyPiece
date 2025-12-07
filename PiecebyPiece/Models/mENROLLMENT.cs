using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mENROLLMENT
    {
        [Key]
        public int enrollID { get; set; }

        public DateTime enrollTime { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string enrollStatus { get; set; } = "In Progress";

        public int userID { get; set; }

        public int courseID { get; set; }

        [StringLength(100)]
        public string? courseName { get; set; }

        // Navigation
        [ForeignKey("userID")]
        public mUSER? User { get; set; }
        [ForeignKey("courseID")]
        public mCOURSE? Course { get; set; }
        public mCERTIFICATE? Certificate { get; set; }
    }
}
