using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.OrderStates
{
        /// <summary>
        /// Defines behavior for each order state.
        /// Each state controls which transitions are allowed.
        /// </summary>
        public interface IOrderState
        {
            /// <summary>
            /// Handles transition from current state to next state.
            /// Throws exception if transition is invalid.
            /// </summary>
            void Handle(Order order, OrderStatus nextStatus);
        }
    
}
