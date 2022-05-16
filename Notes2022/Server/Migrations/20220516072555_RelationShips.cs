using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notes2022.Server.Migrations
{
    public partial class RelationShips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sequencer_NoteFileId",
                table: "Sequencer",
                column: "NoteFileId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteAccess_NoteFileId",
                table: "NoteAccess",
                column: "NoteFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteAccess_NoteFile_NoteFileId",
                table: "NoteAccess",
                column: "NoteFileId",
                principalTable: "NoteFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteContent_NoteHeader_NoteHeaderId",
                table: "NoteContent",
                column: "NoteHeaderId",
                principalTable: "NoteHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NoteHeader_NoteFile_NoteFileId",
                table: "NoteHeader",
                column: "NoteFileId",
                principalTable: "NoteFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sequencer_NoteFile_NoteFileId",
                table: "Sequencer",
                column: "NoteFileId",
                principalTable: "NoteFile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoteAccess_NoteFile_NoteFileId",
                table: "NoteAccess");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteContent_NoteHeader_NoteHeaderId",
                table: "NoteContent");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteHeader_NoteFile_NoteFileId",
                table: "NoteHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_Sequencer_NoteFile_NoteFileId",
                table: "Sequencer");

            migrationBuilder.DropIndex(
                name: "IX_Sequencer_NoteFileId",
                table: "Sequencer");

            migrationBuilder.DropIndex(
                name: "IX_NoteAccess_NoteFileId",
                table: "NoteAccess");
        }
    }
}
