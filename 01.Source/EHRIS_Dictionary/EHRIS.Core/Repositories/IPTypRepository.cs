using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{ 
    public interface IPTypeRepository 
    {
        Task<List<string>> GetPtypeNameListAsync(List<int> ptyNoList);
    }
}
