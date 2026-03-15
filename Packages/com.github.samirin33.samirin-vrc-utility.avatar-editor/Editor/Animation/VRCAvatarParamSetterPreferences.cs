using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Samirin33.AvatarEditor.Animation.Editor
{
    /// <summary>
    /// 一括追加時に除外するパラメータをプロジェクトごとに保存（EditorPrefs）
    /// </summary>
    public static class VRCAvatarParamSetterPreferences
    {
        const string PrefsKeyPrefix = "VRCAvatarParamSetter_Excluded_";
        const string InitializedKeyPrefix = "VRCAvatarParamSetter_Initialized_";
        const string AddAtFrontKeyPrefix = "VRCAvatarParamSetter_AddAtFront_";

        static string PrefsKey => PrefsKeyPrefix + Application.dataPath;
        static string InitializedKey => InitializedKeyPrefix + Application.dataPath;
        static string AddAtFrontKey => AddAtFrontKeyPrefix + Application.dataPath;

        static HashSet<string> s_excluded;

        static HashSet<string> GetExcludedSet()
        {
            if (s_excluded != null)
                return s_excluded;

            s_excluded = new HashSet<string>();

            if (!EditorPrefs.GetBool(InitializedKey, false))
            {
                foreach (var p in VRChatBuiltInParams.All)
                {
                    if (p.DefaultExcluded)
                        s_excluded.Add(p.Name);
                }
                Save();
                EditorPrefs.SetBool(InitializedKey, true);
                return s_excluded;
            }

            try
            {
                string json = EditorPrefs.GetString(PrefsKey, "[]");
                var list = JsonUtility.FromJson<Wrapper>(json);
                if (list?.names != null)
                {
                    foreach (var n in list.names)
                        s_excluded.Add(n);
                }
            }
            catch
            {
                s_excluded = new HashSet<string>();
            }
            return s_excluded;
        }

        [Serializable]
        class Wrapper { public List<string> names = new List<string>(); }

        static void Save()
        {
            var set = GetExcludedSet();
            var list = new List<string>(set);
            var w = new Wrapper { names = list };
            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(w));
        }

        public static bool IsExcluded(string parameterName)
        {
            return GetExcludedSet().Contains(parameterName);
        }

        public static void SetExcluded(string parameterName, bool excluded)
        {
            var set = GetExcludedSet();
            if (excluded)
                set.Add(parameterName);
            else
                set.Remove(parameterName);
            Save();
        }

        /// <summary>すべてのパラメータを除外する、または除外解除する。</summary>
        public static void SetAllExcluded(bool excluded)
        {
            var set = GetExcludedSet();
            set.Clear();
            if (excluded)
            {
                foreach (var p in VRChatBuiltInParams.All)
                    set.Add(p.Name);
            }
            Save();
        }

        /// <summary>DefaultExcluded の設定に戻す。</summary>
        public static void ResetToDefault()
        {
            var set = GetExcludedSet();
            set.Clear();
            foreach (var p in VRChatBuiltInParams.All)
            {
                if (p.DefaultExcluded)
                    set.Add(p.Name);
            }
            Save();
        }

        public static void ClearCache()
        {
            s_excluded = null;
        }

        /// <summary>追加したパラメータを先頭に移動するか。</summary>
        public static bool AddParametersAtFront
        {
            get => EditorPrefs.GetBool(AddAtFrontKey, true);
            set => EditorPrefs.SetBool(AddAtFrontKey, value);
        }
    }
}
