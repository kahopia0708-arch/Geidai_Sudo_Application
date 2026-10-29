#!/usr/bin/env bash
# Drive ミラー → docs/project へ同期（ローカルに Google Drive がある場合）
set -euo pipefail

DRIVE_ROOT="${GEIDAI_DRIVE_DOCS:-$HOME/Library/CloudStorage/GoogleDrive-peugeot.106.s16@gmail.com/マイドライブ/docs/202607.藝大_須藤さん}"
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEST="$REPO_ROOT/docs/project"

if [[ ! -d "$DRIVE_ROOT" ]]; then
  echo "Drive ルートが見つかりません: $DRIVE_ROOT" >&2
  echo "環境変数 GEIDAI_DRIVE_DOCS でパスを指定できます。" >&2
  exit 1
fi

mkdir -p "$DEST/input"

echo "Sync markdown from Drive..."
cp "$DRIVE_ROOT/プロジェクト概要.md" "$DEST/"
cp "$DRIVE_ROOT/修正点まとめ_20261029.md" "$DEST/" 2>/dev/null || true

echo "Sync input (standard set)..."
STANDARD=(
  "打ち合わせ記録_20260818.pdf"
  "修正点まとめ_20261029.pdf"
  "ゲームの仕様について_20260710.pdf"
  "ゲーム案(優先度付き)_20260710版.pdf"
  "作品コンセプト.pdf"
  "ArtDX公募_募集要項2026.pdf"
  "Sさん_学内公募研究.png"
)
for f in "${STANDARD[@]}"; do
  if [[ -f "$DRIVE_ROOT/input/$f" ]]; then
    cp "$DRIVE_ROOT/input/$f" "$DEST/input/"
  else
    echo "  skip (missing): $f"
  fi
done

if [[ "${1:-}" == "--full" ]]; then
  echo "Sync input (--full, includes large 0427 PDFs)..."
  cp "$DRIVE_ROOT/input/"* "$DEST/input/" 2>/dev/null || true
fi

echo "Done → $DEST"
du -sh "$DEST"
