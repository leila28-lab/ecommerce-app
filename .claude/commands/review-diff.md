---
description: Relit le diff courant selon la checklist de revue de l'équipe, sans rien modifier
argument-hint: [branche de base, main par défaut]
allowed-tools: Bash(git diff:*)
---

Relis les changements de la branche courante par rapport à la branche de base indiquée
ici : « $ARGUMENTS » (`main` si rien n'est indiqué).

Fichiers modifiés par rapport à `main` (injecté avant la lecture du prompt) :
!`git diff --stat main...HEAD || git diff --stat`

Pour chaque fichier modifié, vérifie dans cet ordre :

1. **Correction** — le code fait-il ce que le nom de la branche et les commits annoncent ?
   Cas d'erreur traités ? Valeurs nulles ?
2. **Contrats** — une route, un DTO ou un champ JSON a-t-il changé ? Qui le consomme
   (Gateway, Ordering, Web) ? Les consommateurs sont-ils mis à jour ?
3. **Tests** — chaque comportement modifié a-t-il un test ? Un test existant a-t-il été
   affaibli pour passer ?
4. **Conventions** — adresses de services en dur au lieu du service discovery Aspire ?
   Secret ou donnée personnelle dans le code ou les logs ?
5. **Base** — migration présente ? Destructive ? Relue ?

Rends un tableau `Fichier | Constat | Gravité (Bloquant / À corriger / Suggestion) | Proposition`,
puis un verdict en une ligne.

**Lecture seule : ne modifie aucun fichier, ne lance aucune commande d'écriture.**
