﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Temporal.Query.Extensions.Internal;
using System.Linq.Expressions;

namespace EntityFrameworkCore.TemporalTables.Query
{
    /// <summary>
    /// This class is responsible for traversing the Linq query extension methods, searching for any
    /// calls to the "AsOf" extension.
    /// When it encounters them, it will process the expressions they are attached to, and find any "AsOfTableExpression" instances,
    /// then set the "AsOfDate" property of those tables.
    /// </summary>
    public class AsOfQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
    {
        private readonly RelationalQueryableMethodTranslatingExpressionVisitorDependencies _relationalDependencies;
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private ParameterExpression _asOfDateParameter;

        private AsOfQueryableMethodTranslatingExpressionVisitor(AsOfQueryableMethodTranslatingExpressionVisitor parentVisitor)
            : base(parentVisitor)
        {
            _queryCompilationContext = parentVisitor._queryCompilationContext;
            _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
            _asOfDateParameter = parentVisitor._asOfDateParameter;
        }

        public AsOfQueryableMethodTranslatingExpressionVisitor(QueryableMethodTranslatingExpressionVisitorDependencies dependencies, RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies, QueryCompilationContext model, ParameterExpression asOfDateParameter = null)
            : base(dependencies, relationalDependencies, model)
        {
            _relationalDependencies = relationalDependencies;
            _queryCompilationContext = model;
            _asOfDateParameter = asOfDateParameter;

            var sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            var result = base.VisitConstant(constantExpression);
            if (null != _asOfDateParameter && result is ShapedQueryExpression shapedExpression)
            {
                // attempt to apply the captured date parameter to any select-from-table expressions
                shapedExpression.TrySetDateParameter(_asOfDateParameter);
            }

            return result;
        }
    
        protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
            => new AsOfQueryableMethodTranslatingExpressionVisitor(this);

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(SqlServerAsOfQueryableExtensions))
            {
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(SqlServerAsOfQueryableExtensions.AsOf):
                        // capture the date parameter for use by all AsOfTableExpression instances
                        _asOfDateParameter = Visit(methodCallExpression.Arguments[1]) as ParameterExpression;
                        var visitMethodCall = Visit(methodCallExpression.Arguments[0]);
                        
                        if (null != _asOfDateParameter && visitMethodCall is ShapedQueryExpression shapedExpression)
                        {
                            // attempt to apply the captured date parameter to any select-from-table expressions
                            shapedExpression.TrySetDateParameter(_asOfDateParameter);
                        }

                        return visitMethodCall;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}