# 🥅 Création du projet & Mise en place de l'architecture

> Avant de coder, on commence par **structurer correctement la solution**

Une bonne architecture facilite :
- La lisibilité
- Les tests
- L'évolution du projet
- La séparation des responsabilités.

# 1. Architecture choisie :
> Nous allons choisir une architecture en couches classiques :

```
ASP -> BLL -> DAL -> Domain
```

Avec un principe fondamental :

> **Chaque couche ne dépend que de la couche située directement en dessous.**
> L'interface utilisateur (ASP .NET) ne parle jamais directement à la base de données.

---

# 2. Structure finale attendue :

Voici ce que nous voulons obtenir :

```
DemoUser.sln
|
├─ DemoUser.Domain -> Entités métiers + Interfaces
├─ DemoUser.DAL -> Accès DB : SQL, repositories
├─ DemoUser.BLL -> Logique métier, services
└─ DemoUser.ASP -> MVC : contrôleurs, vues
```

### Pourquoi séparer en couches ?

* **Domain** contient la vérité métier (entités, règles générales).
* **DAL** implémente l'accès aux données sans polluer le métier.
* **BLL** applique les validations et orchestrations métier.
* **ASP** gère les interactions utilisateur (pages, formulaires).

Cette isolation permet de modifier l'UI ou la DB sans casser le métier.

---



