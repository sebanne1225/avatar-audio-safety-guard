# CLAUDE

## Goal

- `avatar-audio-safety-guard` は、VRChat avatar 上の `AudioSource` を非破壊に診断し、必要な場合だけ Build 時に build clone 側へ補正を入れる package repo として運用中です。
- handoff には初期仕様ではなく、repo 固有の現況と次回再開用メモだけを残します。

## Current State

- repo root が package source of truth です。`Runtime/`、`Editor/`、`README.md`、`CHANGELOG.md`、release workflow まで揃っています。
- `package.json` は `1.0.2`、ローカル HEAD は `a2e9bdd` (`master`) です。tag は `1.0.2` まであります。
- `Runtime/AvatarAudioSafetySettings.cs` が設定保持の中心で、mode / profile / per-source rule / scan result / source settings id / last build result snapshot を持ちます。
- Inspector (`Editor/UI/AvatarAudioSafetySettingsEditor.cs`) から Dry Run、保存済み結果の閲覧、個別ルール編集、threshold 編集、診断オプション編集ができます。
- AudioSource の再検出は明示的な `AudioSource を走査` で行います。既存の保存済み結果に対する分類更新は、設定変更時の再判定で更新する分離を維持しています。
- Report Window は保存済み scan result の閲覧と filter に集中しています。Build 結果モデル / table / session cache は repo 内に残っていますが、現状の UI では前面に出していません。
- Build 連携は NDMF ベースです。`AvatarAudioSafetySourceSettingsPreBuildHook` で source settings id を埋め、`AvatarAudioSafetyBuildPlugin` / `AvatarAudioSafetyBuildProcessor` で build clone を再列挙して補正します。
- build result snapshot は source settings と session state に保持され、主表示先は NDMF Console / Console log 寄りです。
- `AvatarAudioSafetyGuardCheckWindow.cs` は template 由来の最小確認ウィンドウとして残っていますが、main 導線ではありません。

## Current Direction

- ツール UI は Dry Run / 診断結果確認 / 軽い調整に集中させる方針でよいです。
- Build 時の注目点は NDMF Console / Console log に寄せ、full な履歴 UI を無理に戻さない方針を維持してよいです。
- 「再走査」と「再判定」は分離したまま維持するのが安全です。
- 次に触る時は、内部に残っている build result / session cache 周辺を整理する回にするのか、UI をさらに軽量化する回にするのかを最初に決めるとぶれにくいです。

## Current Blocker

- 明確な blocker は今はありません。
- 未整理ポイントは、使っていない build result UI 向けの部品と、legacy の Check Window をどこまで残すかです。
- repo 外の listing / BOOTH 反映状況は、この repo 単体からは確定できないので handoff では前提にしない方が安全です。

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

## Key Files

- `Runtime/AvatarAudioSafetySettings.cs`
- `Runtime/AvatarAudioSafetyModels.cs`
- `Editor/Core/AvatarAudioSafetyScanner.cs`
- `Editor/Core/AvatarAudioSafetyEvaluator.cs`
- `Editor/Core/AvatarAudioSafetyBuildProcessor.cs`
- `Editor/Core/AvatarAudioSafetyBuildPlanner.cs`
- `Editor/UI/AvatarAudioSafetySettingsEditor.cs`
- `Editor/UI/AvatarAudioSafetyReportWindow.cs`
- `Editor/UI/AvatarAudioSafetyResultTable.cs`
- `Editor/NDMF/AvatarAudioSafetyBuildPlugin.cs`
- `Editor/NDMF/AvatarAudioSafetySourceSettingsPreBuildHook.cs`
- `Editor/NDMF/AvatarAudioSafetyNdmfConsoleReporter.cs`
- `Editor/Utility/AvatarAudioSafetySessionState.cs`
- `Editor/AvatarAudioSafetyGuardCheckWindow.cs`

## Resume Notes

- package: `com.sebanne.avatar-audio-safety-guard`
- version: `1.0.2`
- latest tag: `1.0.2`
- HEAD: `a2e9bdd` (`master`)
- release asset 名: `com.sebanne.avatar-audio-safety-guard-1.0.2.zip`
- Build 結果まわりを整理するなら、先に `Runtime/AvatarAudioSafetyModels.cs` と `Editor/Utility/AvatarAudioSafetySessionState.cs` の参照元確認から始めると安全です。
