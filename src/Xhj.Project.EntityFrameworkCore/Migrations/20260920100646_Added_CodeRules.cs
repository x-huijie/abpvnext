using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xhj.Project.Migrations
{
    /// <inheritdoc />
    public partial class Added_CodeRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCodeRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DateFormat = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Separator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    SerialLength = table.Column<int>(type: "int", nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    CurrentSerial = table.Column<long>(type: "bigint", nullable: false),
                    ResetMode = table.Column<int>(type: "int", nullable: false),
                    LastResetKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCodeRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCodeRules_Code",
                table: "AppCodeRules",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCodeRules");
        }
    }
}
