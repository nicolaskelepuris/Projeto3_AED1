using System.Linq;
using Core.Specifications;

namespace Infrastructure.Data
{
    public class UserSpecificationEvaluator<AppUser>
    {
        public static IQueryable<AppUser> GetQuery(IQueryable<AppUser> inputQuery, ISpecification<AppUser> specification)
        {
            var query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }

            if(specification.IsPaginEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
}