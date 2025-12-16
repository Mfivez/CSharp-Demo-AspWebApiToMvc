## Couche BLL (Business Logic Layer)

### Rôle de la BLL

> **La BLL orchestre la logique métier.**
> Elle utilise la DAL pour accéder aux données, applique des règles (validation, décisions), et expose des méthodes propres pour l’UI.

En résumé :

* **DAL** : *“Comment je parle à la DB ?”*
* **BLL** : *“Qu’est-ce que j’ai le droit de faire et dans quel ordre ?”*
* **ASP** : *“Que demande l’utilisateur et qu’est-ce que je lui affiche ?”*

Ici, la BLL va :

* valider email / mot de passe
* appeler `IUserRepository`
* retourner des `User` (domain) ou des résultats simples (bool, Guid, etc.)

---

## Interface `IUserService`

* Projet : `DemoUser.BLL`
* Dossier : `Services\interfaces`
* Fichier : `IUserService.cs`

On définit les **use-cases** que l’UI utilisera :

> Ici, mettre en place les interfaces des services dans la BLL.

---

## Implémentation `UserService`

* Projet : `DemoUser.BLL`
* Dossier : `Services\implementations`
* Fichier : `UserService.cs`

### Ce que fait la BLL

* La BLL **n’implémente pas** un repository, elle expose un *service métier* (`Register`, `Login`, etc.).
* **Une seule entité `User`** partagée par tout le monde.
* La DAL est cachée derrière `IUserRepository`, l’ASP ne sait même pas qu’il y a du SQL.

---

## Diagramme mental

* **Domain** :
    * `User`
    * `IUserRepository`
* **DAL** :
    * `SqlUserRepository : IUserRepository`
* **BLL** :
    * `UserService : IUserService` utilise `IUserRepository`
* **ASP** :
    * contrôleurs MVC utilisent `IUserService`

---

## Où on en est maintenant

Architecture fonctionnelle :

```text
ASP → IUserService / UserService
       ↓
      IUserRepository / SqlUserRepository
       ↓
      SQL (table + SP)
```