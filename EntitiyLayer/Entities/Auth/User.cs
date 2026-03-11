using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.Entities.Common;
using EntityLayer.Entities.Domain;

namespace EntityLayer.Entities.Auth
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }

        public ICollection<Company> Companies { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
