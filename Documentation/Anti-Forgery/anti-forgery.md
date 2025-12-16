````md
# Le token Anti-Forgery : pourquoi on en a besoin ?

Quand on envoie un formulaire dans une application ASP.NET MVC, on voit parfois ceci :

```csharp
[ValidateAntiForgeryToken]
public IActionResult Login(...)
````

et dans la vue Razor :

```html
<form method="post">
    @Html.AntiForgeryToken()
```

- Mais **à quoi ça sert ?**

---

## 1. Le problème : l’attaque CSRF

CSRF = **Cross-Site Request Forgery**
(en français : *“attaque par falsification de requête inter-site”*)

### Idée simple

Un site **malveillant** pourrait essayer de faire exécuter **une requête POST** sur ton serveur **à l’insu de l’utilisateur**.

Exemple :

1. L’utilisateur est connecté à ton site (il a un cookie valide).
2. Il visite un autre site malveillant.
3. Ce site envoie un formulaire *automatique* vers ton application :

```html
<form action="https://tonsite.com/User/Disable" method="post">
    <input type="hidden" name="id" value="GUID_DE_L_ADMIN" />
</form>

<script>
    document.forms[0].submit();
</script>
```

L'utilisateur ne voit rien…
Mais son navigateur envoie la requête **avec son cookie**,
donc ton site pense que c’est une vraie action de sa part !

Résultat :
→ Il pourrait désactiver un compte, changer un mot de passe, faire un paiement…

C’est ça, une attaque CSRF.

---

## 2. La solution : un token Anti-Forgery

Lorsque tu mets dans un formulaire :

```html
@Html.AntiForgeryToken()
```

ASP.NET génère **un jeton unique**, caché dans le formulaire.

En même temps, un **second jeton** est stocké dans un cookie sécurisé.

### Au moment du POST :

* ASP.NET récupère le **token dans le formulaire**
* ASP.NET récupère le **token dans le cookie**
* Les deux doivent **correspondre exactement**

Ce test est fait automatiquement grâce à :

```csharp
[ValidateAntiForgeryToken]
```

### Si le formulaire n’a pas été créé par ton site :

→ l’attaquant ne pourra **pas** générer le bon token
→ la validation échoue
→ la requête est rejetée

---

## 3. Résumé

### Sans anti-forgery :
un site malveillant peut forcer ton navigateur à exécuter une action POST sur ton site.

### Avec anti-forgery :

- ton serveur vérifie qu’un formulaire POST vient *vraiment* de ton site
- impossible pour un site externe de fabriquer un formulaire valide.

---

## 4. En une phrase

> **Le token anti-forgery garantit que la requête POST vient bien de TON formulaire, et pas d’un site extérieur qui essaierait de t’arnaquer.**

---

## 5. En une image mentale

* Le cookie = badge personnel de l’utilisateur
* Le token anti-forgery = ticket unique collé au formulaire

### Le serveur vérifie :

> *“Est-ce que le ticket correspond bien au badge ?”*

Si oui → OK
Si non → rejet immédiat

---
