using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Data
{
    public class Paginable<TEntity>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalEntities { get; set; }
        public int PageSize { get; set; }

        public IEnumerable<TEntity> Entities { get; set; }

        public Paginable(int currentPage, int totalEntities, int pageSize, IEnumerable<TEntity> entities)
        {
            CurrentPage = currentPage;
            TotalEntities = totalEntities;
            TotalPages = (totalEntities / pageSize) + 1;
            PageSize = pageSize;
            Entities = entities;
        }
    }
}
