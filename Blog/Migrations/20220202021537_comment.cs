using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog.Migrations
{
    public partial class comment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Comments",
                type: "character varying(69)",
                maxLength: 69,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(69)",
                oldMaxLength: 69,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Comments",
                type: "character varying(69)",
                maxLength: 69,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(69)",
                oldMaxLength: 69);
        }
    }
}
