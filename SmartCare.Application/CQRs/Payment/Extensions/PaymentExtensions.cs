using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Enums;
using SmartCare.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.CQRs.Payment.Extensions
{
    public class PaymentExtensions
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly IEmailService _emailService;
        private readonly IStoreRepository _storeRepository;

        public PaymentExtensions(IOrderRepository orderRepository, IInventoryRepository inventoryRepository, IClientRepository clientRepository, IReservationRepository reservationRepository, IBackgroundJobService backgroundJobs, IEmailService emailService, IStoreRepository storeRepository)
        {
            _orderRepository = orderRepository;
            _inventoryRepository = inventoryRepository;
            _clientRepository = clientRepository;
            _reservationRepository = reservationRepository;
            _backgroundJobs = backgroundJobs;
            _emailService = emailService;
            _storeRepository = storeRepository;
        }

        public string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes); // uppercase hex
        }
        public async Task IncrementClientOrdersAsync(string clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, true);
            if (client == null) return;

            client.OrdersCount++;
            await _clientRepository.UpdateAsync(client);
        }

        public async Task ReleaseReservationsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order is null) return;

            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                if (item.ReservationId == Guid.Empty) continue;

                await _reservationRepository.CancelReservationAsync(
                    (Guid)item.ReservationId,
                    item.InvetoryId,
                    ReservationStatus.PaymentTimeOut);
            }
        }
        public async Task FinishReservationsAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId);
            if (order is null) return;
            foreach (var item in order.Items ?? Enumerable.Empty<OrderItem>())
            {
                await _inventoryRepository.FinalizeStockDeductionAsync(
                        item.InvetoryId,
                        item.Quantity,
                        order is FromStoreOrder
                    );
                if (item.ReservationId == Guid.Empty) continue;
                await _reservationRepository.UpdateReservationStatusAsync((Guid)item.ReservationId, ReservationStatus.Completed);
            }
        }

        public void PublishPaymentEvent(Order order, string status, string message)
        {
            _backgroundJobs.Enqueue<IEventPublisherService>(publisher =>
                publisher.PublishPaymentStatusChanged(order.Id, order.ClientId, status, message));
        }

        public async Task SendPickupEmailAsync(Order order, Client client, string pickupCode, Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);

            var emailBody = SystemMessages.PICKUP_ORDER_EMAIL_TEMPLATE
                .Replace("{{UserName}}", client.UserName)
                .Replace("{{PickupCode}}", pickupCode)
                .Replace("{{StoreName}}", store.Name)
                .Replace("{{StoreAddress}}", store.Address)
                .Replace("{{OrderDate}}", order.CreatedAt.ToString("MMMM dd, yyyy"))
                .Replace("{{OrderTotal}}", order.TotalPrice.ToString())
                .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            _backgroundJobs.Schedule(
                () => _emailService.SendEmailAsync(
                    client.Email,
                    "Your Pickup Order Details",
                    emailBody),
                TimeSpan.FromSeconds(5));
        }
        public async Task SendOrderConfirmationEmailAsync(Order order, Client client)
        {

            var emailBody = SystemMessages.ORDERCONFIRMATION_TEMPLATE
                       .Replace("{{UserName}}", client.UserName)
                       .Replace("{{OrderId}}", order.Id.ToString("N")[^6..])
                       .Replace("{{OrderDate}}", order.CreatedAt.ToString("MMMM dd, yyyy"))
                       .Replace("{{OrderTotal}}", order.TotalPrice.ToString())
                       .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            _backgroundJobs.Schedule(() => _emailService.SendEmailAsync(client.Email, "Your  Order Details", emailBody),
                                            TimeSpan.FromSeconds(5));
        }
    }
}
