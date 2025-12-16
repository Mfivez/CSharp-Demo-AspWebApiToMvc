# 🧩 Les sessions : donner une “mémoire” à notre application

> Objectif de cette partie :
> * Comprendre **ce qu’est une session**, **pourquoi on en a besoin**,
> * et **ce qu’on va ajouter dans notre projet démo** étape par étape.

---

## 1. Le problème : le web n’a pas de mémoire

### 1.1. HTTP est “sans état” (stateless)

Quand tu navigues sur un site web, ton navigateur envoie des **requêtes HTTP** au serveur.

Chaque requête est **indépendante** des autres :

* le serveur reçoit une requête, la traite, renvoie une réponse
* puis il “oublie” tout
* la requête suivante est vue comme un **nouveau** appel

->  **HTTP ne se souvient pas que tu étais déjà là avant.**

### 1.2. Pourquoi c’est un problème ?

Imagine un site où tu te connectes (login) :

1. Tu envoies ton email + mot de passe.
2. Le serveur vérifie dans la base de données.
3. Il te répond “Bonjour !” si tout va bien.

Très bien… **mais après ?**

* Si tu vas sur `/User/Index`, comment le serveur sait-il qui tu es ?
* Comment sait-il que tu es déjà “connecté” ?

Sans mécanisme complémentaire :

> **À chaque requête, le serveur doit tout recommencer.**

---

## 2. La solution : les sessions

### 2.1. Idée simple

Une **session**, c’est comme un **badge** ou un **ticket** :

* Quand tu te connectes avec ton email/mot de passe, le serveur te donne un **jeton (token)** unique.
* Le serveur **se souvient**, côté base de données :

    * à quel utilisateur appartient ce token
    * depuis quand il est valide
    * jusqu’à quand il est valable (expiration)

Ensuite :

* le navigateur renverra ce **token** à chaque requête (souvent via un cookie)
* le serveur regarde ce token, retrouve la session en base, et sait :
  -> “C’est Jean / Aziza / tel utilisateur”.

### 2.2. Dans notre démo

Notre application fait déjà :

* `User.Register` → l’utilisateur est créé en DB
* `User.Login` → on vérifie email + mot de passe

Pour l’instant, après le login, on ne fait qu’afficher un message :

> “Welcome xxx !”

Mais **il n’y a pas de vraie notion de “session”**.

- Ce qu’on va ajouter :

1. Une table **Session** en DB, liée à `User`.
2. Une entité `Session` dans la couche **Domain**.
3. Un `ISessionRepo` et une implémentation DAL qui utilise :
    * `DbConnection` / `DbCommand`
    * des requêtes SQL **directes**, pas de procédures stockées.
4. Un `SessionService` dans la **BLL**.
5. Dans le `UserController`, à chaque **Login réussi** :
   → on crée une **session** en DB.

> **Note importante** :
> Pour la démo, on ne va pas encore manipuler les cookies,
> mais on montrera déjà la création de la session côté serveur.

---

## 3. Ce qu’on va stocker dans une session

On va créer une table `Session` dans la DB, reliée à `User` :

* 1 utilisateur → peut avoir **plusieurs sessions** (ex : plusieurs navigateurs / appareils)

### 3.1. Schéma simplifié

```text
User
  Id (GUID)
  Email
  Password
  Salt
  CreatedAt
  DisabledAt (nullable)

Session
  Id (GUID)
  UserId (GUID) -> FK vers User.Id
  Token (GUID)
  CreatedAt (DateTime)
  ExpiresAt (DateTime)
  RevokedAt (nullable)
```

Schématiquement :

```text
User (1) ----- (n) Session
```

### 3.2. À quoi servent les colonnes ?

* `UserId` : identifie **quel utilisateur** est connecté.
* `Token` : identifiant unique de la session (comme un badge).
* `CreatedAt` : quand la session a été créée (login).
* `ExpiresAt` : date/heure d’expiration (ex : maintenant + 4h).
* `RevokedAt` : si on “révoque” la session (déconnexion, invalidation), on met une date ici.

Plus tard, on pourra faire :

* afficher les sessions actives d’un utilisateur
* vérifier si un token est valide
* faire un “Logout” qui met `RevokedAt` à maintenant

---

## 4. Étape 1 : créer la table Session en SQL

On ajoute ce script SQL dans la base de données :

```sql
CREATE TABLE [dbo].[Session]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Session PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Token] UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_Session_Token UNIQUE,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [ExpiresAt] DATETIME2 NOT NULL,
    [RevokedAt] DATETIME2 NULL,

    CONSTRAINT FK_Session_User FOREIGN KEY (UserId)
        REFERENCES [dbo].[User] ([Id])
);
```

Description :

* `UNIQUEIDENTIFIER` = type GUID (identifiant unique).
* `DEFAULT NEWID()` = la DB génère un nouvel Id si on n’en fournit pas.
* clé étrangère `FK_Session_User` → assure que `UserId` existe dans `[User]`.

---

## 5. Étape 2 : la couche Domain – l’entité Session

Dans le **Domain**, on crée une classe `Session` qui représente une session métier.

```csharp
public class Session
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Guid Token { get; }
    public DateTime CreatedAt { get; }
    public DateTime ExpiresAt { get; }
    public DateTime? RevokedAt { get; }

    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    // Constructeur pour hydratation depuis la DB
    public Session(Guid id, Guid userId, Guid token,
                   DateTime createdAt, DateTime expiresAt, DateTime? revokedAt)
    {
        Id = id;
        UserId = userId;
        Token = token;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }

    // Constructeur pour création d'une nouvelle session côté BLL
    public Session(Guid userId, TimeSpan lifetime)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = CreatedAt.Add(lifetime);
    }
}
```

Idées pédagogiques :

* On garde **la même philosophie** que pour `User` : une entité métier simple.
* La propriété calculée `IsActive` résume la logique :
  une session est active si **non révoquée** et **pas expirée**.

---

## 6. Étape 3 : la DAL avec DbConnection et SQL direct

### 6.1. Rappel : ce qu’on faisait avant

Pour les `User`, dans la DAL on avait :

* `SqlConnection`
* `SqlCommand`
* `CommandType.StoredProcedure`
* des procédures stockées : `SP_User_GetAll`, `SP_User_Insert`, etc.

Exemple (simplifié) :

```csharp
using var connection = new SqlConnection(_connectionString);
using var command = new SqlCommand("SP_User_GetAll", connection);
command.CommandType = CommandType.StoredProcedure;
```

### 6.2. Ce qu’on va faire pour les Session

On va changer deux choses :

1. Utiliser **`DbConnection` / `DbCommand`** (types abstraits).
2. Utiliser des **requêtes SQL directes** (`CommandType.Text`).

#### Pourquoi `DbConnection` ?

* `DbConnection` est une **classe abstraite** commune à tous les providers (SQL Server, MySQL, PostgreSQL…).
* Notre code DAL devient un peu plus **générique**.
* Pour l’instant, on instancie quand même une `SqlConnection`,
  mais on la manipule via le type `DbConnection`.

#### Pourquoi SQL direct ?

* Pour montrer une autre manière d’accéder à la DB.
* Pas de procédure stockée : on écrit la requête SQL dans le code.

### 6.3. Exemple de repo : `SqlSessionRepo`

Version simplifiée : on se concentre sur **Insert** et **GetByToken**.

```csharp
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

public class SqlSessionRepo
{
    private readonly string _connectionString;

    public SqlSessionRepo(string connectionString)
    {
        _connectionString = connectionString;
    }

    private DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public Session Insert(Session session)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = @"
            INSERT INTO [Session] (Id, UserId, Token, CreatedAt, ExpiresAt, RevokedAt)
            VALUES (@Id, @UserId, @Token, @CreatedAt, @ExpiresAt, @RevokedAt);

            SELECT Id, UserId, Token, CreatedAt, ExpiresAt, RevokedAt
            FROM [Session]
            WHERE Id = @Id;
        ";

        AddParameter(command, "@Id", session.Id);
        AddParameter(command, "@UserId", session.UserId);
        AddParameter(command, "@Token", session.Token);
        AddParameter(command, "@CreatedAt", session.CreatedAt);
        AddParameter(command, "@ExpiresAt", session.ExpiresAt);
        AddParameter(command, "@RevokedAt", (object?)session.RevokedAt ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("Failed to insert session.");

        return MapSession(reader);
    }

    public Session? GetByToken(Guid token)
    {
        using var connection = CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = @"
            SELECT Id, UserId, Token, CreatedAt, ExpiresAt, RevokedAt
            FROM [Session]
            WHERE Token = @Token;
        ";

        AddParameter(command, "@Token", token);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return MapSession(reader);
    }

    private static Session MapSession(IDataRecord record)
    {
        return new Session(
            id: (Guid)record["Id"],
            userId: (Guid)record["UserId"],
            token: (Guid)record["Token"],
            createdAt: (DateTime)record["CreatedAt"],
            expiresAt: (DateTime)record["ExpiresAt"],
            revokedAt: record["RevokedAt"] is DBNull ? null : (DateTime?)record["RevokedAt"]
        );
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
```

Points pédagogiques importants :

* `CreateConnection()` renvoie un **DbConnection**, mais créé avec `new SqlConnection(...)`.
* `CommandType.Text` → on écrit la requête SQL dans `CommandText`.
* On utilise toujours des **paramètres** (`@Token`, `@UserId`, etc.) pour éviter l’injection SQL.

---

## 7. Étape 4 : la BLL – SessionService

Pour rester cohérent avec l’architecture :

* la DAL fait le “comment”
* la BLL fait le “quoi”

On crée un `SessionService` qui :

* permet de **créer une session** pour un utilisateur donné
* peut retrouver une session par `Token` (plus tard)
* pourrait lister les sessions d’un utilisateur, ou les révoquer

Version simplifiée :

```csharp
public class SessionService
{
    private readonly SqlSessionRepo _sessionRepo;

    public SessionService(SqlSessionRepo sessionRepo)
    {
        _sessionRepo = sessionRepo;
    }

    public Session CreateForUser(Guid userId, TimeSpan lifetime)
    {
        var session = new Session(userId, lifetime);
        return _sessionRepo.Insert(session);
    }
}
```

---

## 8. Étape 5 : ASP – créer une session au login

Dans le `UserController`, après un login réussi, on ajoute la création d’une session.

Idée :

1. L’utilisateur entre email + mot de passe.
2. `_userService.Login` valide le couple (email / mdp).
3. Si OK, on appelle `_sessionService.CreateForUser(user.Id, TimeSpan.FromHours(4));`
4. Pour la démo, on affiche le token dans un message.

Exemple (version pseudo-code) :

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Login(UserLoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = _userService.Login(model.Email, model.Password);

    if (user == null)
    {
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // Création d'une session pour cet utilisateur (valable 4h)
    var session = _sessionService.CreateForUser(user.Id, TimeSpan.FromHours(4));

    // Pour la démo : on affiche le token dans un message.
    TempData["LoginMessage"] = $"Welcome {user.Email}! Session token: {session.Token}";

    return RedirectToAction(nameof(Index));
}
```

> Dans une **vraie appli**, on ne se contenterait pas d’afficher le token :
>
> * on le stockerait dans un **cookie sécurisé**
> * à chaque requête, on lirait ce cookie, vérifierait la session, etc.

Le but ici est de comprendre :

* qu’une session est **créée**
* qu’elle est **stockée en DB**
* qu’elle est **liée à un user**

---

## 9. Récap : ce qu’il faut retenir

1. **Le web (HTTP) est sans mémoire.**
   Chaque requête est indépendante.

2. **Une session donne une “identité” persistante** entre plusieurs requêtes.
   On stocke un **token** côté client, et une **Session** côté serveur.

3. **Notre base de données a maintenant deux tables principales :**

    * `User` : les comptes d’utilisateurs
    * `Session` : les sessions de connexion, reliées à `User`

4. **Dans l’architecture 3 couches :**

   ```text
   ASP (UserController) → SessionService (BLL) → SqlSessionRepo (DAL) → SQL (table Session)
   ```

5. **Deux façons d’accéder à la DB :**

    * via `SqlConnection` + procédures stockées (ce qu’on avait déjà pour `User`)
    * via `DbConnection` + SQL direct (ce qu’on met en place pour `Session`)

6. **Pour l’instant, on ne fait pas encore de “vraie authentification complète”**,
   mais on pose les bases : les sessions sont là, en DB, prêtes à être utilisées.

---
