using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertListsToJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            ALTER TABLE ""Authors""
            ALTER COLUMN ""Follows"" TYPE jsonb
            USING CASE
                    WHEN ""Follows"" IS NULL OR ""Follows"" = '{}' THEN '[]'::jsonb
                    ELSE ""Follows""::jsonb
                  END;
        ");

            // Alter CheepLikes column from text -> jsonb
            migrationBuilder.Sql(@"
            ALTER TABLE ""Authors""
            ALTER COLUMN ""CheepLikes"" TYPE jsonb
            USING CASE
                    WHEN ""CheepLikes"" IS NULL OR ""CheepLikes"" = '{}' THEN '[]'::jsonb
                    ELSE ""CheepLikes""::jsonb
                  END;
        ");

            migrationBuilder.Sql(@"
    ALTER TABLE ""Cheeps""
    ALTER COLUMN ""PeopleLikes"" TYPE jsonb
    USING CASE
            WHEN ""PeopleLikes"" IS NULL OR ""PeopleLikes"" = '{}' THEN '[]'::jsonb
            ELSE ""PeopleLikes""::jsonb
          END;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to text
            migrationBuilder.Sql(@"
            ALTER TABLE ""Authors""
            ALTER COLUMN ""Follows"" TYPE text
            USING ""Follows""::text;
        ");

            migrationBuilder.Sql(@"
            ALTER TABLE ""Authors""
            ALTER COLUMN ""CheepLikes"" TYPE text
            USING ""CheepLikes""::text;
        ");
            
            migrationBuilder.Sql(@"
    ALTER TABLE ""Cheeps""
    ALTER COLUMN ""PeopleLikes"" TYPE text
    USING ""PeopleLikes""::text;
");

        }
    }
}
