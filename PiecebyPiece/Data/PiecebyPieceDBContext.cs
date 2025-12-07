using Microsoft.EntityFrameworkCore;

namespace PiecebyPiece.Models
{
    public class PiecebyPieceDBContext : DbContext
    {
        public PiecebyPieceDBContext(DbContextOptions options) : base(options) { }

        public DbSet<mUSER> dUser { get; set; }
        public DbSet<mADMIN> dAdmin { get; set; }
        public DbSet<mCOURSE> dCourse { get; set; }
        public DbSet<mLESSON> dLesson { get; set; }
        public DbSet<mVideoEP> dVideoEP { get; set; }
        public DbSet<mTEST> dTest { get; set; }
        public DbSet<mQUESTION> dQuestion { get; set; }
        public DbSet<mENROLLMENT> dEnrollment { get; set; }
        public DbSet<mCERTIFICATE> dCertificate { get; set; }
        public DbSet<mLESSONPROGRESS> dLessonProgress { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER - ENROLLMENT
            modelBuilder.Entity<mENROLLMENT>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.userID);

            // COURSE - ENROLLMENT
            modelBuilder.Entity<mENROLLMENT>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.courseID);

            // COURSE - LESSON
            modelBuilder.Entity<mLESSON>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.courseID);

            // LESSON - VIDEO
            modelBuilder.Entity<mVideoEP>()
                .HasOne(v => v.Lesson)
                .WithMany(l => l.VideoEPs)
                .HasForeignKey(v => v.lessonID);

            // LESSON - TEST
            modelBuilder.Entity<mTEST>()
                .HasOne(t => t.Lesson)
                .WithOne(l => l.Test)
                .HasForeignKey<mTEST>(t => t.lessonID);

            // TEST - QUESTION
            modelBuilder.Entity<mQUESTION>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.testID);


            // ENROLLMENT - CERTIFICATE
            modelBuilder.Entity<mCERTIFICATE>()
                .HasOne(c => c.Enrollment)
                .WithOne(e => e.Certificate)
                .HasForeignKey<mCERTIFICATE>(c => c.enrollID);

            // User - LessonProgress
            modelBuilder.Entity<mLESSONPROGRESS>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(lp => lp.userID)
                .OnDelete(DeleteBehavior.NoAction);

            // Enrollment - LessonProgress
            modelBuilder.Entity<mLESSONPROGRESS>()
                .HasOne(lp => lp.Enrollment)
                .WithMany()
                .HasForeignKey(lp => lp.enrollID)
                .OnDelete(DeleteBehavior.Cascade);


            // Lesson - LessonProgress
            // [แก้ไขโดยพี่สาว] เพิ่ม Restrict เพื่อแก้ปัญหา Multiple Cascade Paths
            modelBuilder.Entity<mLESSONPROGRESS>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.lessonID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}