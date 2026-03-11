using AutoMapper;
using EntityLayer.Entities.Domain;
using EntityLayer.Entities.Auth;
using EntityLayer.DTOs.Company;
using EntityLayer.DTOs.CurrentAccount;
using EntityLayer.DTOs.Stock;
using EntityLayer.DTOs.Invoice;
using EntityLayer.DTOs.InvoiceDetail;
using EntityLayer.DTOs.StockTrans;
using EntityLayer.DTOs.Auth;
using EntityLayer.DTOs.Role;

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

            CreateMap<Invoice, InvoiceListDto>().ReverseMap();
            CreateMap<CreateInvoiceDto, Invoice>();
            CreateMap<UpdateInvoiceDto, Invoice>();

            CreateMap<InvoiceDetail, InvoiceDetailListDto>().ReverseMap();
            CreateMap<CreateInvoiceDetailDto, InvoiceDetail>();
            CreateMap<UpdateInvoiceDetailDto, InvoiceDetail>();

            CreateMap<StockTrans, StockTransListDto>().ReverseMap();

            CreateMap<RegisterDto, User>();

            CreateMap<Role, RoleListDto>().ReverseMap();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            CreateMap<UserRole, UserRoleListDto>().ReverseMap();
            CreateMap<CreateUserRoleDto, UserRole>();
        }
    }
}