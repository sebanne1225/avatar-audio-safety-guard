# Avatar Audio Safety Guard

`Avatar Audio Safety Guard` は、VRChat アバター内の AudioSource を Dry Run で診断し、必要に応じて Build 時だけ build clone 側へ安全側の補正を適用できる Unity Editor ツールです。

音量や到達距離の設定ミスを Build 前に見つけたいときや、scene / prefab 本体を触らずに Build 時だけ補正したいときに使えます。

## 何を解決するツールか

- アバター内の AudioSource 設定を Build 前に一覧で確認できます
- 問題がありそうな音源だけを Dry Run で洗い出せます
- 実データは触らず、必要な補正だけを Build 時に build clone 側へ適用できます

## 何ができるか

- アバタールートに `AvatarAudioSafetySettings` を追加して設定を保持できます
- `AudioSource を走査` で Dry Run 診断を実行できます
- `検出された AudioSource` 一覧と `診断レポートを開く` から結果を確認できます
- 結果は `問題なし / 警告 / Build時に補正予定 / 報告のみ / 対象外 / 手動確認` で分類されます
- `Build時に補正` のときだけ build clone 側へ `Far / Near / Volumetric Radius / Gain / Volume` を補正します
- Build 時に `scanned / warnings / corrected_sources / report_only / ignored / manual_review` の要約ログを出します

## 対応環境

- Unity `2022.3`
- VRChat Avatars package `>= 3.10.0`
- NDMF `>= 1.11.0 < 2.0.0-a`
- VCC / VPM ベースの VRChat プロジェクトを推奨します

## VCC / VPM 導入方法

### 推奨: VCC / VPM から導入

1. VCC に追加する URL として `https://sebanne1225.github.io/sebanne-listing/index.json` を追加します。
2. package 一覧から `Avatar Audio Safety Guard` (`com.sebanne.avatar-audio-safety-guard`) を追加します。
3. Unity を開き、依存 package が解決されていることを確認します。

参考ページ (`VCC` 追加先ではありません): `https://sebanne1225.github.io/sebanne-listing/`

### 補助: GitHub / Git URL から導入

- repo: `https://github.com/sebanne1225/avatar-audio-safety-guard`
- Git URL や local package での導入は、開発確認や手動検証向けの補助導線です
- この方法では `VRChat Avatars` と `NDMF` の依存解決を自分で確認する必要があります
- GitHub Release には `com.sebanne.avatar-audio-safety-guard-1.0.1.zip` を添付します。zip 展開後の直下に `package.json` が見える package 構成です。

## 基本的な使い方

1. アバタールートに `AvatarAudioSafetySettings` を追加します。
2. `動作モード` はまず `診断のみ` のままにします。
3. `判定プロファイル`、`診断オプション`、`音源ごとの設定` を必要に応じて調整します。
4. `AudioSource を走査` を押して Dry Run を実行します。
5. `検出された AudioSource` 一覧と `診断レポートを開く` で結果を確認します。
6. 問題がないことを確認してから、必要に応じて `Build時に補正` へ切り替えます。

## Dry Run を先に行う流れ

- `AudioSource を走査` は常に非破壊です。scene / prefab 本体は変更しません。
- `有効化` がオフでも Dry Run 自体は実行できます。Build 時の補正だけを止めたいときに使えます。
- 最新の Dry Run 結果は `AvatarAudioSafetySettings` に保持され、`診断レポート` から再表示できます。
- `診断のみ` では Build 時も値を変更しません。`Build時に補正` のときだけ build clone 側へ補正します。
- `corrected_sources` は、補正された項目数ではなく「補正が入った AudioSource 件数」です。

## 制限事項

- `Custom Rolloff` の自動補正は行いません。`手動確認` として扱います。
- `VRC Spatial Audio Source` が未設定の AudioSource は警告できますが、自動追加はしません。
- `Near` と `Volumetric Radius` は、現在の有効 `Far` を超えている場合だけ build clone 側で補正します。個別の上限値はまだ設定できません。
- 波形解析や `AudioClip` 自体の加工は未対応です。
- `Samples~` はまだ実例入りではなく、最小の骨組みのみです。

## ライセンス

MIT License で提供します。詳細は `LICENSE` を参照してください。
