#!/usr/bin/env bash
# Hook PostToolUse (Bash) — après un `docker build -t <image>`, scanne l'image avec Trivy.
#
# Informe, ne casse rien : sans vulnérabilité CRITICAL, exit 0 et rien n'est dit. Avec une
# vulnérabilité CRITICAL, exit 2 : le résumé est renvoyé à l'agent, qui doit qualifier et
# proposer une remédiation. Le BLOCAGE, lui, est dans la CI (images.yml), là où une procédure
# d'équipe existe pour le lever.
#
# Adapté du corrigé du lab 07 : sans `jq`, on analyse l'entrée JSON brute au lieu de sortir
# en silence (le scan ne dépend plus d'un outil installé sur le poste).
set -uo pipefail

input="$(cat)"
[ -z "$input" ] && exit 0

if command -v jq >/dev/null 2>&1; then
  cmd="$(printf '%s' "$input" | jq -r '.tool_input.command // empty' 2>/dev/null)" || exit 0
else
  cmd="$input"
fi
printf '%s' "$cmd" | grep -q 'docker build' || exit 0

image="$(printf '%s' "$cmd" | sed -nE 's/.*(-t|--tag)[ =]+([^ "\\]+).*/\2/p' | head -1)"
[ -n "$image" ] || exit 0
case "$image" in -*) exit 0 ;; esac                          # pas une référence d'image
command -v trivy >/dev/null 2>&1 || exit 0
docker image inspect "$image" >/dev/null 2>&1 || exit 0     # build échoué : rien à scanner

report="$(trivy image --quiet --severity CRITICAL --ignore-unfixed --format table "$image" 2>/dev/null)" || exit 0
if printf '%s' "$report" | grep -q 'CRITICAL'; then
  {
    echo "Scan Trivy de $image : vulnérabilité(s) CRITICAL corrigeable(s) détectée(s)."
    printf '%s\n' "$report" | grep -E 'CRITICAL|Library|Total' | head -15
    echo "Qualifie chaque résultat (exploitable ici ?) et propose la remédiation minimale."
  } >&2
  exit 2
fi
exit 0
