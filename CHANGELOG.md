# Changelog

このファイルは Avatar Audio Safety Guard の変更履歴を管理します。

## [1.0.2] - 2026-03-21

### Changed

- VCC 導線 URL を live の `index.json` に統一
- README と `BOOTH_PACKAGE` の公開導線表記を調整
- release zip から内部メモを除外するよう workflow を調整
- QuickStart の UI 表記を実装上のボタン名に合わせて修正

## [1.0.0] - 2026-03-21

### Added

- `AvatarAudioSafetySettings` による設定保持
- Dry Run の AudioSource 走査と Inspector 一覧
- `診断レポート` ウィンドウと結果フィルタ
- path ベースの `音源ごとの設定`
- NDMF Build pass による build clone 補正

### Changed

- 補正対象を `Far / Near / Volumetric Radius / Gain / Volume` まで拡張
- Build summary と UI 文言を日本語中心の公開向け表現に整理
- README、TOOL_INFO、package metadata を GitHub / VPM 公開前提の内容へ更新

### Notes

- `Custom Rolloff` は自動補正せず `手動確認` 扱い
- `Near` / `Volumetric Radius` は個別上限値ではなく、現在の有効 `Far` を超える場合のみ build clone 側で補正
- `Samples~` はまだ最小構成で、実例入りサンプルは今後追加予定
