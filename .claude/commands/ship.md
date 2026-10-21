---
description: Prépare une livraison — relecture du diff, tests, commit conventionnel et Pull Request, avec validation à chaque étape
argument-hint: [contexte de la livraison]
---

Prépare la livraison de la branche courante. Contexte fourni par l'utilisateur : $ARGUMENTS

1. Affiche `git status` et `git diff --stat`, puis résume les changements en trois lignes.
2. Relis le diff sous l'angle sécurité et conformité : secret, chaîne de connexion avec mot
   de passe, donnée personnelle dans un log, endpoint de diagnostic exposé. Signale tout
   écart avant d'aller plus loin.
3. Si le diff touche `Migrations/`, `.github/`, `k8s/` ou `helm/`, liste explicitement ce qui
   change pour l'exploitation (schéma, droits, déploiement).
4. Lance `dotnet build ECommerce.slnx` puis `dotnet test ECommerce.slnx`. Si un test échoue,
   **arrête-toi** et explique pourquoi.
5. Propose un message au format Conventional Commits — en anglais, à l'impératif, avec un
   corps qui explique le *pourquoi*.
6. **Attends ma validation** avant de committer.
7. Après validation : commit, puis **redemande** avant le push, puis ouvre la Pull Request
   (titre, contexte, changements, tests, risques, checklist de revue).

## Interdits

- Ne merge **jamais**. Ne force **jamais** un push. N'utilise jamais `--no-verify`.
- Ne committe pas si l'étape 2 a relevé un secret : signale-le et arrête-toi.
