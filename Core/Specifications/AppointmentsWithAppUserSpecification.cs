using System;
using System.Linq.Expressions;
using Core.Entities;
using Core.Entities.Identity;

namespace Core.Specifications
{
    public class AppointmentsWithAppUserSpecification : BaseSpecification<Appointment>
    {
        public AppointmentsWithAppUserSpecification(AppointmentsSpecificationParams appointmentsSpecificationParams, AppUser user)
        {
            Criteria = CreateCriteria(appointmentsSpecificationParams.EndingDate, appointmentsSpecificationParams.StartingDate,
                                    appointmentsSpecificationParams.NameSearch, user);

            ApplyPaging(appointmentsSpecificationParams.PageSize * (appointmentsSpecificationParams.PageIndex - 1), appointmentsSpecificationParams.PageSize);

            if (!string.IsNullOrEmpty(appointmentsSpecificationParams.Sort))
            {
                switch (appointmentsSpecificationParams.Sort)
                {
                    case "dateAsc":
                        AddOrderBy(a => a.EstimatedStartTime);
                        break;
                    case "dateDesc":
                        AddOrderByDescending(a => a.EstimatedStartTime);
                        break;
                    default:
                        AddOrderBy(a => a.EstimatedStartTime);
                        break;
                }
            }
        }

        public static Expression<Func<Appointment, bool>> CreateCriteria(DateTime endingDate, DateTime startingDate,
                string appUserNameSearch, AppUser user)
        {
            if (endingDate == default && startingDate == default && string.IsNullOrEmpty(appUserNameSearch) && user.IsAdmin)
            {
                return null;
            }

            if (startingDate == default)
            {
                startingDate = DateTime.Now.Date;
            }

            if (endingDate == default)
            {
                endingDate = DateTime.Now.Date;
            }

            if (user.IsAdmin || user.IsEmployee)
            {
                return (x => x.Date >= startingDate && x.Date <= endingDate
                    && (string.IsNullOrEmpty(appUserNameSearch) || x.AppUserName.ToLower().Contains(appUserNameSearch)));
            }
            else
            {
                return (x => x.AppUserEmail == user.Email && x.Date >= startingDate && x.Date <= endingDate
                    && (string.IsNullOrEmpty(appUserNameSearch) || x.AppUserName.ToLower().Contains(appUserNameSearch)));
            }
        }

        public AppointmentsWithAppUserSpecification(Expression<Func<Appointment, bool>> criteria) : base(criteria)
        {
        }
    }
}