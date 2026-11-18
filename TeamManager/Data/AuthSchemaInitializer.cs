using Microsoft.EntityFrameworkCore;

namespace TeamManager.Data;

public static class AuthSchemaInitializer
{
    public static async Task EnsureCreatedAsync(SportManagerDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS `users` (
                `id` int NOT NULL AUTO_INCREMENT,
                `name` varchar(120) NOT NULL,
                `email` varchar(254) NOT NULL,
                `password_hash` varchar(512) NOT NULL,
                PRIMARY KEY (`id`),
                UNIQUE KEY `ix_users_email` (`email`)
            ) ENGINE=InnoDB;
            """);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS `revoked_tokens` (
                `jwt_id` varchar(36) NOT NULL,
                `expires_at` datetime(6) NOT NULL,
                PRIMARY KEY (`jwt_id`),
                KEY `ix_revoked_tokens_expires_at` (`expires_at`)
            ) ENGINE=InnoDB;
            """);

        await dbContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM `revoked_tokens` WHERE `expires_at` <= UTC_TIMESTAMP(6);");

        await EnsureOwnerColumnAsync(dbContext, "Teams");
        await EnsureOwnerColumnAsync(dbContext, "Players");
        await EnsureOwnerColumnAsync(dbContext, "Matches");
        await EnsureOwnerColumnAsync(dbContext, "Injuries");
    }

    private static async Task EnsureOwnerColumnAsync(
        SportManagerDbContext dbContext,
        string tableName)
    {
        var columnCount = await dbContext.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS `Value` FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = {0} AND column_name = 'UserId'",
                tableName)
            .SingleAsync();

        if (columnCount == 0)
        {
            await AddOwnerColumnAsync(dbContext, tableName);
        }

        var indexName = $"IX_{tableName}_UserId";
        var indexCount = await dbContext.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS `Value` FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = {0} AND index_name = {1}",
                tableName,
                indexName)
            .SingleAsync();

        if (indexCount == 0)
        {
            await AddOwnerIndexAsync(dbContext, tableName);
        }

        var foreignKeyCount = await dbContext.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS `Value` FROM information_schema.key_column_usage WHERE table_schema = DATABASE() AND table_name = {0} AND column_name = 'UserId' AND referenced_table_name = 'users'",
                tableName)
            .SingleAsync();

        if (foreignKeyCount == 0)
        {
            await AddOwnerForeignKeyAsync(dbContext, tableName);
        }
    }

    private static Task<int> AddOwnerColumnAsync(SportManagerDbContext dbContext, string tableName) =>
        tableName switch
        {
            "Teams" => dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE `Teams` ADD COLUMN `UserId` int NULL;"),
            "Players" => dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE `Players` ADD COLUMN `UserId` int NULL;"),
            "Matches" => dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE `Matches` ADD COLUMN `UserId` int NULL;"),
            "Injuries" => dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE `Injuries` ADD COLUMN `UserId` int NULL;"),
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };

    private static Task<int> AddOwnerIndexAsync(SportManagerDbContext dbContext, string tableName) =>
        tableName switch
        {
            "Teams" => dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX `IX_Teams_UserId` ON `Teams` (`UserId`);"),
            "Players" => dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX `IX_Players_UserId` ON `Players` (`UserId`);"),
            "Matches" => dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX `IX_Matches_UserId` ON `Matches` (`UserId`);"),
            "Injuries" => dbContext.Database.ExecuteSqlRawAsync("CREATE INDEX `IX_Injuries_UserId` ON `Injuries` (`UserId`);"),
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };

    private static Task<int> AddOwnerForeignKeyAsync(SportManagerDbContext dbContext, string tableName) =>
        tableName switch
        {
            "Teams" => dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE `Teams` ADD CONSTRAINT `FK_Teams_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE RESTRICT;"),
            "Players" => dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE `Players` ADD CONSTRAINT `FK_Players_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE RESTRICT;"),
            "Matches" => dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE `Matches` ADD CONSTRAINT `FK_Matches_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE RESTRICT;"),
            "Injuries" => dbContext.Database.ExecuteSqlRawAsync(
                "ALTER TABLE `Injuries` ADD CONSTRAINT `FK_Injuries_users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`id`) ON DELETE RESTRICT;"),
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };
}
