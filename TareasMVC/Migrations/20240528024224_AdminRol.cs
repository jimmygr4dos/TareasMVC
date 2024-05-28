using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql
                 (@"IF NOT EXISTS(SELECT Id FROM AspNetRoles WHERE Id = 'b1f4e139-f940-4927-bbc9-1448f6b3de01')
                    BEGIN
	                    INSERT INTO AspNetRoles (Id, [Name], [NormalizedName])
	                    VALUES ('b1f4e139-f940-4927-bbc9-1448f6b3de01', 'admin', 'ADMIN')
                    END"
                 );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE AspNetRoles WHERE Id = 'b1f4e139-f940-4927-bbc9-1448f6b3de01')");
        }
    }
}
