## Rappel rapide : un JWT, c’est quoi ?

Un **JWT (JSON Web Token)** est un jeton signé qui sert à **prouver l’identité** d’un utilisateur ou d’un service.

Il contient :

* des **données** (claims)
* une **signature** pour garantir qu’il n’a pas été modifié

---

## 2 `issuer` (ou `iss`) : *qui a créé le token ?*

**Issuer = l’émetteur du JWT**

C’est **l’entité qui génère et signe le token**.

### Exemple

```json
"issuer": "DemoUser"
```

Ça veut dire :

> « Ce token a été émis par **DemoUser** »

### À quoi ça sert ?

Quand ton API reçoit un JWT, elle vérifie :

* que le token **vient bien de la source attendue**
* et pas d’un autre service ou d’un attaquant

**Sécurité** :
Si l’`issuer` ne correspond pas à ce que l’API attend → **token refusé**

---

## `audience` (ou `aud`) : *pour qui est le token ?*

**Audience = le destinataire du JWT**

C’est **l’application ou le service qui est censé utiliser le token**.

### Exemple

```json
"audience": "DemoUser"
```

Ça veut dire :

> « Ce token est destiné à être utilisé par **DemoUser** »

### À quoi ça sert ?

Empêcher qu’un token valide pour un service **A** soit utilisé sur un service **B**.

**Sécurité multi-services** :

* API Users → audience = `users-api`
* API Orders → audience = `orders-api`

Un token pour `users-api` **ne pourra pas** appeler `orders-api`.

---

## Exemple concret (très parlant)

### Cas réel

* Auth Server → crée le JWT
* API → consomme le JWT

```json
{
  "iss": "auth.mycompany.com",
  "aud": "api.mycompany.com",
  "sub": "123",
  "exp": 1710000000
}
```

✔ L’API vérifie :

* `iss` == `auth.mycompany.com`
* `aud` == `api.mycompany.com`

Si l’un des deux ne correspond pas → **401 Unauthorized**

---

## Dans ton cas précis

```json
"jwt": {
  "key": "CHANGE_ME_SUPER_SECRET_LONG_KEY",
  "issuer": "DemoUser",
  "audience": "DemoUser"
}
```

Ça veut dire :

* **DemoUser** crée le token
* **DemoUser** consomme le token

OK pour :

* un projet simple
* une API + frontend
* un environnement de test / démo

---

## Résumé ultra simple

| Champ      | Question                | Sert à                       |
| ---------- | ----------------------- | ---------------------------- |
| `issuer`   | Qui a créé le token ?   | Vérifier la **source**       |
| `audience` | Pour qui est le token ? | Vérifier le **destinataire** |

---
