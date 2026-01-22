using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_storefront_server.Migrations
{
    /// <inheritdoc />
    public partial class MailUserMailRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails");

            migrationBuilder.CreateIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails",
                column: "MailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails");

            migrationBuilder.CreateIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails",
                column: "MailId");
        }
    }
}
