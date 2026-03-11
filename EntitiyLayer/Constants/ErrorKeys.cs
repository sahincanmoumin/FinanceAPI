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
        public const string CurrentAccountNotFound = "CurrentAccountNotFound";
        public const string InvoiceNotFound = "InvoiceNotFound";
        public const string InvoiceAlreadyApproved = "InvoiceAlreadyApproved";
        public const string InsufficientStock = "InsufficientStock"; 
        public const string InvalidTransaction = "InvalidTransaction";
    }
}