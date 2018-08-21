using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Carubbi.GenericRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected IDbContext Db;
        protected IDbSet<T> DbSet;
        protected IQueryable<T> Collecton;

        public GenericRepository(IDbContext db, ICollection<T> collection)
            : this(db)
        {
            Collecton = collection.AsQueryable();
        }

        public GenericRepository(IDbContext db, IEnumerable<T> collection)
           : this(db)
        {
            Collecton = collection.AsQueryable();
        }

        public GenericRepository(IDbContext db, IQueryable<T> collection)
          : this(db)
        {
            Collecton = collection;
        }

        public GenericRepository(IDbContext db, IOrderedQueryable<T> collection)
       : this(db)
        {
            Collecton = collection;
        }


        public GenericRepository(IDbContext db)
        {
            Db = db;
            DbSet = Db.Set<T>();
        }


        public virtual List<T> Search()
        {
            return Search(new SearchQuery<T>()).Entities.ToList();
        }

        //-----------------------------------------------------------
        ///<summary>
        /// Implementation method of the IRepository interface.
        ///</summary>  
        public virtual PagedListResult<T> Search(SearchQuery<T> searchQuery)
        {
            var sequence = Collecton ?? DbSet;

            sequence = ManageFilters(searchQuery, sequence);

            sequence = ManageIncludeProperties(searchQuery, sequence);

            sequence = ManageSortCriterias(searchQuery, sequence);

            return GetTheResult(searchQuery, sequence);
        }

        //-----------------------------------------------------------
        /// <summary>
        /// Executes the query against the repository (database).
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        protected virtual PagedListResult<T> GetTheResult(SearchQuery<T> searchQuery, IQueryable<T> sequence)
        {
            //Counting the total number of object.
            var resultCount = sequence.Count();

            var result = (searchQuery.Take > 0)
                                ? (sequence.Skip(searchQuery.Skip).Take(searchQuery.Take).ToList())
                                : (sequence.ToList());

            //Debug info of what the query looks like
            //Console.WriteLine(sequence.ToString());

            // Setting up the return object.
            var hasNext = (searchQuery.Skip > 0 || searchQuery.Take > 0) && (searchQuery.Skip + searchQuery.Take < resultCount);
            return new PagedListResult<T>()
            {
                Entities = result,
                HasNext = hasNext,
                HasPrevious = (searchQuery.Skip > 0),
                Count = resultCount
            };
        }

        //-----------------------------------------------------------
        /// <summary>
        /// Resolves and applies the sorting criteria of the SearchQuery
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        protected virtual IQueryable<T> ManageSortCriterias(SearchQuery<T> searchQuery, IQueryable<T> sequence)
        {
            if (searchQuery.SortCriterias != null && searchQuery.SortCriterias.Count > 0)
            {
                var sortCriteria = searchQuery.SortCriterias[0];
                var orderedSequence = sortCriteria.ApplyOrdering(sequence, false);

                if (searchQuery.SortCriterias.Count > 1)
                {
                    for (var i = 1; i < searchQuery.SortCriterias.Count; i++)
                    {
                        var sc = searchQuery.SortCriterias[i];
                        orderedSequence = sc.ApplyOrdering(orderedSequence, true);
                    }
                }
                sequence = orderedSequence;
            }
            else
            {
                sequence = ((IOrderedQueryable<T>)sequence).OrderBy(x => (true));
            }
            return sequence;
        }

        //-----------------------------------------------------------
        /// <summary>
        /// Chains the where clause to the IQueriable instance.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        protected virtual IQueryable<T> ManageFilters(SearchQuery<T> searchQuery, IQueryable<T> sequence)
        {
            if (searchQuery.Filters == null || searchQuery.Filters.Count <= 0) return sequence;

            foreach (var filterClause in searchQuery.Filters)
            {
                sequence = sequence.Where(filterClause);
            }
            return sequence;
        }

        //-----------------------------------------------------------
        /// <summary>
        /// Implementation of eager-loading. Includes the properties sent as part of the SearchQuery.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        protected virtual IQueryable<T> ManageIncludeProperties(SearchQuery<T> searchQuery, IQueryable<T> sequence)
        {
            if (string.IsNullOrWhiteSpace(searchQuery.IncludeProperties)) return sequence;

            var properties = searchQuery.IncludeProperties.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            return properties.Aggregate(sequence, (current, includeProperty) => current.Include(includeProperty));
        }
    }
}
