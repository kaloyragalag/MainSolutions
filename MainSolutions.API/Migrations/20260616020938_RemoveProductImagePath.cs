using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainSolutions.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EntityImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EntityImages",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EntityImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EntityImages");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
