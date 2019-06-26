using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSocialNetwork.Data.Migrations
{
    public partial class AddImageGallery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GalleryImageId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GalleryImages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<string>(nullable: true),
                    Image = table.Column<byte[]>(nullable: true),
                    ApplicationUserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryImages_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GalleryImages_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GalleryImageId",
                table: "AspNetUsers",
                column: "GalleryImageId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_ApplicationUserId",
                table: "GalleryImages",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_OwnerId",
                table: "GalleryImages",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_GalleryImages_GalleryImageId",
                table: "AspNetUsers",
                column: "GalleryImageId",
                principalTable: "GalleryImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_GalleryImages_GalleryImageId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GalleryImages");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GalleryImageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GalleryImageId",
                table: "AspNetUsers");
        }
    }
}
