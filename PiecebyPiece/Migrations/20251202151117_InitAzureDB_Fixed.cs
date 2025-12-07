using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PiecebyPiece.Migrations
{
    /// <inheritdoc />
    public partial class InitAzureDB_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dAdmin",
                columns: table => new
                {
                    adminID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    adminName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    adminSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    adminEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    adminPassword = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dAdmin", x => x.adminID);
                });

            migrationBuilder.CreateTable(
                name: "dCourse",
                columns: table => new
                {
                    courseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    courseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    courseSubject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    courseDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    coursePhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dCourse", x => x.courseID);
                });

            migrationBuilder.CreateTable(
                name: "dUser",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    userSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    userEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userPassword = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    userPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dUser", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "dLesson",
                columns: table => new
                {
                    lessonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    courseID = table.Column<int>(type: "int", nullable: false),
                    lessonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lessonDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lessonProgress = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dLesson", x => x.lessonID);
                    table.ForeignKey(
                        name: "FK_dLesson_dCourse_courseID",
                        column: x => x.courseID,
                        principalTable: "dCourse",
                        principalColumn: "courseID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dEnrollment",
                columns: table => new
                {
                    enrollID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    enrollStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    userID = table.Column<int>(type: "int", nullable: false),
                    courseID = table.Column<int>(type: "int", nullable: false),
                    courseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dEnrollment", x => x.enrollID);
                    table.ForeignKey(
                        name: "FK_dEnrollment_dCourse_courseID",
                        column: x => x.courseID,
                        principalTable: "dCourse",
                        principalColumn: "courseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dEnrollment_dUser_userID",
                        column: x => x.userID,
                        principalTable: "dUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dTest",
                columns: table => new
                {
                    testID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lessonID = table.Column<int>(type: "int", nullable: false),
                    testScore = table.Column<float>(type: "real", nullable: false),
                    mLESSON = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dTest", x => x.testID);
                    table.ForeignKey(
                        name: "FK_dTest_dLesson_lessonID",
                        column: x => x.lessonID,
                        principalTable: "dLesson",
                        principalColumn: "lessonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dVideoEP",
                columns: table => new
                {
                    vdoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lessonID = table.Column<int>(type: "int", nullable: false),
                    vdoName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    vdoFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    epFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    epProgress = table.Column<float>(type: "real", nullable: false),
                    mLESSON = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dVideoEP", x => x.vdoID);
                    table.ForeignKey(
                        name: "FK_dVideoEP_dLesson_lessonID",
                        column: x => x.lessonID,
                        principalTable: "dLesson",
                        principalColumn: "lessonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dCertificate",
                columns: table => new
                {
                    cerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cerDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    enrollID = table.Column<int>(type: "int", nullable: false),
                    courseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    userName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    userSurname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    userID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dCertificate", x => x.cerID);
                    table.ForeignKey(
                        name: "FK_dCertificate_dEnrollment_enrollID",
                        column: x => x.enrollID,
                        principalTable: "dEnrollment",
                        principalColumn: "enrollID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dCertificate_dUser_userID",
                        column: x => x.userID,
                        principalTable: "dUser",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "dLessonProgress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    lessonID = table.Column<int>(type: "int", nullable: false),
                    enrollID = table.Column<int>(type: "int", nullable: false),
                    courseID = table.Column<int>(type: "int", nullable: false),
                    isPassedTest = table.Column<bool>(type: "bit", nullable: false),
                    progressPercent = table.Column<float>(type: "real", nullable: false),
                    lastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dLessonProgress", x => x.id);
                    table.ForeignKey(
                        name: "FK_dLessonProgress_dEnrollment_enrollID",
                        column: x => x.enrollID,
                        principalTable: "dEnrollment",
                        principalColumn: "enrollID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dLessonProgress_dLesson_lessonID",
                        column: x => x.lessonID,
                        principalTable: "dLesson",
                        principalColumn: "lessonID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dLessonProgress_dUser_userID",
                        column: x => x.userID,
                        principalTable: "dUser",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "dQuestion",
                columns: table => new
                {
                    questionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    testID = table.Column<int>(type: "int", nullable: false),
                    questionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    choiceA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    choiceB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    choiceC = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    choiceD = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    correctAnswer = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    questionPhotoPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dQuestion", x => x.questionID);
                    table.ForeignKey(
                        name: "FK_dQuestion_dTest_testID",
                        column: x => x.testID,
                        principalTable: "dTest",
                        principalColumn: "testID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dCertificate_enrollID",
                table: "dCertificate",
                column: "enrollID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dCertificate_userID",
                table: "dCertificate",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_dEnrollment_courseID",
                table: "dEnrollment",
                column: "courseID");

            migrationBuilder.CreateIndex(
                name: "IX_dEnrollment_userID",
                table: "dEnrollment",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_dLesson_courseID",
                table: "dLesson",
                column: "courseID");

            migrationBuilder.CreateIndex(
                name: "IX_dLessonProgress_enrollID",
                table: "dLessonProgress",
                column: "enrollID");

            migrationBuilder.CreateIndex(
                name: "IX_dLessonProgress_lessonID",
                table: "dLessonProgress",
                column: "lessonID");

            migrationBuilder.CreateIndex(
                name: "IX_dLessonProgress_userID",
                table: "dLessonProgress",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_dQuestion_testID",
                table: "dQuestion",
                column: "testID");

            migrationBuilder.CreateIndex(
                name: "IX_dTest_lessonID",
                table: "dTest",
                column: "lessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dVideoEP_lessonID",
                table: "dVideoEP",
                column: "lessonID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dAdmin");

            migrationBuilder.DropTable(
                name: "dCertificate");

            migrationBuilder.DropTable(
                name: "dLessonProgress");

            migrationBuilder.DropTable(
                name: "dQuestion");

            migrationBuilder.DropTable(
                name: "dVideoEP");

            migrationBuilder.DropTable(
                name: "dEnrollment");

            migrationBuilder.DropTable(
                name: "dTest");

            migrationBuilder.DropTable(
                name: "dUser");

            migrationBuilder.DropTable(
                name: "dLesson");

            migrationBuilder.DropTable(
                name: "dCourse");
        }
    }
}
