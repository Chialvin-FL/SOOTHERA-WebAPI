using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FL.Basecode.DTOs;
using FL.Basecode.DTOs.Authentication;
using FL.Basecode.DTOs.Authentication.Login;
using FL.Basecode.DTOs.Authentication.Register;
using FL.Basecode.Services.Interfaces;
using FL.Basecode.Utilities.Firebase;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

        // =========================
        // EMAIL & PASSWORD REGISTER
        // =========================
        public async Task<StatusResponse> RegisterAsync(RegisterRequest request)
        {
            // Basic validation (same philosophy as reference)
            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Invalid email format."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Password must be at least 6 characters long."
                };
            }

            try
            {
                var uid = await _firebase.RegisterAsync(request.Email, request.Password);
                var verificationLink =
                    await _firebase.GenerateVerificationLinkAsync(request.Email);

                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 201,
                    Message = "Registration successful. Please verify your email.",
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
                return ex.ErrorCode switch
                {
                    ErrorCode.AlreadyExists => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 409,
                        Message = "Email already registered."
                    },

                    ErrorCode.InvalidArgument => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid email or weak password."
                    },

                    _ => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = $"Authentication error: {ex.Message}"
                    }
                };
            }

            catch (Exception ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Unexpected error occurred: {ex.Message}"
                };
            }
        }

        // =========================
        // GOOGLE REGISTER / LOGIN
        // =========================
        public async Task<StatusResponse> RegisterWithGoogleAsync(GoogleAuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Google ID token is required."
                };
            }

            try
            {
                var decodedToken =
                    await FirebaseAuthHelper.VerifyGoogleIdTokenAsync(request.IdToken);

                var uid = decodedToken.Uid;
                UserRecord user;

                try
                {
                    user = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
                }
                catch (FirebaseAuthException ex)
                {
                    return ex.ErrorCode switch
                    {
                        ErrorCode.Unauthenticated => new StatusResponse
                        {
                            Success = false,
                            StatusCode = 401,
                            Message = "Invalid Google ID token."
                        },

                        ErrorCode.FailedPrecondition => new StatusResponse
                        {
                            Success = false,
                            StatusCode = 403,
                            Message = "User account is disabled."
                        },

                        _ => new StatusResponse
                        {
                            Success = false,
                            StatusCode = 500,
                            Message = ex.Message
                        }
                    };
                }


                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Google authentication successful.",
                    Data = new
                    {
                        UID = user.Uid,
                        user.Email,
                        user.DisplayName,
                        user.PhotoUrl
                    }
                };
            }
            catch (FirebaseAuthException ex)
            {
                return ex.ErrorCode switch
                {
                    ErrorCode.NotFound => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "No account found with that email."
                    },

                    ErrorCode.InvalidArgument => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid email address."
                    },

                    _ => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = ex.Message
                    }
                };
            }

            catch (Exception ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Unexpected error occurred: {ex.Message}"
                };
            }
        }

        // =========================
        // FORGOT PASSWORD
        // =========================
        public async Task<StatusResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                !new EmailAddressAttribute().IsValid(request.Email))
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Invalid email address."
                };
            }

            try
            {
                var resetLink =
                    await _firebase.GeneratePasswordResetLinkAsync(request.Email);

                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Password reset email sent. Please check your inbox.",
                    Data = new
                    {
                        request.Email,
                        ResetLink = resetLink
                    }
                };
            }
            catch (FirebaseAuthException ex)
            {
                return ex.ErrorCode switch
                {
                    ErrorCode.NotFound => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 404,
                        Message = "No account found with that email."
                    },

                    ErrorCode.InvalidArgument => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid email address."
                    },

                    _ => new StatusResponse
                    {
                        Success = false,
                        StatusCode = 500,
                        Message = ex.Message
                    }
                };
            }

            catch (Exception ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Unexpected error occurred: {ex.Message}"
                };
            }
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<StatusResponse> LoginAsync(LoginRequest request)
        {
            // ✅ Validate email and password
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(request.Email))
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Invalid email format."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Password is required."
                };
            }

            try
            {
                // ✅ Initialize FirebaseAuthClientHelper
                var authClient = new FirebaseAuthClientHelper();

                // ✅ Sign in with email & password
                var credential = await authClient.LoginAsync(request.Email, request.Password);

                // ✅ Get the ID token from the client SDK
                var idToken = credential.User.IdToken;

                // ✅ Verify token and get UID using Admin SDK
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = decodedToken.Uid;

                // ✅ Fetch full user info from Admin SDK
                var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

                if (!userRecord.EmailVerified)
                {
                    return new StatusResponse
                    {
                        Success = false,
                        StatusCode = 403,
                        Message = "Email not verified."
                    };
                }

                return new StatusResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Login successful.",
                    Data = new
                    {
                        UID = uid,
                        IdToken = idToken,
                        Email = userRecord.Email,
                        DisplayName = userRecord.DisplayName,
                        PhotoUrl = userRecord.PhotoUrl
                    }
                };
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                string message = ex.Message;
                int statusCode = 401;

                if (ex.Message.Contains("EMAIL_NOT_FOUND") || ex.Message.Contains("INVALID_PASSWORD"))
                {
                    message = "Incorrect email or password.";
                }
                else if (ex.Message.Contains("USER_DISABLED"))
                {
                    message = "Your account has been disabled.";
                    statusCode = 403;
                }

                return new StatusResponse
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                return new StatusResponse
                {
                    Success = false,
                    StatusCode = 500,
                    Message = $"Unexpected error occurred: {ex.Message}"
                };
            }
        }


    }
}
