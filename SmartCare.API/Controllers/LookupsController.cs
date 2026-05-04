
using Microsoft.AspNetCore.Mvc;
using SmartCare.Domain.Enums;

namespace SmartCare.API.Controllers
{

    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {


        [HttpGet("genders")]
        public async Task<IActionResult> GetGenders()
        {

            var list = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName =e.ToString()
                })
                .ToList();

            return Ok(list);
        }

        [HttpGet("account-types")]
        public async Task<IActionResult> GetAccountTypes()
        {

            var list = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods()
        {

            var list = Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }
        [HttpGet("payment-statues")]
        public async Task<IActionResult> GetPaymentStatues()
        {

            var list = Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }
        [HttpGet("order-statues")]
        public async Task<IActionResult> GetOrderStatues()
        {

            var list = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }
        [HttpGet("order-Types")]
        public async Task<IActionResult> GetOrderTypes()
        {

            var list = Enum.GetValues(typeof(OrderType))
                .Cast<OrderType>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }
        [HttpGet("cart-statues")]
        public async Task<IActionResult> GetCartstatues()
        {

            var list = Enum.GetValues(typeof(CartStatus))
                .Cast<CartStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }

        
        [HttpGet("Reservation-statues")]
        public async Task<IActionResult> GetReservationstatues()
        {

            var list = Enum.GetValues(typeof(ReservationStatus))
                .Cast<ReservationStatus>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }

        
        [HttpGet("Filter-intervales")]
        public async Task<IActionResult> GetFilterintervals()
        {

            var list = Enum.GetValues(typeof(FilterIntervales))
                .Cast<FilterIntervales>()
                .Select(e => new
                {
                    Value = (int)e,
                    DisplayName = e.ToString()
                })
                .ToList();

            return Ok(list);
        }
    }
}