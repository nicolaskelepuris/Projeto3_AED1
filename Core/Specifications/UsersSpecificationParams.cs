
namespace Core.Specifications
{
    public class UsersSpecificationParams : PaginationSpecificationParams
    {
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