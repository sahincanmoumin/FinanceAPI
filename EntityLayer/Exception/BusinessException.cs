using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace EntityLayer.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorKey { get; }

        public BusinessException(string errorKey) : base(errorKey)
        {
            ErrorKey = errorKey;
        }
    }
}
