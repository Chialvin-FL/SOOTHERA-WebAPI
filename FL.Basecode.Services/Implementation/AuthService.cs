using FirebaseAdmin.Auth;
using FL.Basecode.Services.Interfaces;
using FL.Basecode.Utilities.Firebase;
using FL.Basecode.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FL.Basecode.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseAuthHelper _firebase;

        public AuthService(FirebaseAuthHelper firebase)
        {
            _firebase = firebase;
        }

        public async Task<StatusResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var uid = await _firebase.RegisterAsync(
                    request.Email,
                    request.Password
                );

                var verificationLink =
                    await _firebase.GenerateVerificationLinkAsync(request.Email);

                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 201,
                    Message = "Registration successful. Verify your email.",
                    Data = new
                    {
                        UID = uid,
                        request.Email,
                        VerificationLink = verificationLink
                    }
                };
            }
            catch (FirebaseAuthException ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message
                };
            }
        }
    }

}
