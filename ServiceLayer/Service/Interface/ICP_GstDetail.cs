using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Interface
{
    public interface ICP_GstDetail<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
    }
}
