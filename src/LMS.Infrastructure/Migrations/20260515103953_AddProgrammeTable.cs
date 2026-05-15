using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProgrammeId",
                table: "Courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Programmes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ProgrammeId",
                table: "Courses",
                column: "ProgrammeId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_Code",
                table: "Programmes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Programmes_ProgrammeId",
                table: "Courses",
                column: "ProgrammeId",
                principalTable: "Programmes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Programmes_ProgrammeId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Programmes");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ProgrammeId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ProgrammeId",
                table: "Courses");
        }
    }
}
