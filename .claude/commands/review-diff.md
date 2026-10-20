---
description: Relit le diff courant selon la checklist de revue de l'équipe, sans rien modifier
---

Relis les changements de la branche courante par rapport à main. Pour chaque fichier modifié :

1. Correction — le code fait-il ce que les commits annoncent ? Cas d'erreur traités ?
2. Contrats — une route, un DTO ou un champ JSON a-t-il changé ? Qui le consomme ?
3. Tests — chaque comportement modifié a-t-il un test ?
4. Conventions — URL en dur ? secret ? donnée client dans un log ?
5. Base — une migration est-elle présente ? destructive ?

Rends un tableau Fichier | Constat | Gravité | Proposition, puis un verdict en une ligne.
Lecture seule : ne modifie aucun fichier.
