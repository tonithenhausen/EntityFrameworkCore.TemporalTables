﻿using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkCore.TemporalTables.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityFrameworkCore.TemporalTables.Migrations
{
    public class TemporalTableMigrator<TContext> : Migrator, ITemporalTableMigrator
        where TContext : DbContext
    {
        private readonly ITemporalTableSqlExecutor<TContext> temporalTableSqlExecutor;

        public TemporalTableMigrator(
            IMigrationsAssembly migrationsAssembly,
            IHistoryRepository historyRepository,
            IDatabaseCreator databaseCreator,
            IEnumerable<IMigrationsSqlGenerator> migrationsSqlGenerators,
            IRawSqlCommandBuilder rawSqlCommandBuilder,
            IMigrationCommandExecutor migrationCommandExecutor,
            IRelationalConnection connection,
            ISqlGenerationHelper sqlGenerationHelper,
            ICurrentDbContext currentDbContext,
            IConventionSetBuilder conventionSetBuilder,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger,
            IDatabaseProvider databaseProvider,
            ITemporalTableSqlExecutor<TContext> temporalTableSqlExecutor)
            : base(migrationsAssembly, historyRepository, databaseCreator, resolveMigrationsSqlGenerator(migrationsSqlGenerators), rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentDbContext, conventionSetBuilder, logger, commandLogger, databaseProvider)
        {
            this.temporalTableSqlExecutor = temporalTableSqlExecutor;
        }

        private static IMigrationsSqlGenerator resolveMigrationsSqlGenerator(IEnumerable<IMigrationsSqlGenerator> migrationsSqlGenerators) => migrationsSqlGenerators?.OfType<TemporalTablesMigrationsSqlGenerator<TContext>>().LastOrDefault() ?? migrationsSqlGenerators?.LastOrDefault();

        public override void Migrate(string targetMigration = null)
        {
            base.Migrate(targetMigration);

            temporalTableSqlExecutor.Execute();
        }
    }
}
