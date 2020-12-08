using System;
using System.Linq.Expressions;
using Core.Entities;

namespace Core.Specifications
{
    public class AppointmentsWithFiltersForCountSpecification : BaseSpecification<Appointment>
    {
        public AppointmentsWithFiltersForCountSpecification(Expression<Func<Appointment, bool>> criteria) : base(criteria)
        {
        }
    }
}