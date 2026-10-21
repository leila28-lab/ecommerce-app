#!/usr/bin/env bash
# Hook PreToolUse (Bash) — bloque les commandes irréversibles, côté code comme côté infra.
#
# C'est un contrôle DÉTERMINISTE : le harnais l'exécute à chaque commande, le modèle n'est
# pas consulté et ne peut donc pas le contourner. L'agent peut PROPOSER ces commandes ;
# c'est l'humain qui les lance, hors session, après avoir mesuré l'impact.
#
# Copie locale du hook du plugin oddo-dev-toolkit, avec une différence : sans `jq`, le hook du
# plugin laissait TOUT passer (exit 0). Ici, on analyse alors l'entrée JSON brute : le contrôle
# ne dépend plus d'un outil installé sur le poste.
set -uo pipefail

input="$(cat)"
[ -z "$input" ] && exit 0

if command -v jq >/dev/null 2>&1; then
  cmd="$(printf '%s' "$input" | jq -r '.tool_input.command // empty' 2>/dev/null)"
  [ -z "$cmd" ] && exit 0
else
  # Repli sans jq : les motifs ci-dessous sont cherchés dans tout le JSON de l'appel.
  cmd="$input"
fi

# Données : base, migrations, volumes.
DATA='DROP (TABLE|DATABASE|SCHEMA)|TRUNCATE |dotnet ef database drop|docker (volume rm|volume prune|system prune)|docker compose down .*(-v|--volumes)'
# Infrastructure : cluster, namespace, release.
INFRA='kubectl delete (ns|namespace|pvc|pv|statefulset)|k3d cluster delete|helm (uninstall|delete)'
# Historique Git : ce qui réécrit ou efface le travail des autres.
GIT='git push .*(-f|--force)|git reset --hard|git clean -[a-z]*f|git branch -D'
# Système de fichiers : effacement massif.
FS='rm -rf (/|~|\*|\.( |$)|\$HOME)'

if printf '%s' "$cmd" | grep -qiE "$DATA|$INFRA|$GIT|$FS"; then
  {
    echo "BLOQUÉ par le hook guard-destructive."
    echo "Commande refusée : $cmd"
    echo ""
    echo "Cette commande est irréversible. Procédure attendue :"
    echo "  1. Présenter ce qui sera supprimé et sur quel environnement."
    echo "  2. Proposer l'alternative réversible quand elle existe (rollback, nouvelle migration, revert)."
    echo "  3. L'humain lance lui-même la commande, hors session, s'il la valide."
  } >&2
  exit 2
fi
exit 0
