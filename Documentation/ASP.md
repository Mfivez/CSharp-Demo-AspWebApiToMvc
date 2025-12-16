# Couche ASP (MVC : contrôleurs + vues)

### Rôle de la couche ASP

> **ASP = ce que voit et utilise l’utilisateur.**
> Elle :
>
> * reçoit des requêtes HTTP
> * affiche des pages (Razor)
> * envoie les données des formulaires à la BLL (via `IUserService`)

Elle **ne touche jamais** à la DAL directement.

---

## Brancher la BLL/DAL dans ASP (DI)

### Connection string

* Dans `ASP`, fichier `appsettings.json`, ajoute une section :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Connection String"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Enregistrer les services dans `Program.cs`

Dans `DemoUser.ASP/Program.cs`, ajoute les `using` en haut :

```csharp
using DemoUser.BLL.Services;
using DemoUser.DAL.Repositories;
using DemoUser.Domain.Repositories;
```

Puis dans `Main`, après :

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
```

on ajoute :

```csharp
// Enregistrer IUserRepository -> SqlUserRepository (DAL)
builder.Services.AddScoped<IUserRepository>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlUserRepository(connectionString!);
});

// Enregistrer IUserService -> UserService (BLL)
builder.Services.AddScoped<IUserService, UserService>();
```

---

## ViewModels (ce qu’on échange avec les vues)

* Projet : `DemoUser.ASP`
* Dossier : `Models`

> On ajoute les vues (voir dossier models dans le code)
---

## UserController

* Projet : `DemoUser.ASP`
* Dossier : `Controllers`
* Fichier : `UserController.cs`

> On ajoute les controllers (voirs dossier controllers dans le code)

> NB : on n’a pas encore de système d’auth complet dans cette démo !
> Là on se concentre sur le **CRUD + login côté BLL/DAL**.

---

## 5.4. Vues

> Dans le dossier vues, on ajoute de la même manière les différentes vues (cshtml, etc,..) voir code.
---

## 5.5. Vérifier le routing

Dans `Program.cs`, là où on a déjà :

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

On peut :

* soit aller à `/User/Index` dans le navigateur,
* soit modifier la route par défaut :

```csharp
pattern: "{controller=User}/{action=Index}/{id?}");
```

pour arriver direct sur la liste des users au lancement.

---

## Récap

On a maintenant :

* **Domain** : `User`, `IUserRepository`
* **DAL** : `SqlUserRepository` (+ SQL)
* **BLL** : `IUserService`, `UserService`
* **ASP** :

  * DI branchée (`IUserRepository` + `IUserService`)
  * `UserController`
  * ViewModels
  * Vues Razor `Index`, `Register`, `Login`

Un CRUD simplifié + login **propre** en 3-tiers.