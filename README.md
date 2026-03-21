# Avatar Audio Safety Guard

この repository は、VRChat アバター内の AudioSource を診断し、Build 前後で安全側の運用につなげるための Unity Editor ツール package repo です。現時点ではテンプレ repo からの移行直後で、実ツール名への置換と公開向けの土台整備を優先しています。

## 概要

`Avatar Audio Safety Guard` は、AudioSource の存在や設定を確認しながら、非破壊かつ Dry Run 優先で安全確認を進めるための Unity Editor ツールです。Runtime と Editor を分離し、package として導入しやすい最小構成を維持しています。

## 何ができるか

- Unity Package として `Avatar Audio Safety Guard` をプロジェクトへ導入できます
- `Runtime/` と `Editor/` を分離した状態で、診断処理と Editor 拡張を安全に育てられます
- `Documentation~/` と `Samples~/` を含む標準的な package 骨組みをそのまま利用できます
- Dry Run と診断ログを重視した実装方針を repo 全体で共有できます

## 現在対応していること

- `AvatarAudioSafetySettings` をアバタールートに付与して設定を保持できます
- Dry Run で AudioSource を走査し、一覧と簡易 Report Window で確認できます
- Report Window では `すべて / 問題なし以外 / Build時に補正予定 / 報告のみ / 対象外 / 手動確認` のフィルタを使えます
- Report Window は `AvatarAudioSafetySettings` に保持された最新の Dry Run 結果を参照して再表示できます
- ObjectField 中心の一覧 UI で対象 GameObject を追える最小 UI を実装しています
- NDMF Build pass で、動作モードが `Build時に補正` のときだけ build clone 側へ `Far / Gain / Volume` の補正を適用します
- Build 時に `scanned / warnings / corrected_sources / report_only / ignored / manual_review` の要約ログを出します

## 使い方

1. Unity の Package Manager から、この repo をローカルパッケージまたは Git URL として追加します。
2. アバタールートに `AvatarAudioSafetySettings` を追加します。
3. Inspector で Profile、Diagnostics、Per-Source Rules を必要に応じて設定します。
4. `Scan Audio Sources` を押して Dry Run を実行し、Detected Audio Sources 一覧または `Open Report` で結果を確認します。

## Dry Run / 診断

- 現在の scan は非破壊で、AudioSource の走査と一覧更新だけを行います。
- `Enabled` がオフでも Dry Run scan 自体は実行できます。Build 時は build clone への補正をスキップします。
- 最新の Dry Run 結果は `AvatarAudioSafetySettings` に保持され、Report Window はその保存済み結果を読みます。
- 動作モードが `診断のみ` のときは Build 時も変更しません。`Build時に補正` のときだけ、build clone 側へ補正します。
- Build summary の `corrected_sources` は、補正された項目数ではなく「補正が入った AudioSource 件数」です。
- 実際の診断や補正を追加する場合も、対象、変更予定、スキップ理由、失敗理由を追跡できるログ設計を優先します。
- 既存データを直接壊さない方針を維持し、必要に応じてプレビュー、複製、退避を用意します。

## 制限事項

- `Custom Rolloff` の自動補正はまだ行いません。`Manual Review` として扱います。
- 波形解析や `AudioClip` 自体の加工はまだ未対応です。
- `Near` や `Volumetric Radius` などの細かい補正はこれからです。
- 公開向けドキュメントと Samples は骨組みのみで、具体的な使用例はこれから追加します。

## ライセンス

MIT License で提供します。詳細は `LICENSE` を参照してください。
