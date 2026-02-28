using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartCare.Domain.Entities
{
    /// <summary>
    /// Represents a shopping cart that belongs to a client
    /// and contains a collection of cart items.
    /// </summary>
    public class Cart
    {
        /// <summary>
        /// Unique identifier of the cart.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the client who owns the cart.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Current status of the cart.
        /// </summary>
        public CartStatus status { get; set; }

        /// <summary>
        /// Total price of all items in the cart.
        /// </summary>
        public decimal TotalPrice { get; set ; }

        /// <summary>
        /// Navigation property to the client.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Collection of items inside the cart.
        /// </summary>
        public ICollection<CartItem> Items { get;  set; } = new List<CartItem>();

        /// <summary>
        /// Creates an empty cart.
        /// </summary>
        public Cart() { }

        /// <summary>
        /// Adds an item to the cart.
        /// </summary>
        public void AddItem(CartItem item)
        {
            if (item == null) return;
            Items.Add(item);
        }

        /// <summary>
        /// Removes an item from the cart.
        /// </summary>
        public void RemoveItem(CartItem item)
        {
            if (item == null) return;
            Items.Remove(item);
        }

        /// <summary>
        /// Removes all items from the cart.
        /// </summary>
        public void ClearItems()
        {
            Items.Clear();
        }

        /// <summary>
        /// Calculates the total price of all items.
        /// </summary>
         public void ReCalculateTotalPrice()
        {
            TotalPrice = Items?.Sum(i => i.SubTotal) ?? 0m;
        }
    }
}