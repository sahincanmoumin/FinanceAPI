using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EntityLayer.DTOs.ErrorDetails
{
    public class ErrorDetails
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{{\"Message\": \"{Message}\"}}";
        }
    }
}