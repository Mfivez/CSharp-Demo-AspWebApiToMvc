# La couche DAL (Data Access Layer)

## Rôles de la DAL

> **La DAL est la couche qui sait parelr à la base de données.**
> Elle implémente les interfaces du Domain (`IUserRepository`) 
> en utilisant :
> 
> * SQL
> * ADO.NET (SqlConnection, SqlCommand)

Elle **ne contient pas de logique métier**, juste :

* "Comment je lis/écris des `User` dans la base de donnée".

On garde **UNE seule classe `User`** (celle du Domain), on ne 
crée pas de `DalUser`.

## Base de données : table + objets SQL 

> Ici on crée les tables, et les différentes fonctions.


## Principes de la DAL côté C#

- Projet : DemoUser.DAL

On va :

1. Ajouter la référence à 'Microsoft.Data.SqlClient' (NuGet si besoin).
2. Créer une classe `SqlUserRepository` qui implémente `IUserRepository`.
3. Utiliser une **connection string** passée par le constructeur (qu'on injectera plus tard depuis ASP)

## Impémentation du `SqlUserRepository`

* Aller voir l'implémentation dans le fichier dans le code 

## Points importants

* On n'a **pas d'entité DAL** on manipule **directement** `User`.
* Toute la conversion SQL -> `User` est dans un **seul endroit** :
    - MapUser();
* On utilise using pour ne pas oublier de ferme les connexions. 
* La DAL ne valide que le minimum technique (Email/password non vides); Le reste ira en BLL

## Où on en est maintenant :

```text

DemoUser.Domain :
    - Entities/User.cs
    - Repositories/IUserRepo.cs & co..
    
DemoUser.DAL
    - Repositories/SqlUserRepository.cs
```

**La DAL sait maintenant :**

* Lire tous les users.
* Lire un user par Id
* Insérer un user (avec hash dans la DB)
* Désactiver un user.
* Vérifier un login.