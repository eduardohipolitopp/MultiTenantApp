using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiTenantApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RulesRemoveTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rules_Tenants_TenantId",
                table: "Rules");

            migrationBuilder.DropIndex(
                name: "IX_Rules_TenantId_Name",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Rules");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_Name",
                table: "Rules",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rules_Name",
                table: "Rules");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Rules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Rules_TenantId_Name",
                table: "Rules",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rules_Tenants_TenantId",
                table: "Rules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
