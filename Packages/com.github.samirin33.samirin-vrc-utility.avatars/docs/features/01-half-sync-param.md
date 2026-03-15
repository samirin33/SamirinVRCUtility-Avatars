# HalfSyncParam

## 概要

VRChatで同期するパラメーターの同期範囲もしくは分解能を犠牲にして同期bit数を削減することができます。

## 追加方法

- **Add Component** → **samirin33 VRC** → **HalfSyncParam**
- アバターの Animator がアタッチされている GameObject、またはその子に追加します。

## 使い方

### 基本手順

1. HalfSyncParam をアタッチした GameObject を選択
2. **同期パラメータ設定** で要素を追加
3. 各要素で **パラメータ名**（空の場合は自動生成）、**型**（Int / FloatZeroToPlusOne / FloatMinusOneToPlusOne）、**ビット数**（_1bit 〜 _7bit）を指定
4. 必要に応じて **smoothWeight** でスムージングの強さを調整
5. **replaceWithSmoothedInAnimator** を有効にすると、ビルド時に Animator 内のそのパラメータ参照がスムージング後のパラメータ名に一括置換されます

### オプション・設定

- **writeDefault**: 生成されるレイヤーで Write Defaults の扱いを指定
- 複数パラメータを登録でき、Inspector で並び替え（↑↓）可能
- Float 型の設定がある場合、同一 GameObject に **ParameterSmoothing** がなければ自動で追加されます

## 注意事項

- スムージング処理は[AAP](https://vrc.school/docs/Other/AAPs/)によって行われます。処理後の値はVRCPrameterDriverで取得できないのでご注意ください。

## 関連

- [ParameterSmoothing](04-parameter-smoothing.md) — スムージング処理（HalfSyncParam から自動利用）
