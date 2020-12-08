namespace Core.Specifications
{
    public class PaginationSpecificationParams
    {
        private const int MaxPageSize = 50;
        private const int MinPageSize = 2;
        public int PageIndex { get; set; } = 1;
        private int _pageSize = 30;
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value > MinPageSize
                     ? (value > MaxPageSize ? MaxPageSize : value)
                     : MinPageSize;
            }
        }        
    }
}