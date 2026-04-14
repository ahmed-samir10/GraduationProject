using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonSanad.Repository.Migrations.KonSanadDb
{
    /// <inheritdoc />
    public partial class newmig : Migration
    {
   
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                // ✅ VolunteerMissions.Notes
                migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VolunteerMissions')
                AND NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'VolunteerMissions' AND COLUMN_NAME = 'Notes')
                BEGIN
                    ALTER TABLE [VolunteerMissions] ADD [Notes] nvarchar(max) NULL
                END
            ");

                // ✅ OrderItems.Status string -> int NULL
                migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'Status')
                BEGIN
                    DECLARE @varOS sysname;
                    SELECT @varOS = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderItems]') AND [c].[name] = N'Status');
                    IF @varOS IS NOT NULL EXEC(N'ALTER TABLE [OrderItems] DROP CONSTRAINT [' + @varOS + '];');
                    ALTER TABLE [OrderItems] ALTER COLUMN [Status] int NULL;
                END
            ");

                // ✅ OrderItems.StatusPro
                migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'StatusPro')
                BEGIN
                    ALTER TABLE [OrderItems] ADD [StatusPro] nvarchar(max) NULL
                END
            ");
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropColumn(name: "Notes", table: "VolunteerMissions");
                migrationBuilder.DropColumn(name: "StatusPro", table: "OrderItems");
                migrationBuilder.AlterColumn<string>(
                    name: "Status",
                    table: "OrderItems",
                    type: "nvarchar(max)",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);
            }
        }
}
