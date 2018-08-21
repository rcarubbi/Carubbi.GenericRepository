using System.Collections.Generic;

namespace Carubbi.GenericRepository
{
    public class PagedListResult<TEntity> : IPagedListResult<TEntity>
    {
        //-----------------------------------------------------------
        /// <summary>
        /// Does the returned result contains more rows to be retrieved?
        /// </summary>
        public bool HasNext { get; set; }

        //-----------------------------------------------------------
        /// <inheritdoc />
        /// <summary>
        /// Does the returned result contains previous items ?
        /// </summary>
        public bool HasPrevious { get; set; }

        //-----------------------------------------------------------
        /// <inheritdoc />
        /// <summary>
        /// Total number of rows that could be possibly be retrieved.
        /// </summary>
        public int Count { get; set; }

        //-----------------------------------------------------------
        /// <inheritdoc />
        /// <summary>
        /// Result of the query.
        /// </summary>
        public IEnumerable<TEntity> Entities { get; set; } = new List<TEntity>();

    }

}
