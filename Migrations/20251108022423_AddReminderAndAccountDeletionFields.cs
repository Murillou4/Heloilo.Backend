using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Heloilo.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderAndAccountDeletionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoodTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    MoodCategory = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ProfilePhotoBlob = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ThemeColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false, defaultValue: "#FF6B9D"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletionRequestedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletionScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WishCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Emoji = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ReminderMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ActivityDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RecurrenceType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    RecurrenceParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    RecurrenceEndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_verification_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_used = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_verification_tokens_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    content_type = table.Column<int>(type: "INTEGER", nullable: false),
                    content_id = table.Column<long>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_favorites_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    NotificationType = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    QuietStartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    QuietEndTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    IntensityLevel = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Normal"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_used = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    used_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationshipInvitations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Pending"),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationshipInvitations", x => x.Id);
                    table.CheckConstraint("CK_Invitation_Users_Different", "SenderId != ReceiverId");
                    table.ForeignKey(
                        name: "FK_RelationshipInvitations_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelationshipInvitations_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relationships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    User1Id = table.Column<long>(type: "INTEGER", nullable: false),
                    User2Id = table.Column<long>(type: "INTEGER", nullable: false),
                    MetDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    MetLocation = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RelationshipStartDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CelebrationType = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Annual"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relationships", x => x.Id);
                    table.CheckConstraint("CK_Relationship_Users_Different", "User1Id != User2Id");
                    table.ForeignKey(
                        name: "FK_Relationships_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Relationships_Users_User2Id",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentStatus = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StatusUpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatuses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    MessageType = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Text"),
                    DeliveryStatus = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Sent"),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InitialSetups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsSkipped = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialSetups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InitialSetups_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InitialSetups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Memories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    MemoryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memories_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoodLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    MoodTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LogDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoodLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoodLogs_MoodTypes_MoodTypeId",
                        column: x => x.MoodTypeId,
                        principalTable: "MoodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoodLogs_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoodLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    NotificationType = table.Column<string>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    RecurrencePattern = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_contents",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    relationship_id = table.Column<long>(type: "INTEGER", nullable: false),
                    content_type = table.Column<int>(type: "INTEGER", nullable: false),
                    content_id = table.Column<long>(type: "INTEGER", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_revoked = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    revoked_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    access_count = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    last_accessed_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shared_contents", x => x.id);
                    table.ForeignKey(
                        name: "FK_shared_contents_Relationships_relationship_id",
                        column: x => x.relationship_id,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoryPages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    PageNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    ImageBlob = table.Column<byte[]>(type: "BLOB", nullable: true),
                    PageDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryPages_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wishes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RelationshipId = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<long>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LinkUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ImageBlob = table.Column<byte[]>(type: "BLOB", nullable: true),
                    ImportanceLevel = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    FulfilledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishes", x => x.Id);
                    table.CheckConstraint("CK_Wish_ImportanceLevel", "ImportanceLevel >= 1 AND ImportanceLevel <= 5");
                    table.ForeignKey(
                        name: "FK_Wishes_Relationships_RelationshipId",
                        column: x => x.RelationshipId,
                        principalTable: "Relationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wishes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wishes_WishCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "WishCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MessageMedia",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChatMessageId = table.Column<long>(type: "INTEGER", nullable: false),
                    FileBlob = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageMedia_ChatMessages_ChatMessageId",
                        column: x => x.ChatMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemoryMedia",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    FileBlob = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemoryMedia_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemoryTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    TagName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemoryTags_Memories_MemoryId",
                        column: x => x.MemoryId,
                        principalTable: "Memories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WishId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishComments_Wishes_WishId",
                        column: x => x.WishId,
                        principalTable: "Wishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MoodTypes",
                columns: new[] { "Id", "CreatedAt", "Description", "Emoji", "IsActive", "MoodCategory", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(5930), "Sensação de leveza ou satisfação", "😊", true, "Positive", "Feliz / Contente", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(5933) },
                    { 2L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7586), "Energia para fazer coisas", "🚀", true, "Positive", "Animado / Motivado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7588) },
                    { 3L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7591), "Paz interior, sem estresse", "😌", true, "Positive", "Calmo / Relaxado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7591) },
                    { 4L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7592), "Quando sente que fez algo legal", "😎", true, "Positive", "Orgulhoso", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7592) },
                    { 5L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7593), "Aprecia o que tem", "🙏", true, "Positive", "Grato / Satisfeito", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7594) },
                    { 6L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7595), "Desânimo ou sofrimento emocional", "😢", true, "Negative", "Triste / Melancólico", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7595) },
                    { 7L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7596), "Raiva ou impaciência", "😠", true, "Negative", "Irritado / Frustrado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7596) },
                    { 8L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7597), "Sensação de tensão ou medo", "😰", true, "Negative", "Ansioso / Preocupado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7598) },
                    { 9L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7599), "Pensamentos sobre erros", "😔", true, "Negative", "Culpado / Arrependido", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7599) },
                    { 10L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7600), "Falta de energia", "😴", true, "Negative", "Cansado / Desmotivado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7600) },
                    { 11L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7601), "Sem estímulo ou interesse", "😐", true, "Neutral", "Entediado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7601) },
                    { 12L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7602), "Não sabe bem o que pensar", "🤔", true, "Neutral", "Confuso / Indeciso", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7603) },
                    { 13L, new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7604), "Mente ativa, querendo descobrir", "🤨", true, "Neutral", "Curioso / Intrigado", new DateTime(2025, 11, 8, 2, 24, 22, 718, DateTimeKind.Utc).AddTicks(7604) }
                });

            migrationBuilder.InsertData(
                table: "WishCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "Emoji", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(4455), "Lugares que o casal quer conhecer juntos", "🌍", true, "Viagem", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(4459) },
                    { 2L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5767), "Coisas que um quer ganhar ou comprar", "🎁", true, "Compras / Presentes", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5769) },
                    { 3L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5771), "Atividades e momentos a dois", "✨", true, "Experiências", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5771) },
                    { 4L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5772), "Objetivos compartilhados", "🎯", true, "Metas do Casal", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5773) },
                    { 5L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5774), "Ideias para o lar", "🏡", true, "Casa e Decoração", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5774) },
                    { 6L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5775), "Planos para aniversários e comemorações", "📅", true, "Datas Especiais", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5775) },
                    { 7L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5776), "Coisas individuais que melhoram o bem-estar", "🧘‍♀️", true, "Auto-cuidado", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5776) },
                    { 8L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5777), "Desejos relacionados a pets", "🐾", true, "Animais de Estimação", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5778) },
                    { 9L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5779), "Sonhos artísticos ou hobbies", "🎨", true, "Projetos Criativos", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5779) },
                    { 10L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5780), "Lugares para comer e receitas", "🍝", true, "Gastronomia", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5780) },
                    { 11L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5781), "Coisas mais distantes ou inspiracionais", "🌠", true, "Sonhos Grandes", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5781) },
                    { 12L, new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5782), "Desejos voltados a ajudar outros", "💗", true, "Doações e Impacto", new DateTime(2025, 11, 8, 2, 24, 22, 717, DateTimeKind.Utc).AddTicks(5783) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_DeliveryStatus",
                table: "ChatMessages",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RelationshipId_SentAt",
                table: "ChatMessages",
                columns: new[] { "RelationshipId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_SentAt",
                table: "ChatMessages",
                columns: new[] { "SenderId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_DeletedAt",
                table: "DailyActivities",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_RecurrenceParentId",
                table: "DailyActivities",
                column: "RecurrenceParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_RecurrenceType",
                table: "DailyActivities",
                column: "RecurrenceType");

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_UserId_ActivityDate",
                table: "DailyActivities",
                columns: new[] { "UserId", "ActivityDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyActivities_UserId_IsCompleted",
                table: "DailyActivities",
                columns: new[] { "UserId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_tokens_expires_at",
                table: "email_verification_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_tokens_token",
                table: "email_verification_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_tokens_token_is_used_expires_at",
                table: "email_verification_tokens",
                columns: new[] { "token", "is_used", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_tokens_user_id",
                table: "email_verification_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_content_type",
                table: "favorites",
                column: "content_type");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_content_type_content_id",
                table: "favorites",
                columns: new[] { "content_type", "content_id" });

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id",
                table: "favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id_content_type_content_id",
                table: "favorites",
                columns: new[] { "user_id", "content_type", "content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialSetups_IsCompleted",
                table: "InitialSetups",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_InitialSetups_RelationshipId_UserId",
                table: "InitialSetups",
                columns: new[] { "RelationshipId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialSetups_UserId",
                table: "InitialSetups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_DeletedAt",
                table: "Memories",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_RelationshipId_MemoryDate",
                table: "Memories",
                columns: new[] { "RelationshipId", "MemoryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MemoryMedia_MemoryId",
                table: "MemoryMedia",
                column: "MemoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryTags_MemoryId_TagName",
                table: "MemoryTags",
                columns: new[] { "MemoryId", "TagName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageMedia_ChatMessageId",
                table: "MessageMedia",
                column: "ChatMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodLogs_MoodTypeId",
                table: "MoodLogs",
                column: "MoodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MoodLogs_RelationshipId_LogDate",
                table: "MoodLogs",
                columns: new[] { "RelationshipId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MoodLogs_UserId_LogDate",
                table: "MoodLogs",
                columns: new[] { "UserId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MoodTypes_MoodCategory_IsActive",
                table: "MoodTypes",
                columns: new[] { "MoodCategory", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MoodTypes_Name",
                table: "MoodTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_NotificationType",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelationshipId_IsRead",
                table: "Notifications",
                columns: new[] { "RelationshipId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_SentAt",
                table: "Notifications",
                columns: new[] { "UserId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_expires_at",
                table: "password_reset_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token",
                table: "password_reset_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token_is_used_expires_at",
                table: "password_reset_tokens",
                columns: new[] { "token", "is_used", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipInvitations_ReceiverId_Status",
                table: "RelationshipInvitations",
                columns: new[] { "ReceiverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RelationshipInvitations_SenderId_Status",
                table: "RelationshipInvitations",
                columns: new[] { "SenderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_IsActive_DeletedAt",
                table: "Relationships",
                columns: new[] { "IsActive", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_RelationshipStartDate",
                table: "Relationships",
                column: "RelationshipStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_User1Id_User2Id_IsActive",
                table: "Relationships",
                columns: new[] { "User1Id", "User2Id", "IsActive" },
                unique: true,
                filter: "IsActive = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_User2Id",
                table: "Relationships",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_IsActive_ReminderDate",
                table: "Reminders",
                columns: new[] { "IsActive", "ReminderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_RelationshipId",
                table: "Reminders",
                column: "RelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserId_ReminderDate",
                table: "Reminders",
                columns: new[] { "UserId", "ReminderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_contents_expires_at",
                table: "shared_contents",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_shared_contents_relationship_id_content_type_content_id",
                table: "shared_contents",
                columns: new[] { "relationship_id", "content_type", "content_id" });

            migrationBuilder.CreateIndex(
                name: "IX_shared_contents_token",
                table: "shared_contents",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shared_contents_token_is_revoked_expires_at",
                table: "shared_contents",
                columns: new[] { "token", "is_revoked", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_StoryPages_RelationshipId_PageDate",
                table: "StoryPages",
                columns: new[] { "RelationshipId", "PageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StoryPages_RelationshipId_PageNumber",
                table: "StoryPages",
                columns: new[] { "RelationshipId", "PageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive_DeletedAt",
                table: "Users",
                columns: new[] { "IsActive", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_StatusUpdatedAt",
                table: "UserStatuses",
                column: "StatusUpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatuses_UserId",
                table: "UserStatuses",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishCategories_IsActive",
                table: "WishCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WishCategories_Name",
                table: "WishCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishComments_UserId",
                table: "WishComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WishComments_WishId_CreatedAt",
                table: "WishComments",
                columns: new[] { "WishId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_CategoryId",
                table: "Wishes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_ImportanceLevel",
                table: "Wishes",
                column: "ImportanceLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_RelationshipId_DeletedAt",
                table: "Wishes",
                columns: new[] { "RelationshipId", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_RelationshipId_Status_DeletedAt",
                table: "Wishes",
                columns: new[] { "RelationshipId", "Status", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_Status",
                table: "Wishes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Wishes_UserId_CreatedAt",
                table: "Wishes",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyActivities");

            migrationBuilder.DropTable(
                name: "email_verification_tokens");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "InitialSetups");

            migrationBuilder.DropTable(
                name: "MemoryMedia");

            migrationBuilder.DropTable(
                name: "MemoryTags");

            migrationBuilder.DropTable(
                name: "MessageMedia");

            migrationBuilder.DropTable(
                name: "MoodLogs");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "RelationshipInvitations");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "shared_contents");

            migrationBuilder.DropTable(
                name: "StoryPages");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropTable(
                name: "WishComments");

            migrationBuilder.DropTable(
                name: "Memories");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "MoodTypes");

            migrationBuilder.DropTable(
                name: "Wishes");

            migrationBuilder.DropTable(
                name: "Relationships");

            migrationBuilder.DropTable(
                name: "WishCategories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
