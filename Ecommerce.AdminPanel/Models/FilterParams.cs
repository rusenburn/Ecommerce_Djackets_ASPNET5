namespace Ecommerce.AdminPanel.Models
{
    public class FilterParams
    {
        private int page = 1;
        public int PageSize { get; set; } = 20;
        public string Search { get; set; } = "";
        public int Page
        {
            get
            {
                return page;
            }
            set
            {
                page = value < 1 ? 1 : value;
            }
        }
        public int NextPage
        {
            get
            {
                return page + 1;
            }
        }
        public int PreviousPage
        {
            get
            {
                return page <= 1 ? 1 : page - 1;
            }
        }


    }
}