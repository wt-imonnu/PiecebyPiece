using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PiecebyPiece.Models
{
    public class mLESSONPROGRESS
    {
        [Key]
        public int id { get; set; }

        public int userID { get; set; }
        public int lessonID { get; set; }
        public int enrollID { get; set; }
        public int courseID { get; set; }

        public bool isPassedTest { get; set; } = false;
        public float progressPercent { get; set; } = 0; // 0–100

        public DateTime lastUpdate { get; set; } = DateTime.Now;

        [ForeignKey("userID")]
        public mUSER? User { get; set; }

        [ForeignKey("lessonID")]
        public mLESSON? Lesson { get; set; }
        [ForeignKey("enrollID")]
        public mENROLLMENT? Enrollment { get; set; }
    }
}
