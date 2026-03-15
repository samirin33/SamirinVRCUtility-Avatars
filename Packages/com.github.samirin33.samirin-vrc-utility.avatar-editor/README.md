# SamirinVRCUtility Avatar Editor

Samirin33 の VRChat Avatar 3.0 向けエディタ拡張パッケージです。  
Animator の編集効率化やアニメーションクリップの一括操作、VRChat パラメータ管理などの機能を提供します。

## 必要環境

- Unity 2022.3 以降（VRChat SDK 対応バージョン）
- [VRChat Avatars](https://vcc.docs.vrchat.com/vpm/packages#vrchat-official-packages) 3.7.0 以上
- [SamirinVRCUtility Avatar](https://github.com/samirin33/samirin-vrc-utility.avatar) 1.0.0 以上（ランタイム用）

## インストール

こちらから
https://samirin33.github.io/Samirin33VPM/

### VPM（推奨）

1. [VRChat Creator Companion](https://vcc.docs.vrchat.com/vpm/installation) でプロジェクトを開く
2. パッケージ一覧に以下を追加  
   `https://github.com/samirin33/samirin-vrc-utility.avatar-editor.git`

### 手動（UPM）

1. `Window` → `Package Manager` → `+` → `Add package from git URL`
2. 以下を入力  
   `https://github.com/samirin33/samirin-vrc-utility.avatar-editor.git`

## 使い方

メニュー **`samirin33 Editor Tools`** から各ツールを開けます。

| 機能 | メニュー | 説明 |
|------|----------|------|
| [Animator Default Setting](docs/features/01-animator-default-setting.md) | （Animator ウィンドウ内で自動適用） | 新規ステート・遷移・レイヤーの初期値 |
| [Behaviour Copy Paste](docs/features/02-animator-behaviour-copy.md) | Animator 内の Behaviour で右クリック | StateMachineBehaviour / ステートのコピー・ペースト |
| [Animation Clip Path Replace](docs/features/03-animation-clip-path-replace.md) | samirin33 Editor Tools → Animation Clip Path Replace | クリップ内のバインドパス一括リネーム |
| [Animation Clip Replace](docs/features/04-animation-clip-replace.md) | samirin33 Editor Tools → Animation Clip Replace | Controller 内のクリップ参照一括置換 |
| [VRChat Avatar Param Setter](docs/features/05-vrc-avatar-param-setter.md) | samirin33 Editor Tools → VRChat Avatar Param Setter | 不足 VRChat パラメータの一括追加・早見 |
| [Animation Clip Selector](docs/features/06-animation-clip-selector.md) | samirin33 Editor Tools → Animation Clip Selector | 編集中 Animator のクリップ一覧・選択・競合表示 |
| [FPS Limiter](docs/features/07-fps-limiter-editor.md) | GameObject → SamirinEditorTools → FPS Limiter | エディタプレビュー最高FPS制限 |

詳細は **[機能別ドキュメント](docs/README.md)** を参照してください。

## ライセンス

MIT License
