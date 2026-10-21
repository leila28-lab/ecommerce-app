---
description: Initialise le dépôt Git du projet et le publie sur GitHub en privé, avec confirmation avant toute action distante
argument-hint: [nom-du-dépôt]
---

Initialise le dépôt Git de ce projet et publie-le sur GitHub. Nom souhaité : « $ARGUMENTS »
(par défaut : le nom du dossier courant).

1. Si le dossier est déjà un dépôt avec un remote (`git remote -v` non vide) : **arrête-toi** et dis-le.
2. Vérifie `gh auth status`. Si l'authentification manque : arrête-toi et propose `gh auth login`.
3. `git init`, `git branch -M main`.
4. Avant d'indexer, liste ce qui ne doit **jamais** l'être et vérifie le `.gitignore` :
   `.env`, `*.pem`, `*.key`, `appsettings.*.local.json`, `bin/`, `obj/`, `.vs/`, `TestResults/`.
5. `git add .` puis montre `git status --short` et **attends ma validation**.
6. Commit initial : `chore: initial commit`.
7. **Demande confirmation** avant de créer le dépôt distant, puis :
   `gh repo create <nom> --private --source=. --remote=origin --push`.
8. Affiche l'URL du dépôt.

## Interdits

- Jamais de dépôt **public** sans demande explicite.
- Jamais de `--force`.
