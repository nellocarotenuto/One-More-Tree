using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace Back_End.Migrations
{
    public partial class TrackChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(File.ReadAllText(@"Sql\ChangeTrackingEnable.sql"), true);
            migrationBuilder.Sql(File.ReadAllText(@"Sql\TreeSyncStoreProcedure.sql"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
