using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertOrderEnumsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // أولاً: نحول القيم القديمة (string) إلى int
            migrationBuilder.Sql(@"
        UPDATE Orders 
        SET Status = CASE Status
            WHEN 'Pending' THEN 0
            WHEN 'Processing' THEN 1
            WHEN 'Completed' THEN 2
            WHEN 'Cancelled' THEN 3
            ELSE 0 -- default
        END,
        PaymentStatus = CASE PaymentStatus
            WHEN 'Paid' THEN 0
            WHEN 'Unpaid' THEN 1
            WHEN 'Failed' THEN 2
            ELSE 1 -- default Unpaid
        END");

            // بعد كده نغير نوع العمود
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // لو عايز ترجع القيم لـ string (اختياري)
            migrationBuilder.Sql(@"
        UPDATE Orders 
        SET Status = CASE Status
            WHEN 0 THEN 'Pending'
            WHEN 1 THEN 'Processing'
            WHEN 2 THEN 'Completed'
            WHEN 3 THEN 'Cancelled'
            ELSE 'Pending'
        END,
        PaymentStatus = CASE PaymentStatus
            WHEN 0 THEN 'Paid'
            WHEN 1 THEN 'Unpaid'
            WHEN 2 THEN 'Failed'
            ELSE 'Unpaid'
        END");
        }
    }
}
