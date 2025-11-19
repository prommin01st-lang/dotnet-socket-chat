using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReadTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadTimestamp",
                table: "ConversationParticipants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadTimestamp",
                table: "ConversationParticipants");
        }
    }
}
