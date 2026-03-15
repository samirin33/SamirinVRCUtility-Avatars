using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Samirin33.AvatarEditor.Animation.Editor
{
    public static class VRChatBuiltInParams
    {
        public struct ParamDef
        {
            public string Name;
            public AnimatorControllerParameterType Type;
            public bool DefaultExcluded;
            public string Description;

            public ParamDef(string name, AnimatorControllerParameterType type, bool excluded, string description = "")
            {
                Name = name;
                Type = type;
                DefaultExcluded = excluded;
                Description = description;
            }
        }

        public static IReadOnlyList<ParamDef> All { get; } = new List<ParamDef>
        {
            new ParamDef("IsLocal", AnimatorControllerParameterType.Bool, false,
                "このアバターを自分が着用している場合 true、他人のアバターとして表示されている場合は false。"),
            new ParamDef("PreviewMode", AnimatorControllerParameterType.Int, true,
                "エディタプレビュー用。本番では使用しないため通常は除外。"),
            new ParamDef("Viseme", AnimatorControllerParameterType.Int, false,
                "リップシンク用。Oculus viseme インデックス 0–14。Jawbone/Jawflap 使用時は 0–100 で音量を表す。"),
            new ParamDef("Voice", AnimatorControllerParameterType.Float, false,
                "マイク音量。0.0～1.0 の範囲。"),
            new ParamDef("GestureLeft", AnimatorControllerParameterType.Int, false,
                "左手ジェスチャー。0=Neutral, 1=Fist, 2=HandOpen, 3=FingerPoint, 4=Victory, 5=RockNRoll, 6=HandGun, 7=ThumbsUp。"),
            new ParamDef("GestureRight", AnimatorControllerParameterType.Int, false,
                "右手ジェスチャー。0=Neutral, 1=Fist, 2=HandOpen, 3=FingerPoint, 4=Victory, 5=RockNRoll, 6=HandGun, 7=ThumbsUp。"),
            new ParamDef("GestureLeftWeight", AnimatorControllerParameterType.Float, false,
                "左手アナログトリガーの押し具合。0.0～1.0。トリガーを引くほど増加し、アナログジェスチャーに利用可能。"),
            new ParamDef("GestureRightWeight", AnimatorControllerParameterType.Float, false,
                "右手アナログトリガーの押し具合。0.0～1.0。トリガーを引くほど増加。"),
            new ParamDef("AngularY", AnimatorControllerParameterType.Float, true,
                "Y軸まわりの角速度。回転の速さに応じて変化。"),
            new ParamDef("VelocityX", AnimatorControllerParameterType.Float, false,
                "左右方向の移動速度 (m/s)。"),
            new ParamDef("VelocityY", AnimatorControllerParameterType.Float, false,
                "上下方向の移動速度 (m/s)。"),
            new ParamDef("VelocityZ", AnimatorControllerParameterType.Float, false,
                "前後方向の移動速度 (m/s)。"),
            new ParamDef("VelocityMagnitude", AnimatorControllerParameterType.Float, false,
                "移動速度の合計の大きさ（スカラー）。"),
            new ParamDef("Upright", AnimatorControllerParameterType.Float, false,
                "直立度。0=うつ伏せに近い、1=まっすぐ立っている。"),
            new ParamDef("Grounded", AnimatorControllerParameterType.Bool, false,
                "地面（または足場）に接触している場合 true。"),
            new ParamDef("Seated", AnimatorControllerParameterType.Bool, true,
                "ステーションに座っている場合 true。"),
            new ParamDef("AFK", AnimatorControllerParameterType.Bool, true,
                "離席中の場合 true。Endキー、HMDを外した時、一部のシステムメニュー表示時に true。"),
            new ParamDef("TrackingType", AnimatorControllerParameterType.Int, true,
                "トラッキング種別。0=未初期化, 1=Generic, 2=ハンドのみ(遷移中), 3=3点(頭+手), 4=4点(+腰), 6=フルボディ。VRMode が 1 のとき 3/4/6 が有効。"),
            new ParamDef("VRMode", AnimatorControllerParameterType.Int, false,
                "VR 利用時は 1、デスクトップ（非VR）時は 0。"),
            new ParamDef("MuteSelf", AnimatorControllerParameterType.Bool, true,
                "自分をミュートしている場合 true。"),
            new ParamDef("InStation", AnimatorControllerParameterType.Bool, true,
                "ステーション内にいる場合 true。"),
            new ParamDef("Earmuffs", AnimatorControllerParameterType.Bool, true,
                "イヤーマフ機能がオンの場合 true（他人の声を減衰）。"),
            new ParamDef("IsOnFriendsList", AnimatorControllerParameterType.Bool, true,
                "このユーザーが自分のフレンドリストに含まれる場合 true。"),
            new ParamDef("AvatarVersion", AnimatorControllerParameterType.Int, true,
                "アバターのバージョン番号。同一アバターのバージョン判別に利用。"),
            new ParamDef("IsAnimatorEnabled", AnimatorControllerParameterType.Bool, true,
                "アニメーターが有効になっている場合 true。"),
            new ParamDef("ScaleModified", AnimatorControllerParameterType.Bool, true,
                "アバターのスケールが変更されている場合 true。"),
            new ParamDef("ScaleFactor", AnimatorControllerParameterType.Float, true,
                "現在のスケール係数。デフォルトアバターサイズに対する倍率。"),
            new ParamDef("ScaleFactorInverse", AnimatorControllerParameterType.Float, true,
                "スケール係数の逆数。計算用。"),
            new ParamDef("EyeHeightAsMeters", AnimatorControllerParameterType.Float, true,
                "目の高さをメートルで表した値。"),
            new ParamDef("EyeHeightAsPercent", AnimatorControllerParameterType.Float, true,
                "目の高さをパーセント（0～1）で表した値。"),
        };
    }
}
