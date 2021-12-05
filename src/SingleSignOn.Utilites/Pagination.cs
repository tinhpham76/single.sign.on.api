using System.Collections.Generic;

namespace SingleSignOn.Utilites
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; }

        public int TotalRecords { get; set; }
    }
}
