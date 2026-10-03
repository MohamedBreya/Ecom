using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.infrastructure.Data.Migrations
{
    /// <summary>
    /// The earlier migrations created <c>Products.Price decimal(18,0)</c> and the "updatepricesuccess" migration was
    /// empty, so the schema never matched the model (<c>NewPrice</c>/<c>OldPrice decimal(18,2)</c>). This migration
    /// brings any database - fresh or one that was patched by hand - to the model. It is idempotent.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261003000000_FixPriceColumns")]
    public partial class FixPriceColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Products', 'Price') IS NOT NULL AND COL_LENGTH('Products', 'NewPrice') IS NULL
    EXEC sp_rename 'Products.Price', 'NewPrice', 'COLUMN';");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Products', 'OldPrice') IS NULL
    EXEC('ALTER TABLE [Products] ADD [OldPrice] decimal(18,2) NOT NULL CONSTRAINT [DF_Products_OldPrice] DEFAULT 0');");

            foreach (var column in new[] { "NewPrice", "OldPrice" })
            {
                migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = '{column}'
             AND NOT (NUMERIC_PRECISION = 18 AND NUMERIC_SCALE = 2))
    EXEC('ALTER TABLE [Products] ALTER COLUMN [{column}] decimal(18,2) NOT NULL');");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty: reverting would drop price data and the previous schema was inconsistent.
        }
    }
}
