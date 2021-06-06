﻿using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CleanSolution.Infrastructure.Persistence.Migrations
{
    public partial class initialization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Salary = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrivateNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    Phones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address_Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PictureName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employes_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "Id", "Name", "Salary" },
                values: new object[] { new Guid("15770657-fd70-4f63-b41e-5a9e46fb2708"), "პროგრამისტი", 2000.0 });

            migrationBuilder.InsertData(
                table: "Positions",
                columns: new[] { "Id", "Name", "Salary" },
                values: new object[] { new Guid("f5b8c239-d5c1-42bc-96b5-d7316f93e375"), "ტესტერი", 1000.0 });

            migrationBuilder.CreateIndex(
                name: "IX_Employes_PositionId",
                table: "Employes",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employes_PrivateNumber",
                table: "Employes",
                column: "PrivateNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employes");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
