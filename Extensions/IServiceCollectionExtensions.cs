using System.Collections.Generic;
using EntityFrameworkCore.TemporalTables.Migrations;
using EntityFrameworkCore.TemporalTables.Query;
using EntityFrameworkCore.TemporalTables.Sql;
using EntityFrameworkCore.TemporalTables.Sql.Factory;
using EntityFrameworkCore.TemporalTables.Sql.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EntityFrameworkCore.TemporalTables.Extensions
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Register temporal table services for the specified <see cref="DbContext"/>.
        /// </summary>
        public static IServiceCollection RegisterTemporalTablesForDatabase<TContext>(
            this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<ISqlQueryExecutor<TContext>, SqlQueryExecutor<TContext>>();
            services.AddScoped<ITableHelper<TContext>, TableHelper<TContext>>();
            services.AddScoped<ITemporalTableSqlBuilder<TContext>, TemporalTableSqlBuilder<TContext>>();
            services.AddScoped<ITemporalTableSqlExecutor<TContext>, TemporalTableSqlExecutor<TContext>>();
            //services.AddScoped<IMigrator, TemporalTableMigrator<TContext>>(); //<-- with 2 temporal-DbContexts, an IMigrator gets registered twice, the second migrator takes precedence, so it is called by EF, even when the other Context is passed as the arg to the CLI tool
            services.AddScoped<ITemporalTableMigrator, TemporalTableMigrator<TContext>>();
            services.AddScoped<IMigrator, TemporalTableMigratorResolver>();
            services.AddScoped<IMigrationsSqlGenerator, TemporalTablesMigrationsSqlGenerator<TContext>>();

            services.AddSingleton<ITemporalTableSqlGeneratorFactory, TemporalTableSqlGeneratorFactory>();

            // replace the service responsible for generating SQL strings
            services.AddSingleton<IQuerySqlGeneratorFactory, AsOfQuerySqlGeneratorFactory>();
            // replace the service responsible for traversing the Linq AST (a.k.a Query Methods)
            services.AddSingleton<IQueryableMethodTranslatingExpressionVisitorFactory, AsOfQueryableMethodTranslatingExpressionVisitorFactory>();
            // replace the service responsible for providing instances of SqlExpressions
            services.AddSingleton<ISqlExpressionFactory, AsOfSqlExpressionFactory>();


            services.AddSingleton<IRelationalParameterBasedSqlProcessorFactory, MyRelationalParameterBasedSqlProcessorFactory>();

            return services;
        }
    }

    public class MyRelationalParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
    {
        private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MyRelationalParameterBasedSqlProcessorFactory(RelationalParameterBasedSqlProcessorDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
        {
            return new MySqlServerParameterBasedSqlProcessor(_dependencies, useRelationalNulls);
        }
    }

    public class MySqlServerParameterBasedSqlProcessor : SqlServerParameterBasedSqlProcessor
    {
        public MySqlServerParameterBasedSqlProcessor(RelationalParameterBasedSqlProcessorDependencies dependencies, bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
        {
        }

        protected override SelectExpression ProcessSqlNullability(SelectExpression selectExpression, IReadOnlyDictionary<string, object> parametersValues, out bool canCache)
        {
            return new MySqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(selectExpression, parametersValues, out canCache);
        }
    }

    public class MySqlNullabilityProcessor : SqlNullabilityProcessor
    {
        public MySqlNullabilityProcessor(RelationalParameterBasedSqlProcessorDependencies dependencies, bool useRelationalNulls)
            : base(dependencies, useRelationalNulls)
        {
        }

        protected override TableExpressionBase Visit(TableExpressionBase tableExpressionBase)
        {
            if (tableExpressionBase is AsOfTableExpression)
                return tableExpressionBase;
            return base.Visit(tableExpressionBase);
        }
    }
}