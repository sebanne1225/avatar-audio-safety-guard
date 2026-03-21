# NDMF Build Pass Notes

## 目的

今回の `Avatar Audio Safety Guard` 実装で行った NDMF 関連の調査と判断を、次回以降に再利用しやすい形でまとめる。

このメモは以下を分けて扱う。

- 共通化向き: 別の NDMF ベース tool repo でも使いやすい前提
- repo 固有: `Avatar Audio Safety Guard` の仕様に依存する判断
- まだ不確実: version 変更や未検証条件で揺れうる点

## 今回の一次ソース

今回の判断は、主に URL の断片的な検索結果ではなく、ローカルに存在する package source を一次ソースとして使った。

- `C:\vrc-pro\ToolDevWorkspace\Packages\nadena.dev.ndmf\package.json`
- `C:\vrc-pro\ToolDevWorkspace\Packages\nadena.dev.ndmf\Editor\API\Fluent\Plugin.cs`
- `C:\vrc-pro\ToolDevWorkspace\Packages\nadena.dev.ndmf\Editor\API\Fluent\Sequence\Constraints.cs`
- `C:\vrc-pro\ToolDevWorkspace\Packages\nadena.dev.ndmf\Editor\API\BuildContext.cs`
- `C:\vrc-pro\ToolDevWorkspace\Packages\nadena.dev.modular-avatar\Editor\PluginDefinition\PluginDefinition.cs`
- `Editor/NDMF/AvatarAudioSafetyBuildPlugin.cs`
- `Editor/Core/AvatarAudioSafetyBuildProcessor.cs`

## 共通化向き

### 1. 今回確認した NDMF の仕様・使い方

- 今回ローカルで確認できた NDMF version は `1.11.0`。
- plugin は `[assembly: ExportsPlugin(typeof(...))]` と `Plugin<T>` 継承で登録する。
- pass の登録は `Configure()` 内で `InPhase(BuildPhase.XXX)` から始める。
- build 対象のルートは `BuildContext.AvatarRootObject` / `BuildContext.AvatarRootTransform` から取る。
- sequence の順序指定は `AfterPlugin(string qualifiedName)` / `BeforePlugin(...)` / `AfterPass(...)` / `WaitFor(...)` が使える。
- `AfterPlugin(string)` に渡す値は package 名ではなく plugin の `QualifiedName` が基準。
- Modular Avatar の plugin `QualifiedName` は今回のローカル source では `nadena.dev.modular-avatar` だった。

### 2. 今回の実装で採用した判断

- NDMF 依存コードは専用 asmdef に分離した。
  - 理由: tool 本体の Editor asmdef に直接 `nadena.dev.ndmf` を参照させると、依存 package がない環境で壊れやすいため。
- plugin 本体は薄くし、実処理は repo 側の core に寄せた。
  - `BuildPlugin -> BuildProcessor -> Planner / Applier / Logger` の流れにした。
- build pass では、その場で clone を列挙・判定・補正する形を採用した。
  - Dry Run の保存結果を使い回さず、`BuildContext.AvatarRootObject` から再列挙する。
- ordering は `AfterPlugin("nadena.dev.modular-avatar")` までに留めた。
  - coarse な順序だけ合わせて、特定 pass 名への強結合は避けた。
- 補正 plan と apply は分けた。
  - 将来 `Near` や `Volumetric Radius` を足すときに、planner と applier を伸ばしやすくするため。

### 3. 採用しなかった案とその理由

- 既存 Editor asmdef に NDMF 参照を直接追加する案は採用しなかった。
  - optional dependency にしづらく、package 単体の取り回しが悪くなるため。
- Build 時に Dry Run 保存結果をそのまま適用する案は採用しなかった。
  - MA などの処理後に最終 clone が変わりうるため、Dry Run と Build の対象がズレる。
- `plannedChange` の文字列を parse して apply する案は採用しなかった。
  - UI 向け文字列に実装が引っ張られて壊れやすいため。
- Modular Avatar の個別 pass に `AfterPass(...)` でぶら下げる案は採用しなかった。
  - MA 内部 pass 名への依存が強く、version 追従コストが上がるため。

### 4. 今後も共通化できそうな知見

- NDMF plugin を optional にしたい場合は「専用 asmdef に分離」がかなり扱いやすい。
- core ロジックは NDMF から切り離し、plugin 側は `BuildContext` 受け取りだけにすると保守しやすい。
- Build 時補正は、UI の保存状態ではなく build clone の再列挙を正にしたほうが Dry Run との差分理由を説明しやすい。
- ordering は最初から pass 単位に寄せすぎず、まず plugin 単位の弱い順序で始めるほうが安全。
- ローカルに入っている package source を一次ソースにすると、web の断片情報より判断がぶれにくい。

## repo 固有

### 1. 今回の repo で採用した実装判断

- 対象条件は `AvatarAudioSafetySettings` が root にあり、`Enabled == true` かつ `Mode == Apply On Build` のときだけ。
- 補正対象は初版では `Far / Gain / Volume` のみ。
- 補正優先順位は `Far -> Gain -> Volume`。
- `Ignore` は診断も補正もしない。
- `Report Only` は診断するが補正しない。
- `Custom Threshold` はその音源だけ個別上限を使う。
- `Custom Rolloff` は `Manual Review` 扱いにして、自動補正しない。
- build clone 上の `AvatarAudioSafetySettings` は、初版では安全側を優先して残したままにした。
- build log は `scanned / warnings / clamped / report_only / ignored / manual_review` の要約を出す。

### 2. repo 固有に留めるべき知見

- plugin の `QualifiedName` を `com.sebanne.avatar-audio-safety-guard.ndmf` にしたこと。
- build log の文言やカテゴリ名。
- `VRC Spatial Audio Source` の reflection member 名をどう扱うか。
  - 今回は `Gain / gain / _Gain` と `Far / far / _Far` の候補を使った。
- `Would Clamp` の結果から `Far / Gain / Volume` だけ plan を起こす方針。
- `scene / prefab 本体は触らず、build clone だけ変更` をこの tool の UX 前提として固定していること。

## まだ不確実

### 1. version 依存や未検証の点

- 今回確認した NDMF は `1.11.0`。`2.x` 系では API や sequencing の前提が変わる可能性がある。
- `AfterPlugin("nadena.dev.modular-avatar")` は今回のローカル MA source に合わせた判断。
  - MA 未導入 project での挙動は、今回は別途検証していない。
- `BuildContext` の clone 前提は source comment と NDMF の用途からは自然だが、将来の内部実装変更までは保証できない。
- `VRC Spatial Audio Source` の reflection 書き込み先は member 名に依存している。
  - VRChat SDK 側のフィールド名変更が入ると追従が必要。

### 2. 実装時に注意したほうがよい点

- compile 確認用の一時 project では、`com.vrchat.base` 同梱 test code の都合で `com.unity.test-framework` が必要だった。
  - これは tool 本体の設計というより、検証環境依存の注意点。
- NDMF / VRChat SDK / MA を同時に使う tool は、compile 環境を再現しないと判断を誤りやすい。

## 次回見る順番

次に別 repo で NDMF Build pass を入れるときは、まずこの順で確認するとよい。

1. 対象 repo の clamp policy と rule semantics を整理する
2. 対応する NDMF / MA の local package source で `QualifiedName` と API を確認する
3. plugin は薄く、core 処理は NDMF 非依存のまま切り出す
4. build clone を再列挙して evaluator を回し、planner / applier / logger を分ける
5. compile 確認は local package を参照する一時 project で行う
