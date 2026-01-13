using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FL.Basecode.DTOs.Authentication;
using FL.Basecode.DTOs.Authentication.Register;


namespace FL.Basecode.Services.Interfaces
{
    public interface IAuthService
    {
        Task<StatusResponse> RegisterAsync(RegisterRequest request);

        Task<StatusResponse> RegisterWithGoogleAsync(GoogleAuthRequest request);

    }

}
