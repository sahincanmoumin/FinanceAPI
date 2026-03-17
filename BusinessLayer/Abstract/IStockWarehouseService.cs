using EntityLayer.DTOs.StockWarehouse;
using EntityLayer.Entities;
using EntityLayer.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface IStockWarehouseService
    {
        Task UpdateBalanceAsync(int stockId, int warehouseId, decimal quantity, ReceiptType type);
        Task UpdateTransferBalanceAsync(int stockId, int fromWarehouseId, int toWarehouseId, decimal quantity);
        Task<List<StockWarehouseListDto>> GetStockStatusByWarehouseAsync(int warehouseId);
    }
}