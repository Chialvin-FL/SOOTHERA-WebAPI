using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FL.Basecode.Services.Implementation;
using FL.Basecode.Services.Interfaces;
using FL.Basecode.Utilities.Firebase;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// Firebase configuration Local run
// ------------------------------
//string firebaseKeyPath = Path.Combine(
//    builder.Environment.ContentRootPath,
//    "sootheradb-firebase-adminsdk-fbsvc-075e4ce008.json"
//);


// ------------------------------
// Firebase configuration Deployed run
// ------------------------------
string firebaseKeyPath =
    @"D:\DZHosts\LocalUser\whyiamhated\Protected.dev-soothera-api.somee.com\sootheradb-firebase-adminsdk-fbsvc-075e4ce008.json";

if (!File.Exists(firebaseKeyPath))
{
    throw new FileNotFoundException("Firebase service account file not found.", firebaseKeyPath);
}


// OPTIONAL: only needed if other Google SDKs rely on it
Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    firebaseKeyPath
);

// Initialize Firebase ONCE
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseKeyPath)
    });
}

// ------------------------------
// Dependency Injection
// ------------------------------
builder.Services.AddSingleton(FirebaseAuth.DefaultInstance);

builder.Services.AddSingleton(provider =>
{
    return FirestoreDb.Create("sootheradb");
});

// Your app services
builder.Services.AddScoped<FirebaseAuthHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------------------
// HTTP pipeline
// ------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Run();
