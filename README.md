# SamirinVRCUtility Avatars

Samirin33 の VRChat Avatar 3.0 向け **NDMF（Modular Avatar）** ベースのビルド時コンポーネントパッケージです。  
パラメータの同期ビット削減・ワールド固定・オブジェクトリセット・モジュール追加など、アバターのビルド時に処理を行うコンポーネントを提供します。

## 必要環境

- Unity 2022.3 以降（VRChat SDK 対応バージョン）
- [VRChat Avatars](https://vcc.docs.vrchat.com/vpm/packages#vrchat-official-packages) 3.7.0 以上
- [NDMF](https://github.com/bdunderscore/ndmf) 1.5.0 以上
- [Modular Avatar](https://github.com/bdunderscore/modular-avatar) 1.8.0 以上

## インストール

こちらから
https://samirin33.github.io/Samirin33VPM/

### VPM（推奨）

1. [VRChat Creator Companion](https://vcc.docs.vrchat.com/vpm/installation) でプロジェクトを開く
2. パッケージ一覧に以下を追加  
   `https://github.com/samirin33/samirin-vrc-utility.avatars.git`

### 手動（UPM）

1. `Window` → `Package Manager` → `+` → `Add package from git URL`
2. 以下を入力  
   `https://github.com/samirin33/samirin-vrc-utility.avatars.git`

## 使い方

各コンポーネントは **Add Component** から **`samirin33 VRC`** カテゴリで追加できます。

| 機能 | コンポーネント名 | 説明 |
|------|------------------|------|
| [HalfSyncParam](docs/features/01-half-sync-param.md) | HalfSyncParam | パラメータの同期ビット数を削減（1〜7bit） |
| [WorldFix](docs/features/02-world-fix.md) | WorldFix | オブジェクトをワールド空間で固定（Constraint） |
| [GameObjectResetter](docs/features/03-game-object-resetter.md) | GameObjectResetter | ビルド時に有効/無効・位置・回転・スケールをリセット |
| [ParameterSmoothing](docs/features/04-parameter-smoothing.md) | ParameterSmoothing | パラメータのスムージング（HalfSyncParam からも利用） |
| [ModuleSetter](docs/features/05-module-setter.md) | ModuleSetter | ビルド時にモジュールプレファブをアバター直下に配置 |

詳細は **[機能別ドキュメント](docs/README.md)** を参照してください。

## ドキュメントの編集・追加

- 新機能の説明を追加するときは [機能ドキュメントのテンプレート](docs/FEATURE_TEMPLATE.md) をコピーして使用してください。
- 既存の機能説明は `docs/features/` 内の対応する `.md` を編集してください。

## ライセンス

MIT License
