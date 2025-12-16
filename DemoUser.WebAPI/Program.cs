using DemoUser.BLL.Services.Implementations;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.DAL.Repositories;
using DemoUser.Domain.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration["jwt:issuer"];
var jwtAudience = builder.Configuration["jwt:audience"];

var jwtKey = builder.Configuration["jwt:key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT secret key not configured. Set jwt:key.");
}

// Configure l’authentification de l’API en utilisant le schéma "Bearer JWT".
// Concrètement : l’API va s’attendre à recevoir un header HTTP
// Authorization: Bearer <token>
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Paramètres de validation appliqués à CHAQUE JWT reçu.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            /// <summary>
            /// Active la vérification de l’émetteur (claim "iss").
            /// </summary>
            /// <remarks>
            /// Si true, le token doit contenir un "iss" qui correspond à <see cref="ValidIssuer"/>.
            /// Sinon => token refusé (401).
            /// </remarks>
            ValidateIssuer = true,

            /// <summary>
            /// Active la vérification de l’audience (claim "aud").
            /// </summary>
            /// <remarks>
            /// Si true, le token doit contenir un "aud" qui correspond à <see cref="ValidAudience"/>.
            /// Sinon => token refusé (401).
            /// </remarks>
            ValidateAudience = true,

            /// <summary>
            /// Active la vérification de la durée de validité du token.
            /// </summary>
            /// <remarks>
            /// Valide les claims temporels : principalement "exp" (expiration) et parfois "nbf" (not before).
            /// Si le token est expiré ou pas encore valable => refusé.
            /// </remarks>
            ValidateLifetime = true,

            /// <summary>
            /// Active la vérification de la signature du token.
            /// </summary>
            /// <remarks>
            /// Si true, ASP.NET Core vérifie que le token a bien été signé avec la clé attendue
            /// (<see cref="IssuerSigningKey"/>). Si la signature ne correspond pas => refusé.
            /// </remarks>
            ValidateIssuerSigningKey = true,

            /// <summary>
            /// Valeur attendue pour l’émetteur (iss).
            /// </summary>
            /// <remarks>
            /// Exemple : "auth.mycompany.com" ou "DemoUser" ici.
            /// </remarks>
            ValidIssuer = jwtIssuer,

            /// <summary>
            /// Valeur attendue pour l’audience (aud).
            /// </summary>
            /// <remarks>
            /// Exemple : "api.mycompany.com" ou "DemoUser".
            /// </remarks>
            ValidAudience = jwtAudience,

            /// <summary>
            /// Clé utilisée pour valider la signature du JWT (HS256, HS512, etc.).
            /// </summary>
            /// <remarks>
            /// Ici on utilise une clé symétrique (même secret pour signer et vérifier).
            /// Important : la clé doit être longue et gardée secrète.
            /// </remarks>
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            /// <summary>
            /// Tolérance de décalage d’horloge (clock drift) pour les dates du token.
            /// </summary>
            /// <remarks>
            /// Sert surtout pour "exp" et "nbf".
            /// Exemple : si ClockSkew = 1 min, un token peut être accepté jusqu’à 1 min
            /// après exp (et/ou 1 min avant nbf), selon le cas.
            /// Ça évite des rejets liés à des serveurs pas parfaitement à l’heure.
            /// </remarks>
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Ajoute le système d’autorisation (policies, roles, [Authorize], etc.).
// Sans ça : tu peux authentifier, mais tu ne gères pas correctement les règles d’accès.
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DemoUser.WebAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez: Bearer {votre_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DAL + BLL
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddScoped<ITodoRepo>(_ => new SqlTodoRepo(cs));
builder.Services.AddScoped<IUserRepo>(_ => new SqlUserRepo(cs));
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IUserService, UserService>();

// ==============================
// CORS (Cross-Origin Resource Sharing)
// ==============================
//
// Le CORS est un mécanisme de sécurité APPLIQUÉ PAR LE NAVIGATEUR.
// Il empêche une page web (HTML/JS) hébergée sur une origine
// (schéma + domaine + port) d'appeler une API située sur une autre
// origine, sauf si le serveur l'autorise explicitement.
//
// Exemple :
// - MVC (navigateur) : http://localhost:5188
// - Web API          : http://localhost:5062
// → origines différentes ⇒ CORS bloqué par défaut
//
// Ce middleware indique AU NAVIGATEUR quelles origines
// ont le droit d'appeler l'API.
//

builder.Services.AddCors(opt =>
{
    // Déclaration d'une policy CORS nommée "MvcCors"
    opt.AddPolicy("MvcCors", p =>
    {
        // Origines autorisées à appeler l'API
        // Une origine = scheme + host + port
        //
        // Exemple :
        //   http://localhost:5188
        //   https://localhost:5188
        //
        // IMPORTANT :
        // - Ce n'est PAS une URL complète
        // - Pas de /api, pas de path
        // - Le port DOIT correspondre exactement
        //
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        p.WithOrigins(origins)

         // Autorise tous les headers HTTP envoyés par le navigateur
         // Exemples :
         // - Content-Type
         // - Authorization (JWT)
         // - Accept
         //
         // Sans ça :
         // → les requêtes POST/PUT avec JSON sont bloquées
         .AllowAnyHeader()

         // Autorise toutes les méthodes HTTP
         // GET, POST, PUT, DELETE, OPTIONS...
         //
         // Sans ça :
         // → seules les requêtes GET fonctionnent
         //
         // Le navigateur fait aussi une requête "OPTIONS"
         // (préflight) avant certains appels
         .AllowAnyMethod();
        //.WithMethods("GET", "POST", "PUT");
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("MvcCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
