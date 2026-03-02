using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bacera.Gateway.Data.Migrations.TenantDb
{
    /// <inheritdoc />
    public partial class chatschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.CreateTable(
                name: "_Chat",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MetaData = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatorPartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_Chat_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_Chat__Creator_Party_Id_fk",
                        column: x => x.CreatorPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_ChatMessage",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    SenderPartyId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Content = table.Column<string>(type: "text", nullable: false, defaultValueSql: "''::text"),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ChatMessage_pk", x => x.Id);
                    table.ForeignKey(
                        name: "_ChatMessage__Chat_Id_fk",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "_Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_ChatMessage__Sender_Party_Id_fk",
                        column: x => x.SenderPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_ChatParticipant",
                schema: "chat",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    PartyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ChatParticipant_pk", x => new { x.ChatId, x.PartyId });
                    table.ForeignKey(
                        name: "_ChatParticipant__Chat_Id_fk",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "_Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_ChatParticipant__Party_Id_fk",
                        column: x => x.PartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "_ChatMessageInbox",
                schema: "chat",
                columns: table => new
                {
                    ReceiverPartyId = table.Column<long>(type: "bigint", nullable: false),
                    ChatMessageId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("_ChatMessageInbox_pk", x => new { x.ReceiverPartyId, x.ChatMessageId });
                    table.ForeignKey(
                        name: "_ChatMessageInbox__ChatMessage_Id_fk",
                        column: x => x.ChatMessageId,
                        principalSchema: "chat",
                        principalTable: "_ChatMessage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "_ChatMessageInbox__Receiver_Party_Id_fk",
                        column: x => x.ReceiverPartyId,
                        principalSchema: "core",
                        principalTable: "_Party",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__Chat_CreatedOn",
                schema: "chat",
                table: "_Chat",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__Chat_CreatorPartyId",
                schema: "chat",
                table: "_Chat",
                column: "CreatorPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__Chat_Status",
                schema: "chat",
                table: "_Chat",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX__ChatMessage_ChatId",
                schema: "chat",
                table: "_ChatMessage",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX__ChatMessage_CreatedOn",
                schema: "chat",
                table: "_ChatMessage",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX__ChatMessage_SenderPartyId",
                schema: "chat",
                table: "_ChatMessage",
                column: "SenderPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__ChatMessageInbox_ChatMessageId",
                schema: "chat",
                table: "_ChatMessageInbox",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX__ChatMessageInbox_ReceiverPartyId",
                schema: "chat",
                table: "_ChatMessageInbox",
                column: "ReceiverPartyId");

            migrationBuilder.CreateIndex(
                name: "IX__ChatParticipant_ChatId",
                schema: "chat",
                table: "_ChatParticipant",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX__ChatParticipant_PartyId",
                schema: "chat",
                table: "_ChatParticipant",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_ChatMessageInbox",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "_ChatParticipant",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "_ChatMessage",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "_Chat",
                schema: "chat");
        }
    }
}
