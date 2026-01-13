using FL.Basecode.DTOs;
using FL.Basecode.DTOs.Authentication;
using FL.Basecode.DTOs.Authentication.Login;
using FL.Basecode.DTOs.Authentication.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FL.Basecode.Services.Interfaces
{
    public interface IAuthService
    {
        Task<StatusResponse> RegisterAsync(RegisterRequest request);

        Task<StatusResponse> RegisterWithGoogleAsync(GoogleAuthRequest request);

        Task<StatusResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

        Task<StatusResponse> LoginAsync(LoginRequest request);



    }

}
