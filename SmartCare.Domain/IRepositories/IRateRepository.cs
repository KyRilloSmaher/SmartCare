using SmartCare.Domain.Entities;

namespace SmartCare.Domain.IRepositories
{
    public interface IRateRepository : IGenericRepository<Rate>
    {
        #region Query Methods

        /// <summary>
        /// Gets queryable for rates with optional tracking
        /// </summary>
        IQueryable<Rate> GetRatesQueryable(bool trackChanges = false);

        /// <summary>
        /// Gets all rates for a specific product
        /// </summary>
        Task<IEnumerable<Rate>> GetRatesByProductIdAsync(Guid productId);

        /// <summary>
        /// Gets all rates by a specific user
        /// </summary>
        Task<IEnumerable<Rate>> GetRatesByUserIdAsync(string userId);

        /// <summary>
        /// Checks if a product has been rated by a user
        /// </summary>
        Task<bool> IsProductRatedByUserAsync(string userId, Guid productId);

        /// <summary>
        /// Gets average rating for a product
        /// </summary>
        Task<float> GetAverageRatingForProductAsync(Guid productId);

        /// <summary>
        /// Gets rating count for a product
        /// </summary>
        Task<int> GetRatingCountForProductAsync(Guid productId);

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// Updates the average rating for a product based on all rates
        /// </summary>
        Task<float> UpdateAverageRateForProductAsync(Guid productId);

        /// <summary>
        /// Marks all rates for a client as deleted (when client is deleted)
        /// </summary>
        Task<bool> MarkAllClientRatesAsDeletedAsync(string userId);

        #endregion
    }
}