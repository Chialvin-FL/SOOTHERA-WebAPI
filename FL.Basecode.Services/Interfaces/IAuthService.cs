using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FL.Basecode.DTOs;


namespace FL.Basecode.Services.Interfaces
{
    public interface IAuthService
    {
        Task<StatusResponse> RegisterAsync(RegisterRequest request);
    }

}
