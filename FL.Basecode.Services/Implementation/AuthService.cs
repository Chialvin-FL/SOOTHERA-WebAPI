using FirebaseAdmin.Auth;
using FL.Basecode.DTOs.Authentication.Register;
using FL.Basecode.DTOs.Authentication;

using FL.Basecode.Services.Interfaces;
using FL.Basecode.Utilities.Firebase;
using System;

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
                // Create Firebase user
                var uid = await _firebase.RegisterAsync(request.Email, request.Password);

                // Generate email verification link
                var verificationLink = await _firebase.GenerateVerificationLinkAsync(request.Email);

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
                // Map Firebase error codes to API responses
                return ex.AuthErrorCode switch
                {
                    AuthErrorCode.EmailAlreadyExists => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 409,
                        Message = "Email is already registered."
                    },
                    AuthErrorCode.UserNotFound => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "User not found."
                    },
                    _ => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = $"Registration failed: {ex.Message}"
                    }
                };
            }
            catch (Exception ex)
            {
                // Catch-all for other unexpected errors
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Internal server error: {ex.Message}"
                };
            }
        }

     public async Task<StatusResponse> RegisterWithGoogleAsync(GoogleAuthRequest request)
        {
            try
            {
                // Verify the Google ID token
                var decodedToken = await FirebaseAuthHelper.VerifyGoogleIdTokenAsync(request.IdToken);
                var uid = decodedToken.Uid;

                // Check if the user already exists in Firebase
                UserRecord user;
                try
                {
                    user = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                }
                catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    // Create a new Firebase user using Google info
                    user = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                    {
                        Uid = uid,
                        Email = decodedToken.Claims["email"]?.ToString(),
                        DisplayName = decodedToken.Claims["name"]?.ToString(),
                        PhotoUrl = decodedToken.Claims["picture"]?.ToString(),
                        EmailVerified = true
                    });
                }

                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Google authentication successful.",
                    Data = new
                    {
                        UID = user.Uid,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        PhotoUrl = user.PhotoUrl
                    }
                };
            }
            catch (FirebaseAuthException ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = ex.AuthErrorCode == AuthErrorCode.UserNotFound ? 404 : 500,
                    Message = $"Google authentication failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Internal server error: {ex.Message}"
                };
            }
        }
    }
}
