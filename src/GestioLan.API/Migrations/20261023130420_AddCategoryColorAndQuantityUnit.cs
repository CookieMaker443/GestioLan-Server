using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestioLan.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryColorAndQuantityUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterDatabase(
                collation: "utf8mb4_unicode_ci",
                oldCollation: "armscii8_general_ci")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8");

            migrationBuilder.AlterTable(
                name: "user")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterTable(
                name: "items")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterTable(
                name: "images")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterTable(
                name: "category")
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "user",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "items",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "image_name",
                table: "items",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "items",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "items",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "id_quantity_unit",
                table: "items",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                table: "images",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "name_category",
                table: "category",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "AssociatedProviderName",
                table: "category",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "armscii8")
                .OldAnnotation("Relational:Collation", "armscii8_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "category_color",
                table: "category",
                type: "varchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#FFFFFF",
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quantity_units",
                columns: table => new
                {
                    id_quantity_unit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    symbol = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quantity_units", x => x.id_quantity_unit);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            // travaso dati: dopo CreateTable, prima di cancellare type_quantity
            // serve per sposare i dati dalla colonna type_quantity della tabella items alla nuova tabella quantity_units e aggiornare la colonna id_quantity_unit nella tabella items con i riferimenti corretti.
            migrationBuilder.Sql(@"
                INSERT INTO quantity_units (name, symbol)
                SELECT DISTINCT type_quantity, LEFT(type_quantity, 16)
                FROM items
                WHERE type_quantity IS NOT NULL AND type_quantity <> '';
            ");

            migrationBuilder.Sql(@"
                UPDATE items i
                JOIN quantity_units u ON u.name = i.type_quantity
                SET i.id_quantity_unit = u.id_quantity_unit;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_items_id_quantity_unit",
                table: "items",
                column: "id_quantity_unit");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Category_Color",
                table: "category",
                sql: "category_color REGEXP '^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$'");

            migrationBuilder.CreateIndex(
                name: "IX_quantity_units_name",
                table: "quantity_units",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_items_quantity_units_id_quantity_unit",
                table: "items",
                column: "id_quantity_unit",
                principalTable: "quantity_units",
                principalColumn: "id_quantity_unit",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "type_quantity",
                table: "items");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_items_quantity_units_id_quantity_unit",
                table: "items");

            migrationBuilder.DropTable(
                name: "quantity_units");

            migrationBuilder.DropIndex(
                name: "IX_items_id_quantity_unit",
                table: "items");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Category_Color",
                table: "category");

            migrationBuilder.DropColumn(
                name: "id_quantity_unit",
                table: "items");

            migrationBuilder.DropColumn(
                name: "category_color",
                table: "category");

            migrationBuilder.AlterDatabase(
                collation: "armscii8_general_ci",
                oldCollation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterTable(
                name: "user")
                .Annotation("MySql:CharSet", "armscii8")
                .Annotation("Relational:Collation", "armscii8_general_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterTable(
                name: "items")
                .Annotation("MySql:CharSet", "armscii8")
                .Annotation("Relational:Collation", "armscii8_general_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterTable(
                name: "images")
                .Annotation("MySql:CharSet", "armscii8")
                .Annotation("Relational:Collation", "armscii8_general_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterTable(
                name: "category")
                .Annotation("MySql:CharSet", "armscii8")
                .Annotation("Relational:Collation", "armscii8_general_ci")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "user",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "user",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "user",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "items",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "image_name",
                table: "items",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64,
                oldNullable: true)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "items",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "items",
                type: "longtext",
                nullable: true,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<string>(
                name: "type_quantity",
                table: "items",
                type: "varchar(45)",
                maxLength: 45,
                nullable: true,
                collation: "armscii8_general_ci")
                .Annotation("MySql:CharSet", "armscii8");

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                table: "images",
                type: "longtext",
                nullable: false,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "name_category",
                table: "category",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "AssociatedProviderName",
                table: "category",
                type: "longtext",
                nullable: true,
                collation: "armscii8_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "armscii8")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");
        }
    }
}
