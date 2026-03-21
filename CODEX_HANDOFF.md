# CODEX_HANDOFF

## Goal
`Avatar Audio Safety Guard` の MVP を package repo として実装したいです。

このツールの目的:
- アバタールートに設定コンポーネントを付ける
- 最終的にアバターへ載る AudioSource を全走査する
- Dry Run では診断だけ行う
- 実際の補正は Build 時のみ行う
- AudioSource 一覧を見られるようにする
- Select / Ping で対象 GameObject を追えるようにする
- Build 時はログ要約を出す

今回の repo / package 名:
- repo: `avatar-audio-safety-guard`
- package: `com.sebanne.avatar-audio-safety-guard`
- displayName: `Avatar Audio Safety Guard`
- Runtime asmdef: `Sebanne.AvatarAudioSafetyGuard`
- Editor asmdef: `Sebanne.AvatarAudioSafetyGuard.Editor`

## Current State
仕様の方向性は固まっています。
まだ実装はこれからです。

前提:
- NDMF 準拠
- 非破壊
- 冪等
- 事前生成なし
- Build 時だけ適用
- Dry Run は診断のみ
- 判定対象は、最終的にアバターに載るすべての AudioSource
- MA などの後でまとめて診る前提

## Rules
- 非破壊
- 冪等
- Dry Run / 診断優先
- ログ重視
- 既存ファイルは必要以上に壊さない
- まず短い plan を出してから作業
- 作業後に変更ファイル一覧と tree を出す
- まだ commit / push はしない

## Requested Structure
必要なら package repo の受け皿をこの方向へ寄せてください。

- `Editor/Core/`
- `Editor/UI/`
- `Editor/Diagnostics/`
- `Editor/Utility/`
- `Runtime/` は必要最小限。不要なら薄くてよい

責務の考え方:
- Core: 診断ロジック、補正ロジック、モデル
- UI: Inspector / Report Window
- Diagnostics: 結果データ、一覧表示まわり
- Utility: Ping / path / 共通ヘルパー

## MVP Scope
初版でやること:
1. 設定コンポーネント
2. 共通診断ロジック
3. Dry Run 用の一覧表示
4. 詳細レポートウィンドウ
5. NDMF Build pass
6. Build 時ログ要約

初版でやらないこと:
- AudioClip アセット自体の加工
- 波形解析
- 事前 bake / 事前生成
- CSV 出力
- 正規表現 path 指定
- 複雑な危険度スコア
- Build 後ポップアップ

## Component Spec
アバタールートに付ける設定コンポーネントを作ってください。

仮名:
- `AvatarAudioSafetySettings`

想定項目:

### Basic
- Enabled
- Mode
  - Preview Only
  - Apply On Build
- Profile
  - Conservative
  - Standard
  - Custom

### Custom Thresholds
Custom のときだけ表示:
- Max Gain
- Max Far Distance
- Max Volume

### Diagnostics
- Warn On Missing VRC Spatial Audio Source
- Warn On Custom Rolloff
- Warn On Loop With Long Range
- Warn On Non-3D Audio

### Per-Source Rules
各要素:
- Path
- Rule
  - Default
  - Ignore
  - Report Only
  - Custom Threshold
- Max Gain
- Max Far Distance
- Max Volume
- Memo

### Tools
- Scan Audio Sources
- Open Report

### Detected Audio Sources
一覧表示:
- Result
- Path
- Clip
- Loop
- Volume
- Gain
- Far
- Rule
- Select
- Ping

## Behavior Spec

### Preview Only
- 診断だけ行う
- 値は変更しない
- 一覧とレポートに結果を出す

### Apply On Build
- Build 時のみ補正する
- 平常時は何も書き換えない
- 補正対象は build clone 側のみ

### Per-Source Rule
- Default:
  - 全体設定に従う
- Ignore:
  - 診断しない
  - 補正しない
- Report Only:
  - 診断する
  - 問題を表示する
  - 補正しない
- Custom Threshold:
  - その音源だけ個別上限を使う

## Detection Spec
対象:
- 最終アバターに存在する AudioSource 全部
- 同じ GameObject 上の VRC Spatial Audio Source があれば読む

主に見る値:
- AudioSource.volume
- AudioSource.spatialBlend
- AudioSource.rolloffMode
- AudioSource.minDistance
- AudioSource.maxDistance
- AudioSource.loop
- AudioSource.playOnAwake
- VRC Spatial Audio Source の Gain
- Near
- Far
- Volumetric Radius
- Use Spatializer Falloff の有無

判定結果の種類は、最低限これでよいです:
- Safe
- Warning
- Would Clamp
- Report Only
- Ignored
- Manual Review

## Clamp Policy
初版の補正優先順位:
1. 範囲を詰める
2. Gain を抑える
3. Volume を抑える

初版では自動補正しないもの:
- Custom Rolloff
- AudioClip 自体
- 波形

Custom Rolloff は Manual Review 扱いでよいです。

## Threshold Presets
まずは仮値でよいので、 Conservative / Standard / Custom を実装してください。
数値は後で調整しやすいように、定数の散在ではなく、まとまって見える形にしてください。

意図:
- Conservative = 安全寄り
- Standard = 少し緩め
- Custom = 手入力

## UI Requirements
Inspector は初心者でも見やすい形にしてください。
項目をただ並べるより、セクションが分かる見た目を優先してください。

最低限ほしいこと:
- Basic
- Thresholds
- Diagnostics
- Per-Source Rules
- Tools
- Detected Audio Sources

一覧では:
- 行クリックまたは Select ボタンで対象選択
- Ping ボタンで Hierarchy 上で見つけやすくする

## Report Window
Dry Run 結果の詳細レポートウィンドウを作ってください。

最低限ほしい表示:
- All
- Warnings
- Would Clamp
- Report Only
- Ignored
- Manual Review

各行で見たいもの:
- Path
- Clip
- 主要値
- 問題理由
- 予定補正
- Rule
- Select
- Ping

Build 中にポップアップは不要です。
Build 時はログ要約中心でお願いします。

## NDMF Build Integration
NDMF ベースで実装してください。

要件:
- Build 時のみ適用
- 最終アバターに対して処理
- MA などの後でまとめて診る想定
- 事前生成なし
- build clone に対してだけ変更
- 設定コンポーネントは build clone 側で後片付けする方向でよいです

Dry Run と Build で診断ロジックがズレないよう、
判定部分は共通化してください。

### NDMF Build Pass Notes (short)
- NDMF Build pass は local package source を一次ソースにして確認する。今回確認した前提では `AfterPlugin(...)` は package 名ではなく plugin `QualifiedName` 基準。
- NDMF 依存コードは専用 asmdef に分離し、plugin 本体は薄く保つ。core は NDMF 非依存で切り出す。
- Build 時補正は Dry Run 保存結果を流用せず、build clone を再列挙して `evaluator -> planner -> applier` で処理する。
- ordering はまず plugin 単位の弱い順序に留め、内部 pass 名への強結合は避ける。
- version 依存あり。今回確認ベースは NDMF `1.11.0` / MA plugin `QualifiedName` の `nadena.dev.modular-avatar` 前提。

## Logging
Build 時は要約ログを出してください。
最低限:
- scanned
- warnings
- clamped
- report_only
- ignored
- manual_review

例:
- scanned: 8
- warnings: 3
- clamped: 2
- report_only: 1
- ignored: 1
- manual_review: 1

できれば個別ログも簡潔に出したいです。
例:
- warned: `Path/...` - reason
- clamped: `Path/...` - Far 20 -> 6

## Implementation Notes
- 判定ロジックと適用ロジックは分ける
- Dry Run と Build で同じ判定関数を使う
- UI から見える文言は分かりやすくする
- 後で名前変更しやすいように、ツール名の文字列は散らさない
- しきい値は後で調整しやすい実装にする
- 例外設定は path ベースでまず実装する

## Deliverables
今回ほしいもの:
1. MVP 実装
2. 変更ファイル一覧
3. tree
4. Unity での確認手順
5. 未対応事項
6. 次にやるとよさそうなこと

## Definition of Done
- Unity でコンパイルエラーがない
- 設定コンポーネントがアバタールートに付けられる
- Scan Audio Sources で一覧が更新される
- Select / Ping が動く
- Open Report で詳細レポートが開く
- Preview Only で値を変更しない
- Apply On Build で build clone 側だけに補正が入る
- Build 時に要約ログが出る
- README か TOOL_INFO に MVP 範囲が軽く追記されている
