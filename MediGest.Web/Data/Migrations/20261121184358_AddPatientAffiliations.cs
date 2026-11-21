using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediGest.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientAffiliations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_HealthPlans_HealthInsuranceId_Id",
                table: "HealthPlans",
                columns: new[] { "HealthInsuranceId", "Id" });

            migrationBuilder.CreateTable(
                name: "PatientAffiliations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    HealthInsuranceId = table.Column<int>(type: "int", nullable: false),
                    HealthPlanId = table.Column<int>(type: "int", nullable: false),
                    MembershipNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MemberType = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAffiliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientAffiliations_HealthInsurances_HealthInsuranceId",
                        column: x => x.HealthInsuranceId,
                        principalTable: "HealthInsurances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientAffiliations_HealthPlans_HealthInsuranceId_HealthPlanId",
                        columns: x => new { x.HealthInsuranceId, x.HealthPlanId },
                        principalTable: "HealthPlans",
                        principalColumns: new[] { "HealthInsuranceId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientAffiliations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAffiliations_HealthInsuranceId",
                table: "PatientAffiliations",
                column: "HealthInsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAffiliations_HealthInsuranceId_HealthPlanId",
                table: "PatientAffiliations",
                columns: new[] { "HealthInsuranceId", "HealthPlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAffiliations_HealthInsuranceId_MembershipNumber",
                table: "PatientAffiliations",
                columns: new[] { "HealthInsuranceId", "MembershipNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAffiliations_HealthPlanId",
                table: "PatientAffiliations",
                column: "HealthPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAffiliations_PatientId",
                table: "PatientAffiliations",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientAffiliations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_HealthPlans_HealthInsuranceId_Id",
                table: "HealthPlans");
        }
    }
}
