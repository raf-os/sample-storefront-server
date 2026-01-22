using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_storefront_server.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPropertiesToUserMailJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MailId",
                table: "UserMails",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails",
                column: "MailId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMails_Mails_MailId",
                table: "UserMails",
                column: "MailId",
                principalTable: "Mails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMails_Mails_MailId",
                table: "UserMails");

            migrationBuilder.DropIndex(
                name: "IX_UserMails_MailId",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "MailId",
                table: "UserMails");
        }
    }
}
