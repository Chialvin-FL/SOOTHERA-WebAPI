using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Threading.Tasks;

namespace FL.Basecode.Utilities.Firebase
{
    public class FirebaseAuthClientHelper
    {
        private readonly FirebaseAuthClient _authClient;

        public string ApiKey { get; }

        public FirebaseAuthClientHelper()
        {
            // Initialize FirebaseAuthClient with your API key and Auth Domain
            ApiKey = "AIzaSyB7ab9JSHKN9l5XfnR_NZx9aXcVfhW3qjU";

            _authClient = new FirebaseAuthClient(new FirebaseAuthConfig
            {
                ApiKey = ApiKey,
                AuthDomain = "sootheradb.firebaseapp.com"
            });
        }

        // =========================
        // EMAIL & PASSWORD LOGIN
        // =========================
        public async Task<UserCredential> LoginAsync(string email, string password)
        {
            return await _authClient.SignInWithEmailAndPasswordAsync(email, password);
        }

        //// =========================
        //// GET USER FROM ID TOKEN
        //// =========================
        //public async Task<User> GetUserAsync(string idToken)
        //{
        //    return await _authClient.GetUserAsync(idToken);
        //}

        //// =========================
        //// REFRESH TOKEN
        //// =========================
        //public async Task<UserCredential> RefreshTokenAsync(string refreshToken)
        //{
        //    return await _authClient.RefreshTokenAsync(refreshToken);
        //}
    }
}
