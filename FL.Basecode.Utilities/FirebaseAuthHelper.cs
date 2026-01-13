using FirebaseAdmin.Auth;

namespace FL.Basecode.Utilities.Firebase
{
    public class FirebaseAuthHelper
    {
        private readonly FirebaseAuth _auth;

        public FirebaseAuthHelper(FirebaseAuth auth)
        {
            _auth = auth;
        }

        public async Task<string> VerifyTokenAsync(string token)
        {
            var decoded = await _auth.VerifyIdTokenAsync(token);
            return decoded.Uid;
        }

        public async Task<string> RegisterAsync(string email, string password)
        {
            var user = await _auth.CreateUserAsync(new UserRecordArgs
            {
                Email = email,
                Password = password,
                EmailVerified = false,
                Disabled = false
            });

            return user.Uid;
        }

        public async Task<string> GenerateVerificationLinkAsync(string email)
        {
            return await _auth.GenerateEmailVerificationLinkAsync(email);
        }

        public static async Task<FirebaseToken> VerifyGoogleIdTokenAsync(string idToken)
        {
            return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }

        public async Task<string> GeneratePasswordResetLinkAsync(string email)
        {
            return await _auth.GeneratePasswordResetLinkAsync(email);
        }
    }
}
