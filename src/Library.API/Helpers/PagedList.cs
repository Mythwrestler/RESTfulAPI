using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.API.Helpers
{
    public class PagedList<t> : List<t>
    {
        public PagedList() { }

        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasPrevious
        {
            get
            {
                return (CurrentPage > 1);
            }
        }

        public bool HasNext
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }

        public PagedList (List<t> list, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(list);
        }

        public static PagedList<t> Create(IQueryable<t> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var list = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<t>(list, count, pageNumber, pageSize);

        }

    }
}
