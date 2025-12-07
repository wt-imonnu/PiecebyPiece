using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace PiecebyPiece.Models
{
    public class mCERTIFICATE
    {
        [Key]
        public int cerID { get; set; }
        public DateTime cerDate { get; set; } = DateTime.Now;
        public int enrollID { get; set; }
        [StringLength(100)]
        public string? courseName { get; set; }
        [StringLength(50)]
        public string? userName { get; set; }

        [StringLength(50)]
        public string? userSurname { get; set; }

        // Navigation
        [ForeignKey("enrollID")]
        public mENROLLMENT? Enrollment { get; set; }
        [ForeignKey("userID")]
        public mUSER? User { get; set; }
    }
}
