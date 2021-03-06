using System;
using EntityFrameworkCore.TemporalTables.Query;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    public static class SqlServerAsOfEntityTypeBuilderExtensions
    {
        public static string ANNOTATION_TEMPORAL = "IS_TEMPORAL_TABLE";

        public static EntityTypeBuilder<TEntity> HasTemporalTable<TEntity>(this EntityTypeBuilder<TEntity> entity) where TEntity : class
        {
            entity.Metadata.SetAnnotation(ANNOTATION_TEMPORAL, true);
            return entity;
        }   
        
        public static EntityTypeBuilder HasTemporalTable(this EntityTypeBuilder entity) 
        {
            entity.Metadata.SetAnnotation(ANNOTATION_TEMPORAL, true);
            return entity;
        }

        public static DbContextOptionsBuilder EnableTemporalTableQueries(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
        {
            // If service provision is NOT being performed internally, we cannot replace services.
            var coreOptions = optionsBuilder.Options.GetExtension<CoreOptionsExtension>();
            if (coreOptions.InternalServiceProvider == null)
            {
                return optionsBuilder
                    // replace the service responsible for generating SQL strings
                    .ReplaceService<IQuerySqlGeneratorFactory, AsOfQuerySqlGeneratorFactory>()
                    // replace the service responsible for traversing the Linq AST (a.k.a Query Methods)
                    .ReplaceService<IQueryableMethodTranslatingExpressionVisitorFactory, AsOfQueryableMethodTranslatingExpressionVisitorFactory>()
                    // replace the service responsible for providing instances of SqlExpressions
                    .ReplaceService<ISqlExpressionFactory, AsOfSqlExpressionFactory>();
            }
            else 
                return optionsBuilder;
        }
    }
}
