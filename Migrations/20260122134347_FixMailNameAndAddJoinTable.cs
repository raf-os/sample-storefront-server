using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sample_storefront_server.Migrations
{
    /// <inheritdoc />
    public partial class FixMailNameAndAddJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMails",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "ExtraRecipients",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "SendDate",
                table: "UserMails");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserMails");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMails",
                table: "UserMails",
                columns: new[] { "SenderId", "RecipientId" });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipientId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mails_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mails_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMails_RecipientId",
                table: "UserMails",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_RecipientId",
                table: "Mails",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_SenderId",
                table: "Mails",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMails_Users_RecipientId",
                table: "UserMails",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMails_Users_SenderId",
                table: "UserMails",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMails_Users_RecipientId",
                table: "UserMails");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMails_Users_SenderId",
                table: "UserMails");

            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMails",
                table: "UserMails");

            migrationBuilder.DropIndex(
                name: "IX_UserMails_RecipientId",
                table: "UserMails");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserMails",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "UserMails",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExtraRecipients",
                table: "UserMails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "UserMails",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendDate",
                table: "UserMails",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserMails",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMails",
                table: "UserMails",
                column: "Id");
        }
    }
}
