using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AspNetCoreTodo.Data.Migrations
{
    public partial class AddAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedTaskDate",
                table: "Items",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTaskDate",
                table: "Items",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserCreateTask",
                table: "Items",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedTaskDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CreationTaskDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UserCreateTask",
                table: "Items");
        }
    }
}
