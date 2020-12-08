using System;
using System.Linq.Expressions;
using Core.Entities.Identity;

namespace Core.Specifications
{
    public class UsersSpecification : BaseSpecification<AppUser>
    {
        public UsersSpecification(UsersSpecificationParams usersSpecificationParams)
        {
            Criteria = CreateCriteria(usersSpecificationParams.NameSearch);

            ApplyPaging(usersSpecificationParams.PageSize * (usersSpecificationParams.PageIndex - 1), usersSpecificationParams.PageSize);

            AddOrderBy(a => a.UserName);            
        }

        public static Expression<Func<AppUser, bool>> CreateCriteria(string appUserDisplayNameSearch)
        {
            return (x => string.IsNullOrEmpty(appUserDisplayNameSearch) || x.UserName.ToLower().Contains(appUserDisplayNameSearch));
        }
    }
}