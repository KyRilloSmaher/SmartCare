
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
    }
}