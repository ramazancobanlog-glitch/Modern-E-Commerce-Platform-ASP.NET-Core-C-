using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationCodeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verificationCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsEmailConfirmed", "VerificationCodeExpiresAt", "verificationCode" },
                values: new object[] { true, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "verificationCode",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsEmailConfirmed",
                value: false);
        }
    }
}
