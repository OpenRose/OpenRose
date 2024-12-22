// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.Helper
{
    public class PagedList<T>: List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
//        public bool HasPrevious => (CurrentPage > 1);
        public bool HasPrevious
        {
            get
            {
                if (CurrentPage > 1 && CurrentPage <= TotalPages)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool HasNext => (CurrentPage < TotalPages);

        public PagedList(List<T> itemz, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(itemz);
        }
        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var itemz = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(itemz, count, pageNumber, pageSize);
        }
    }
}
