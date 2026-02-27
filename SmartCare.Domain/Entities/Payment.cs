using SmartCare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Entities
{
    public class Payment
    {
        /// <summary>
        /// Gets the unique identifier for this transaction.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the Order being Pruches.
        /// </summary>
        public Guid OrderId { get; private set; }

        public Order Order { get; private set; } = default!;
        /// <summary>
        /// Gets the unique identifier for the associated payment intent.
        /// </summary>
        public string ProviderReferenceId { get; private set; } = default!;

        /// <summary>
        /// Gets the monetary amount of the payment.
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Gets the current status of the  transaction.
        /// </summary>
        public PaymentStatus Status { get; private set; }

        /// <summary>
        /// Gets the name of the payment provider associated with the transaction.
        /// </summary>
        public PaymentMethod Method { get; private set; }

        /// <summary>
        /// Gets the date and time when the transaction was initiated.
        /// </summary>
        public DateTime CreatedAt { get; private set; }
        /// <summary>
        /// Gets the date and time when the transaction was Updated.
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the date and time when the transaction was completed or failed.
        /// </summary>
        /// <remarks>
        /// This property is null until the transaction reaches a terminal state (completed or failed).
        /// </remarks>
        public DateTime? CompletedAt { get; private set; }

        /// <summary>
        /// Gets or sets the client secret used for frontend payment confirmation.
        /// </summary>
        /// <remarks>
        /// This value is typically provided by the payment gateway and used by the client-side
        /// application to confirm payment intent with the payment provider.
        /// </remarks>
        public string? ClientPaymentToken { get; private set; }

        /// <summary>
        /// Gets or sets the version number for concurrency control and payment update tracking.
        /// </summary>
        /// <remarks>
        /// This property helps prevent race conditions when updating recharge status and
        /// ensures proper sequencing of payment provider callbacks.
        /// </remarks>
        public int Version { get; set; }

        public Payment() { }
        public Payment(Guid orderId , decimal amount , PaymentMethod provider ,string providerReferenceId, string clientPaymentToken)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Amount = amount;
            Method = provider;
            ProviderReferenceId = providerReferenceId;
            ClientPaymentToken = clientPaymentToken;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            Version = 1;
        }

        public void MarkCompleted()
        {
            if (Status != PaymentStatus.Pending)
                throw new Exception(
                    "Only pending recharges can be completed."
                );
            Status = PaymentStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkFailed()
        {
            if (Status != PaymentStatus.Pending)
                throw new Exception(
                    "Only pending recharges can be failed."
                );

            Status = PaymentStatus.Failed;
            CompletedAt = DateTime.UtcNow;
        }

        public void AttachProviderSession(string providerReferenceId, string clientPaymentToken)
        {
            if (string.IsNullOrWhiteSpace(providerReferenceId))
                throw new Exception("Provider reference ID cannot be empty.");

            if (string.IsNullOrWhiteSpace(clientPaymentToken))
                throw new Exception("Client payment token cannot be empty.");

            ProviderReferenceId = providerReferenceId;
            ClientPaymentToken = clientPaymentToken;
            Version++;
        }

        public void UpdatePaymentData(decimal newAmount , string providerReferenceId, string clientPaymentToken)
        {
            Amount = newAmount;
            ProviderReferenceId = providerReferenceId;
            ClientPaymentToken = clientPaymentToken;
            Version++;
            UpdatedAt = DateTime.UtcNow;
        }
    }

}
