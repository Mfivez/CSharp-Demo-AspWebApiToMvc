# La couche **Domain**

## Rôles de la couche Domain

> **Le Domain contient ce que le système manipule et comprend :
> Les entités métier et les contrats (interfaces) de manipulation 
> de ces entités.**
> 
> Le Domain ne sait RIEN de :
> 
> * SQL
> * HTTP
> * ASP.NET
> * config, connexion string, etc

Ici, notre métier est très simple: des **Users**.

Donc le Domain va contenir :

1. Une classe `User`
2. Une (ou plus) interface(s) `IUserRepository` (contrat pour manipuler des `User`).

---

## Entités métier :

### `User`
- Projet : 'DemoUser.Domain'
- Dossier : `Entities`
- Fichier : `User.cs`

> Ici pendant le cours, nous avons reconstitué l'entité User (voir fichier).

### Contrat de repository `IUserRepo`

- Projet : 'DemoUser.Domain'
- Dossier : `Repositories`
- Fichier : `IUserRepo.cs`

## Idées clées pour le Domain
* **Interface seulement** : pas d'implémentation ici.
* Ca décrit **ce que** l'on peut faire avec des `User`, pas **comment** on le fait
* La couche **DAL** devra fournir une classe qui implémente cette interface(ex: `SqlUserRepository`)