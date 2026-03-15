# Animation Clip Selector

## 概要

**Animation ウィンドウ**で編集中の Animator を検出し、その Animator Controller に含まれる **編集可能な AnimationClip 一覧**を表示するエディタウィンドウです。クリップをクリックすると Animation ウィンドウの編集対象が切り替わり、同じパスに複数クリップがバインドされている**競合**も表示できます。

## 開き方

- メニュー: **`samirin33 Editor Tools`** → **`Animation Clip Selector`**
- ウィンドウタイトル: **Animation Clip Selector**

## 使い方

### 基本手順

1. **Animation ウィンドウ**を開き、編集したい GameObject（アバターなど）を選択して Animator を表示する
2. **Animation Clip Selector** を開く
3. 編集中の Animator に紐づくクリップが、レイヤー・サブステートマシンごとに一覧表示される
4. 一覧でクリップをクリックすると、**Animation ウィンドウの編集対象**がそのクリップに切り替わる
5. 同じバインドパスに複数クリップが割り当てられている場合は**競合**として表示され、詳細ウィンドウで確認できる

### オプション・設定

- **設定アセット**（AnimationClipSelectorSettings）で、アイテム間の余白や、競合警告を出さないクリップ（無視リスト）などを設定できます。
- 設定アセットは `Assets/SamirinVRCUtility/Editor/AnimationClipSelectorSettings.asset` に自動作成されます。

## 注意事項

- Animation ウィンドウで「編集中のルート」が決まっていないと、Selector 側で正しい Controller を取得できない場合があります。先に Animation ウィンドウで対象を選んでください。
- 競合は「同じパスに複数のクリップがバインドされている」状態を検出します。意図的に同じパスを使っている場合は、無視リストに追加すると警告を消せます。

## 関連

- [Animator デフォルト設定](01-animator-default-setting.md)
- [VRChat Avatar Param Setter](05-vrc-avatar-param-setter.md) — 同じ Controller のパラメータを整える場合
