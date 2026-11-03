#!/usr/bin/env bash
# Pushes small CI outputs (PNG screenshots, log tails) to the orphan branch ci-shots so they can be
# read with plain git from a phone-only workflow where the Actions log/artifact download is blocked.
# Usage: publish-ci.sh <label> <dir-or-file>...   Needs GITHUB_TOKEN with contents: write.
set -u
label=$1; shift
run=${GITHUB_RUN_NUMBER:-0}; sha=${GITHUB_SHA:-unknown}
out=$(mktemp -d); dest="$out/$run-$label"; mkdir -p "$dest"
for src in "$@"; do
  [ -e "$src" ] || continue
  find "$src" -maxdepth 4 -type f \( -name '*.png' -o -name '*.log' -o -name '*.txt' -o -name '*.xml' \) -size -6M 2>/dev/null | head -40 | while read -r f; do
    case "$f" in *.png|*.xml) cp "$f" "$dest/$(basename "$f")";; *) tail -n 400 "$f" > "$dest/$(basename "$f").tail.txt";; esac
  done
done
ls -la "$dest"
cd "$out"
git init -q && git checkout -q -b ci-shots
git config user.email ci@sheytoonak && git config user.name "sheytoonak-ci"
remote="https://x-access-token:${GITHUB_TOKEN}@github.com/${GITHUB_REPOSITORY}.git"
# Two jobs (tests, screenshots) publish at the same moment; a plain push lost the screenshots of runs 58 and 59
# ("cannot lock ref"). So: rebuild on top of the branch as it is now and retry a few times.
mv "$dest" "$out.new"
for try in 1 2 3 4 5; do
  git checkout -q --orphan "tmp$try" 2>/dev/null; git rm -rq --cached . 2>/dev/null; find . -mindepth 1 -maxdepth 1 ! -name .git -exec rm -rf {} +
  # keep earlier runs: start from the branch's current tree, then add this run's folder
  if git fetch -q "$remote" ci-shots 2>/dev/null; then git reset -q --soft FETCH_HEAD && git checkout -q FETCH_HEAD -- . 2>/dev/null || true; fi
  rm -rf "$run-$label"; cp -r "$out.new" "$run-$label"
  # keep at most the 12 newest run folders so the branch stays small
  ls -d [0-9]*-* 2>/dev/null | sort -t- -k1,1n | head -n -12 | xargs -r rm -rf
  git add -A && git commit -qm "run $run $label ($sha)" && git push -q "$remote" HEAD:ci-shots && exit 0
  sleep $((try * 7))
done
echo "::warning::could not publish to ci-shots"
