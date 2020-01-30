using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Extensions
{
    internal static class ReflectionHelpers
    {
        internal static List<PropertyInfo> GetDbSetProperties(this Microsoft.EntityFrameworkCore.DbContext context)
        {
            var dbSetProperties = new List<PropertyInfo>();
            var properties = context.GetType().GetProperties();
            foreach (var property in properties)
            {
                var setType = property.PropertyType;
                var isDbSet = setType.IsGenericType && (typeof (Microsoft.EntityFrameworkCore.DbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()));
                if (isDbSet)
                {
                    dbSetProperties.Add(property);
                }
            }
            return dbSetProperties;
        }

        internal static readonly MethodInfo SetQueryFilterMethod = typeof(ModelBuilderExtensions)
              .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
              .Single(t => t.IsGenericMethod && t.Name == "SetQueryFilter");

        internal static void SetEntityQueryFilter<TEntityInterface>(
              this Microsoft.EntityFrameworkCore.ModelBuilder builder,
              Type entityType,
              Expression<Func<TEntityInterface, bool>> filterExpression)
        {
            SetQueryFilterMethod
              .MakeGenericMethod(entityType, typeof(TEntityInterface))
              .Invoke(null, new object[] { builder, filterExpression });
        }


        internal static T GetAttr<T>(this System.Type tableType)
            where T : class
        {
            return tableType.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(T)) as T;
        }

        internal static T GetAttr<T>(this PropertyInfo propInfo)
            where T : class
        {
            return propInfo.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(T)) as T;
        }

        internal static PropertyInfo[] Props(this System.Type tableType, bool onlyPublic = true)
        {
            var retval = new List<PropertyInfo>();
            if (onlyPublic)
                retval.AddRange(tableType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance));
            else
                retval.AddRange(tableType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));

            retval.RemoveAll(x => x.Name == "EFCore.Extensions.ITenantEntity.TenantId");
            retval.RemoveAll(x => x.Name == "EFCore.Extensions.ISoftDeleted.IsDeleted");

            return retval.ToArray();
        }

        internal static Tuple<T, PropertyInfo> GetAttrWithProp<T>(this PropertyInfo propInfo)
            where T : class
        {
            var t = propInfo.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(T)) as T;
            return new Tuple<T, PropertyInfo>(t, propInfo);
        }

        internal static bool NotMapped(this PropertyInfo prop)
        {
            return prop.GetCustomAttributes(true).Any(z => z.GetType() == typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute));
        }

        // Given an expression for a method that takes in a single parameter (and
        // returns a bool), this method converts the parameter type of the parameter
        // from TSource to TTarget.
        internal static Expression<Func<TTarget, bool>> Convert<TSource, TTarget>(
          this Expression<Func<TSource, bool>> root)
        {
            var visitor = new ParameterTypeVisitor<TSource, TTarget>();
            return (Expression<Func<TTarget, bool>>)visitor.Visit(root);
        }

        private class ParameterTypeVisitor<TSource, TTarget> : ExpressionVisitor
        {
            private ReadOnlyCollection<ParameterExpression> _parameters;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _parameters?.FirstOrDefault(p => p.Name == node.Name)
                  ?? (node.Type == typeof(TSource) ? Expression.Parameter(typeof(TTarget), node.Name) : node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
                return Expression.Lambda(Visit(node.Body), _parameters);
            }
        }

    }

    internal static class ModelBuilderExtensions
    {
        static void SetQueryFilter<TEntity, TEntityInterface>(
          this Microsoft.EntityFrameworkCore.ModelBuilder builder,
          Expression<Func<TEntityInterface, bool>> filterExpression)
            where TEntityInterface : class
            where TEntity : class, TEntityInterface
        {
            var concreteExpression = filterExpression
              .Convert<TEntityInterface, TEntity>();
            builder.Entity<TEntity>()
              .HasQueryFilter(concreteExpression);
        }

        // More code to follow...
    }

}
