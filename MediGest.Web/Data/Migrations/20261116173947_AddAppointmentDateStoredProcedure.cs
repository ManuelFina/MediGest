
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediGest.Web.Data.Migrations
{
    public partial class AddAppointmentDateStoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR ALTER PROCEDURE sp_GetAppointmentsByDate
                    @AppointmentDate DATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT Id
                    FROM Appointments
                    WHERE AppointmentDate = @AppointmentDate
                    ORDER BY StartTime;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP PROCEDURE IF EXISTS sp_GetAppointmentsByDate;
                """);
        }
    }
}
