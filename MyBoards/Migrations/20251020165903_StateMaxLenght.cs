using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyBoards.Migrations
{
    /// <inheritdoc />
    public partial class StateMaxLenght : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "States");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "States",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "States");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "States",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
