using System;

namespace Core.Specifications
{
    public class AppointmentsSpecificationParams : PaginationSpecificationParams
    {
        public string Sort { get; set; }
        private DateTime _endingDate = default;
        public DateTime EndingDate
        {
            get { return _endingDate; }
            set { _endingDate = value.Date; }
        }
        
        private DateTime _startingDate = default;
        public DateTime StartingDate
        {
            get { return _startingDate; }
            set { _startingDate = value.Date; }
        }
        
        private string _nameSearch;
        public string NameSearch
        {
            get { return _nameSearch; }
            set
            {
                _nameSearch = value.ToLower();
            }
        }

    }
}