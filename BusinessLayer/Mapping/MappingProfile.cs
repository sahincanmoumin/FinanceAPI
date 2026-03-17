using AutoMapper;
using EntityLayer.DTOs.Auth;
using EntityLayer.DTOs.Company;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.InvoiceDetail;
using EntityLayer.DTOs.Role;
using EntityLayer.DTOs.Stock;
using EntityLayer.DTOs.StockReceipt;
using EntityLayer.DTOs.StockReceiptDetail;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.DTOs.StockWarehouse;
using EntityLayer.DTOs.Warehouse;
using EntityLayer.Entities;
using EntityLayer.Entities.Auth;
using EntityLayer.Entities.Domain;

namespace BusinessLayer.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyListDto>().ReverseMap();
            CreateMap<CreateCompanyDto, Company>();
            CreateMap<UpdateCompanyDto, Company>();

            CreateMap<Stock, StockListDto>().ReverseMap();
            CreateMap<CreateStockDto, Stock>();
            CreateMap<UpdateStockDto, Stock>();

            CreateMap<CurrentAccount, CurrentAccountListDto>().ReverseMap();
            CreateMap<CreateCurrentAccountDto, CurrentAccount>();
            CreateMap<UpdateCurrentAccountDto, CurrentAccount>();

            CreateMap<Invoice, InvoiceListDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
                .ForMember(dest => dest.CurrentAccountName, opt => opt.MapFrom(src => src.CurrentAccount.Name));
            CreateMap<CreateInvoiceDto, Invoice>();
            CreateMap<UpdateInvoiceDto, Invoice>();

            CreateMap<InvoiceDetail, InvoiceDetailListDto>()
                .ForMember(dest => dest.StockName, opt => opt.MapFrom(src => src.Stock.Name));
            CreateMap<CreateInvoiceDetailDto, InvoiceDetail>();
            CreateMap<UpdateInvoiceDetailDto, InvoiceDetail>();

            CreateMap<StockTrans, StockTransListDto>().ReverseMap();

            CreateMap<RegisterDto, User>();

            CreateMap<Role, RoleListDto>().ReverseMap();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<UserRole, UserRoleListDto>().ReverseMap();
            CreateMap<CreateUserRoleDto, UserRole>();

            CreateMap<Warehouse, WarehouseListDto>();
            CreateMap<CreateWarehouseDto, Warehouse>();
            CreateMap<UpdateWarehouseDto, Warehouse>();

            CreateMap<StockReceipt, StockReceiptListDto>();
            CreateMap<CreateStockReceiptDto, StockReceipt>();
            CreateMap<CreateStockReceiptDetailDto, StockReceiptDetail>();
            CreateMap<UpdateStockReceiptDto, StockReceipt>();
            CreateMap<UpdateStockReceiptDetailDto, StockReceiptDetail>();
            CreateMap<StockReceiptDetail, StockReceiptDetailListDto>();

            CreateMap<StockWarehouse, StockWarehouseListDto>()
                .ForMember(dest => dest.StockName, opt => opt.MapFrom(src => src.Stock.Name))
                .ForMember(dest => dest.StockCode, opt => opt.MapFrom(src => src.Stock.Code))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name));
        }
    }
}