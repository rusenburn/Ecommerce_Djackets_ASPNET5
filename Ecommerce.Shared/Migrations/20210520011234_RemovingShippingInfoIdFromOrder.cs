using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Shared.Migrations
{
    public partial class RemovingShippingInfoIdFromOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingInfoId",
                table: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShippingInfoId",
                table: "Orders",
                type: "int",
                nullable: true);
        }
    }
}
