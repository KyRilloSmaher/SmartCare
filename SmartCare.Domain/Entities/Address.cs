using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    /// <summary>
    /// Represents a physical address entity in the SmartCare system.
    /// Handles address information with geolocation capabilities and soft delete functionality.
    /// </summary>
    public class Address
    {
        #region Attributes

        /// <summary>
        /// Gets or sets the unique identifier for the address.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the client identifier associated with this address.
        /// </summary>
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the full address string.
        /// </summary>
        public string AddressLine { get; set; } = null!;

        /// <summary>
        /// Gets or sets a descriptive label for the address (e.g., "Home", "Work", "Clinic").
        /// </summary>
        public string Label { get; set; } = null!;

        /// <summary>
        /// Gets or sets any additional information about the address (apartment number, landmarks, etc.).
        /// </summary>
        public string? AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate of the address.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate of the address.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the primary address for the client.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this address is soft-deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor for creating a new Address instance.
        /// </summary>
        /// <param name="clientId">The ID of the client associated with this address.</param>
        /// <param name="addressLine">The full address string.</param>
        /// <param name="label">A descriptive label for the address.</param>
        /// <param name="additionalInfo">Additional information about the address.</param>
        /// <param name="latitude">Latitude coordinate.</param>
        /// <param name="longitude">Longitude coordinate.</param>
        /// <param name="isPrimary">Whether this is the primary address.</param>
        private Address(string clientId, string addressLine, string label, string? additionalInfo, float latitude, float longitude, bool isPrimary)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            AddressLine = addressLine;
            Label = label;
            AdditionalInfo = additionalInfo;
            Latitude = latitude;
            Longitude = longitude;
            IsPrimary = isPrimary;
            IsDeleted = false;
        }
<<<<<<< HEAD
        public Address() { } 
=======
        public Address()
        {
            
        }

>>>>>>> c3c90de8b23e23142f77cdd187b7f12bcb3819df
        #endregion

        #region Methods

        /// <summary>
        /// Factory method to create a new Address instance.
        /// </summary>
        /// <param name="clientId">The ID of the client associated with this address.</param>
        /// <param name="addressLine">The full address string.</param>
        /// <param name="label">A descriptive label for the address.</param>
        /// <param name="additionalInfo">Additional information about the address.</param>
        /// <param name="latitude">Latitude coordinate.</param>
        /// <param name="longitude">Longitude coordinate.</param>
        /// <param name="isPrimary">Whether this is the primary address.</param>
        /// <returns>A new instance of Address.</returns>
        public static Address Create(string clientId, string addressLine, string label, string? additionalInfo, float latitude, float longitude, bool isPrimary)
            => new Address(clientId, addressLine, label, additionalInfo, latitude, longitude, isPrimary);

        /// <summary>
        /// Soft deletes the address by setting the IsDeleted flag to true.
        /// </summary>
        public void Delete()
        {
            IsDeleted = true;
        }

        /// <summary>
        /// Updates the address information.
        /// </summary>
        /// <param name="addressLine">The new address string.</param>
        /// <param name="label">The new label.</param>
        /// <param name="additionalInfo">The new additional information.</param>
        /// <param name="latitude">The new latitude coordinate.</param>
        /// <param name="longitude">The new longitude coordinate.</param>
        /// <param name="isPrimary">The new primary status.</param>
        /// <exception cref="InvalidOperationException">Thrown when attempting to update a deleted address.</exception>
        public void Update(string addressLine, string label, string? additionalInfo, float latitude, float longitude, bool isPrimary)
        {
            if (IsDeleted)
            {
                throw new InvalidOperationException("Cannot update a deleted address.");
            }

            AddressLine = addressLine;
            Label = label;
            AdditionalInfo = additionalInfo;
            Latitude = latitude;
            Longitude = longitude;
            IsPrimary = isPrimary;
        }

        #endregion
    }
}