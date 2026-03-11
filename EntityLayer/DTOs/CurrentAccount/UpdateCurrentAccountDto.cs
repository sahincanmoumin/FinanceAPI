using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Enums;

namespace EntityLayer.DTOs.CurrentAccount
{
    public class UpdateCurrentAccountDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public AccountType Type { get; set; }
    }
}