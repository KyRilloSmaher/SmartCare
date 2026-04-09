using SmartCare.Application.DTOs.Orders.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.Notifications
{
    public interface IOrderNotificationService
    {
        Task NotifyNewOnlineOrderAsync(Guid storeId, OnlineOrderResponseDto order, CancellationToken ct = default);
        Task NotifyNewPickUpOrderAsync(Guid storeId,PickUpOrderNotificationDto order,CancellationToken ct = default);
    }
}
