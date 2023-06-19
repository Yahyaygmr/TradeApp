using Microsoft.EntityFrameworkCore.Migrations;

namespace TradeApp.Migrations
{
    public partial class payment_add : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardName",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cvc",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExprationMonth",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExprationYear",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardName",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "Cvc",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "ExprationMonth",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "ExprationYear",
                table: "OrderHeaders");
        }
    }
}
