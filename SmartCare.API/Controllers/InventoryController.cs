using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCare.API.Helpers;
using SmartCare.Application.CQRs.Inventory.Commands;
using SmartCare.Application.CQRs.Inventory.Queries;
using SmartCare.Application.DTOs.Companies.Responses;
using SmartCare.Application.DTOs.Inventory.Request;
using SmartCare.Application.DTOs.Inventory.Response;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Domain.Entities;
using static SmartCare.API.Helpers.ApplicationRouting;

namespace SmartCare.API.Controllers
{
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
       // private readonly IinventoryService _InventoryService;
        private readonly IMediator _mediator;

        public InventoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //public InventoryController(IinventoryService inventoryService)
        //{
        //    _InventoryService = inventoryService;
        //}


        [HttpGet(ApplicationRouting.Inventory.GetBestByProductId)]
        [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBestInventoryId(Guid ProductId , int quantityRequired)
        {
            //var result = await _InventoryService.GetBestInventoryId(ProductId , quantityRequired);
            var result = await _mediator.Send(new GetBestInventoryIdQuery(ProductId, quantityRequired));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpGet(ApplicationRouting.Inventory.GetAvailableByProductId)]
        [ProducesResponseType(typeof(Response<List<InventoryUserResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableInventoriesForProduct(Guid productId)
        {
            //var result = await _InventoryService.GetAvailableInventoriesForProduct(productId);
            var result = await _mediator.Send(new GetAvailableInventoriesForProductQuery(productId));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpGet(ApplicationRouting.Inventory.GetTotalStockByProductId)]
        [ProducesResponseType(typeof(Response<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTotalStockForProduct(Guid productId)
        {
            //var result = await _InventoryService.GetTotalStockForProduct(productId);
            var result = await _mediator.Send(new GetTotalStockForProductQuery(productId));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpGet(ApplicationRouting.Inventory.GetStockInStore)]
        [ProducesResponseType(typeof(Response<InventoryUserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStockOfProductInStore(Guid productId, Guid storeId)
        {
            //var result = await _InventoryService.GetStockOfProductInStore(productId, storeId);
            var result = await _mediator.Send(new GetStockOfProductInStoreQuery(productId, storeId));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpGet(ApplicationRouting.Inventory.GetAllInStore)]
        [ProducesResponseType(typeof(Response<List<InventoryAdminResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllInventoryInStore(Guid storeId, int pageNumber, int pageSize)
        {
            //var result = await _InventoryService.GetAllInventoryInStore(storeId, pageNumber, pageSize);
            var result = await _mediator.Send(new GetAllInventoryInStoreQuery(storeId, pageNumber, pageSize));
            return ControllersHelperMethods.FinalResponse(result);
        }




        [HttpGet(ApplicationRouting.Inventory.GetLowStock)]
        [ProducesResponseType(typeof(Response<List<InventoryAdminResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowStockItemsAsync(int threshold)
        {
            //var result = await _InventoryService.GetLowStockItemsAsync(threshold);
            var result = await _mediator.Send(new GetLowStockItemsAsyncQuery(threshold));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpGet(ApplicationRouting.Inventory.GetLowStockInStore)]
        [ProducesResponseType(typeof(Response<List<InventoryAdminResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLowStockItemsInStoreAsync(int threshold, Guid storeId)
        {
            //var result = await _InventoryService.GetLowStockItemsInStoreAsync(threshold, storeId);
            var result = await _mediator.Send(new GetLowStockItemsInStoreAsyncQuery(threshold, storeId));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpPost(ApplicationRouting.Inventory.Create)]
        [ProducesResponseType(typeof(Response<InventoryAdminResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateInventoryAsync(CreateInventoryRequestDto inventoryDto)
        {
            //var result = await _InventoryService.CreateInventoryAsync(inventoryDto);
            var result = await _mediator.Send(new CreateInventoryAsyncCommand(inventoryDto));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPut(ApplicationRouting.Inventory.IncreaseStock)]
        [ProducesResponseType(typeof(Response<InventoryAdminResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> IncreaseProductStock(Guid InventoryId, int quantityToAdd)
        {
            //var result = await _InventoryService.IncreaseProductStock(InventoryId , quantityToAdd);
            var result = await _mediator.Send(new IncreaseProductStockCommand(InventoryId, quantityToAdd));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPut(ApplicationRouting.Inventory.DecreaseStock)]
        [ProducesResponseType(typeof(Response<InventoryAdminResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DecreaseProductStock(Guid InventoryId, int quantityToSubtract)
        {
            //var result = await _InventoryService.DecreaseProductStock(InventoryId , quantityToSubtract);
            var result = await _mediator.Send(new DecreaseProductStockCommand(InventoryId ,quantityToSubtract));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPut(ApplicationRouting.Inventory.Update)]
        [ProducesResponseType(typeof(Response<InventoryAdminResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateInventoryAsync(UpdateInventoryRequestDto inventoryDto)
        {
            //var result = await _InventoryService.UpdateInventoryAsync(inventoryDto);
            var result = await _mediator.Send(new UpdateInventoryAsyncCommand(inventoryDto));
            return ControllersHelperMethods.FinalResponse(result);
        }
        [HttpPatch(ApplicationRouting.Inventory.Reserve)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReserveStockAsync(Guid inventoryId, int quantity)
        {
            //var result = await _InventoryService.ReserveStockAsync(inventoryId, quantity);
            var result = await _mediator.Send(new ReserveStockAsyncCommand(inventoryId, quantity));
            return ControllersHelperMethods.FinalResponse(result);
        }



        [HttpPatch(ApplicationRouting.Inventory.ReleaseReserved)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReleaseReservedStockAsync(Guid inventoryId, int quantity)
        {
            //var result = await _InventoryService.ReleaseReservedStockAsync(inventoryId, quantity);
            var result = await _mediator.Send(new ReleaseReservedStockAsyncCommand(inventoryId, quantity));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPatch(ApplicationRouting.Inventory.TransferStock)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferStockAsync(Guid fromInventoryId, Guid toInventoryId, int quantity)
        {
            //var result = await _InventoryService.TransferStockAsync(fromInventoryId, toInventoryId, quantity);
            var result = await _mediator.Send(new TransferStockAsyncCommand(fromInventoryId, toInventoryId, quantity));
            return ControllersHelperMethods.FinalResponse(result);
        }


        [HttpPatch(ApplicationRouting.Inventory.SetStock)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetStockLevelAsync(Guid inventoryId, int newQuantity)
        {
           //var result = await _InventoryService.SetStockLevelAsync(inventoryId , newQuantity);
            var result = await _mediator.Send(new SetStockLevelAsyncCommand(inventoryId, newQuantity));
            return ControllersHelperMethods.FinalResponse(result);
        }

        [HttpDelete(ApplicationRouting.Inventory.Delete)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteInventoryAsync(Guid Id)
        {
            //var result = await _InventoryService.DeleteInventoryAsync(Id);
            var result = await _mediator.Send(new  DeleteInventoryAsyncCommand(Id));
            return ControllersHelperMethods.FinalResponse(result);
        }


    }
}
