using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Constants
{
    public static class ErrorKeys
    {
        public const string UserNotFound = "UserNotFound";
        public const string InvalidCredentials = "InvalidCredentials";
        public const string CompanyNotFound = "CompanyNotFound";
        public const string StockNotFound = "StockNotFound";
        public const string StockAlreadyExists = "StockAlreadyExists";
        public const string CurrentAccountNotFound = "CurrentAccountNotFound";
        public const string CurrentAccountAlreadyExists = "CurrentAccountAlreadyExists";
        public const string InvoiceNotFound = "InvoiceNotFound";
        public const string InvoiceAlreadyApproved = "InvoiceAlreadyApproved";
        public const string InsufficientStock = "InsufficientStock"; 
        public const string InvalidTransaction = "InvalidTransaction";
        public const string CompanyAlreadyExists = "CompanyAlreadyExists";
        public const string UserAlreadyExists = "UserAlreadyExists";
        public const string RoleNotFound = "RoleNotFound";
        public const string RoleAlreadyExists = "RoleAlreadyExists";
        public const string WrongPassword = "WrongPassword";
        public const string UserNameAlreadyExists = "UserNameAlreadyExists";
        public const string UserRoleNotFound = "UserRoleNotFound";
        public const string InvalidAccountType = "InvalidAccountType";
        public const string Unauthorized = "Unauthorized";
        public const string OnlyDraftInvoicesCanBeApproved = "OnlyDraftInvoicesCanBeApproved";
        public const string InvoiceNotDraft= "InvoiceNotDraft";
        public const string WarehouseNotFound = "WarehouseNotFound";
        public const string WarehouseAlreadyExists = "WarehouseAlreadyExists";
        public const string MainWarehouseAlreadyExists = "MainWarehouseAlreadyExists";
        public const string ReceiptNotFound = "ReceiptNotFound";
        public const string ReceiptAlreadyApproved = "ReceiptAlreadyApproved";
        public const string ReceiptCannotBeModified ="ReceiptCannotBeModified";
        public const string OnlyDraftReceiptsCanBeApproved = "OnlyDraftReceiptsCanBeApproved";
        public const string TransferReceiptTargetWarehouseRequired ="TransferReceiptTargetWarehouseRequired";
        public const string StockReceiptNotDraft = "StockReceiptNotDraft";
    }
}