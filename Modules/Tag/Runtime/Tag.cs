﻿using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Pancake.BTag
{
    [CreateAssetMenu(fileName = "Tag", menuName = "Pancake/BTag/Tag")]
    [EditorIcon("scriptable_tag")]
    public class Tag : ScriptableBTag
    {
        public Action<GameObject> OnGameObjectStart;
        public Action<GameObject> OnGameObjectEnabled;
        public Action<GameObject> OnGameObjectDisabled;
        public Action<GameObject> OnGameObjectDestroyed;

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += state => ClearListeners(state);
#endif
        }

#if UNITY_EDITOR
        private void ClearListeners(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode) return;
            ClearListeners();
        }
#endif
        private void ClearListeners()
        {
            OnGameObjectStart = null;
            OnGameObjectEnabled = null;
            OnGameObjectDisabled = null;
            OnGameObjectDestroyed = null;
        }

        public static implicit operator Tag(string tagName) => BTag.ByName(tagName);
    }
}