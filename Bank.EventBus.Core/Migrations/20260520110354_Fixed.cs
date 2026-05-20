using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.EventBus.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusCollectionsOperations_Operations_OperationId",
                table: "BusCollectionsOperations");

            migrationBuilder.DropIndex(
                name: "IX_BusCollectionsOperations_OperationId",
                table: "BusCollectionsOperations");

            migrationBuilder.DropColumn(
                name: "OperationId",
                table: "BusCollectionsOperations");

            migrationBuilder.CreateIndex(
                name: "IX_BusCollectionsOperations_BusOperationId",
                table: "BusCollectionsOperations",
                column: "BusOperationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusCollectionsOperations_Operations_BusOperationId",
                table: "BusCollectionsOperations",
                column: "BusOperationId",
                principalTable: "Operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusCollectionsOperations_Operations_BusOperationId",
                table: "BusCollectionsOperations");

            migrationBuilder.DropIndex(
                name: "IX_BusCollectionsOperations_BusOperationId",
                table: "BusCollectionsOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "OperationId",
                table: "BusCollectionsOperations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BusCollectionsOperations_OperationId",
                table: "BusCollectionsOperations",
                column: "OperationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusCollectionsOperations_Operations_OperationId",
                table: "BusCollectionsOperations",
                column: "OperationId",
                principalTable: "Operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
