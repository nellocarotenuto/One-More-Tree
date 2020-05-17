using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;

namespace Back_End.Migrations
{
    public partial class TrackChanges : Migration
    {
        private readonly string _sqlDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}\Sql";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(File.ReadAllText($@"{_sqlDirectory}\ChangeTrackingEnable.sql"), true);
            migrationBuilder.Sql(File.ReadAllText($@"{_sqlDirectory}\TreeSyncStoreProcedure.sql"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
