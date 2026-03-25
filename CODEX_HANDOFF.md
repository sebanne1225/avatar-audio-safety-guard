# CODEX_HANDOFF

## Goal
`avatar-audio-safety-guard` は、VRChat avatar 上の `AudioSource` を非破壊に診断し、必要な場合だけ Build 時に build clone 側へ補正を入れる package repo として運用中です。

今の handoff では、初期仕様書ではなく、repo 固有の現況と次回再開用メモだけを残します。

## Current State
- package repo の土台は存在し、Runtime / Editor / NDMF 依存分離まで入っています
- `AvatarAudioSafetySettings` をアバタールートに付け、Inspector から Dry Run / 診断レポート / 個別ルール編集ができます
- AudioSource の再検出は明示的な `AudioSource を走査` で行います
- 既存の検出結果に対する分類更新は、設定変更時の再判定で更新します
- Inspector と Report Window は同じ保存済み検出結果を見ます
- 診断レポートから軽い個別ルール変更ができ、重い編集は Settings 側へ残す方針です
- Build 連携は NDMF ベースで、build clone を再列挙して処理します
- VRChat SDK validation 対策は入っていて、settings component は upload 対象に残さない前提です
- Build 結果の主表示先は NDMF Console 寄りです
- Inspector / Report Window の Build 結果 UI は撤去済みです
- build result snapshot / session cache は内部にはまだ残っており、即削除はしていません

## Current Direction
- ツール UI は Dry Run / 診断結果確認 / 軽い調整に集中させる
- Build 時の注目点は NDMF Console に出し、full な履歴 UI は無理に戻さない
- 「再走査」と「再判定」は分離したまま維持する
- 一覧 UI では軽い操作を優先し、複雑な編集や詳細確認は Settings 側へ逃がす
- 理由や次アクションが弱い分類は、独立フィルタとして無理に前面化しない

## Current Blocker
- 明確な blocker は今はありません
- 次に触る時は、診断 UI の軽量化を続けるのか、内部に残っている build result / session cache 周辺を整理するのかを最初に切り分けると安全です
- 2026-03-25 時点で、repo 内の公開面 version drift は `1.0.2` に揃え直しています。次は repo 内最終確認のあと、Release -> listing -> VCC確認 -> BOOTH の順で進める想定です

## Rules
- 非破壊
- 冪等
- Dry Run / 診断優先
- ログ重視
- Runtime と Editor / NDMF 依存を混ぜすぎない
- build clone 側の処理と source/original 側の設定保持を混同しない
- UI 改修では、まず既存の保存済み結果と再判定フローを活かす
- まず短い plan を出してから作業
- まだ commit / push はしない

## Tasks
- Dry Run / 診断 UI の使いやすさ改善は、まず一覧操作と文言整理を優先する
- Build 関連を触る場合は、NDMF Console を主表示先に寄せる方針を崩さない
- 内部の build result snapshot / session cache を整理する場合は、現状どこで参照されているかを先に確認する
- source settings 解決、build clone cleanup、NDMF timing を触る場合は blocker 修正として別で扱う
- 行単位 UI を増やす時は、再走査を自動化せず、既存の再判定フローへつなぐ

## Definition of Done
- Unity compile を壊していない
- 既存の Dry Run / 診断導線を壊していない
- 明示走査と設定変更時再判定の分離を壊していない
- Inspector と Report Window の見え方が大きくズレていない
- build clone 側の補正と source/original 側の設定保持の境界を壊していない
- ログまたは UI から、利用者が次の行動を判断できる
- commit / push はしていない

## Resume Notes
- 設定保持と保存済み検出結果:
  - `Runtime/AvatarAudioSafetySettings.cs`
  - `Runtime/AvatarAudioSafetyModels.cs`
- 明示走査 / 再判定の境界:
  - `Editor/Core/AvatarAudioSafetyScanner.cs`
  - `Editor/Utility/AvatarAudioSafetyScanActions.cs`
- Inspector / 診断レポート UI:
  - `Editor/UI/AvatarAudioSafetySettingsEditor.cs`
  - `Editor/UI/AvatarAudioSafetyReportWindow.cs`
  - `Editor/UI/AvatarAudioSafetyResultTable.cs`
  - `Editor/UI/AvatarAudioSafetyUiText.cs`
- 個別ルールの editor 操作:
  - `Editor/Utility/AvatarAudioSafetyObjectActions.cs`
- Build / NDMF / Console:
  - `Editor/NDMF/AvatarAudioSafetyBuildPlugin.cs`
  - `Editor/NDMF/AvatarAudioSafetyNdmfConsoleReporter.cs`
  - `Editor/Core/AvatarAudioSafetyBuildProcessor.cs`
- 内部 build result / session cache を後で削るなら先に確認:
  - `Editor/Utility/AvatarAudioSafetySessionState.cs`
