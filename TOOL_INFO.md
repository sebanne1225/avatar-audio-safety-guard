# TOOL_INFO

## ツール名

- Avatar Audio Safety Guard

## package名

- `com.sebanne.avatar-audio-safety-guard`

## 表示名

- `Avatar Audio Safety Guard`

## asmdef

- Runtime: `Sebanne.AvatarAudioSafetyGuard`
- Editor: `Sebanne.AvatarAudioSafetyGuard.Editor`

## 想定用途

- VRChat アバター内の AudioSource を診断し、Build 時に安全側へ補正できる Unity Editor ツールの基盤を提供する。

## 現在対応していること

- `AvatarAudioSafetySettings` で設定を保持できる
- Dry Run で AudioSource 一覧を更新し、ObjectField から対象を追える
- Report Window に `すべて / 問題なし以外 / Build時に補正予定 / 報告のみ / 対象外 / 手動確認` のフィルタがある
- Report Window は `AvatarAudioSafetySettings` に保持された最新 Dry Run 結果を参照する
- 簡易 Report Window の土台があり、Dry Run 一覧まで実装済み
- NDMF Build pass で `Build時に補正` のときだけ build clone 側へ `Far / Gain / Volume` を補正する
- Build 時に要約ログを出し、`診断のみ` では build clone を変更しない

## 非対応

- `Custom Rolloff` の自動補正
- 波形解析
- `AudioClip` 加工
- `Near` / `Volumetric Radius` の補正

## 今後やりたいこと

- AudioSource 診断ルールの追加
- Dry Run 結果を見やすく出すログ基盤の追加
- Build 時補正フローと安全確認 UI の追加
- サンプルと公開ドキュメントの拡充
