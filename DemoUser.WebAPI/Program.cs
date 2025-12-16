using DemoUser.BLL.Services.Implementations;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.DAL.Repositories;
using DemoUser.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAL + BLL
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddScoped<ITodoRepo>(_ => new SqlTodoRepo(cs));
builder.Services.AddScoped<ITodoService, TodoService>();

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

app.MapControllers();

app.Run();
