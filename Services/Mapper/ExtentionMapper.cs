using System;
using System.Linq.Expressions;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace Services.Mapping
{
    public static class QueryableMappingExtensions
    {
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            params Expression<Func<TDestination, object>>[] membersToExpand) => source == null
                ? throw new ArgumentNullException(nameof(source))
                : source.ProjectTo(ConfigMapper.Instance.ConfigurationProvider, null, membersToExpand);

        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            object parameters) => source == null
                ? throw new ArgumentNullException(nameof(source))
                : source.ProjectTo<TDestination>(ConfigMapper.Instance.ConfigurationProvider, parameters);
    }
}
