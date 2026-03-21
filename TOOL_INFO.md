# TOOL_INFO

このファイルは、`Avatar Audio Safety Guard` の repo 補助文書です。README の代わりではなく、公開準備や listing 反映時に確認したい情報を短くまとめています。

## 基本情報

- ツール名: `Avatar Audio Safety Guard`
- package名: `com.sebanne.avatar-audio-safety-guard`
- 表示名: `Avatar Audio Safety Guard`
- Runtime asmdef: `Sebanne.AvatarAudioSafetyGuard`
- Editor asmdef: `Sebanne.AvatarAudioSafetyGuard.Editor`
- 現在 version: `1.0.1`

## 公開メタ情報

- GitHub repo: `https://github.com/sebanne1225/avatar-audio-safety-guard`
- changelogUrl: `https://github.com/sebanne1225/avatar-audio-safety-guard/blob/master/CHANGELOG.md`
- listing repo: `https://github.com/sebanne1225/sebanne-listing`
- listing page: `https://sebanne1225.github.io/sebanne-listing/`
- VCC に追加する URL: `https://sebanne1225.github.io/sebanne-listing/index.json`
- listing 側に追加する `githubRepos`: `sebanne1225/avatar-audio-safety-guard`
- BOOTH 販売名: `Avatar Audio Safety Guard`

## 公開スコープの要約

- `AvatarAudioSafetySettings` で設定を保持できる
- Dry Run で AudioSource を走査し、Inspector 一覧と `診断レポート` で確認できる
- `Build時に補正` のときだけ build clone 側へ `Far / Near / Volumetric Radius / Gain / Volume` を補正する
- Build 時に要約ログを出し、`診断のみ` では build clone を変更しない

## 導入導線の前提

- 主導線は VCC / VPM
- Git URL / local package 導入は補助扱い
- Git URL 導入時は依存 package の解決を別途確認する

## 既知の制限

- `Custom Rolloff` の自動補正はしない
- 波形解析と `AudioClip` 加工は未対応
- `Near` / `Volumetric Radius` の個別上限値設定は未対応
- `Samples~` と公開画像はまだ拡充余地がある
