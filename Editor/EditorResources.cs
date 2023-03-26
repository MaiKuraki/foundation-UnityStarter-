using UnityEngine;

namespace PancakeEditor
{
    internal static class EditorResources
    {
        private const string RELATIVE_PATH = "Editor/Misc/Icons";
        private const string RELATIVE_TEMPLATE_PATH = "Editor/Misc/Templates";

        internal static Texture2D BoxContentDark => Editor.FindAssetWithPath<Texture2D>("box_content_dark.png", RELATIVE_PATH);
        internal static Texture2D BoxBackgroundDark => Editor.FindAssetWithPath<Texture2D>("box_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D EvenBackground => Editor.FindAssetWithPath<Texture2D>("even_bg.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundBlue => Editor.FindAssetWithPath<Texture2D>("even_bg_select.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundDark => Editor.FindAssetWithPath<Texture2D>("even_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D ScriptableEvent => Editor.FindAssetWithPath<Texture2D>("scriptable_event.png", RELATIVE_PATH);
        internal static Texture2D ScriptableEventListener => Editor.FindAssetWithPath<Texture2D>("scriptable_event_listener.png", RELATIVE_PATH);
        internal static Texture2D ScriptableList => Editor.FindAssetWithPath<Texture2D>("scriptable_list.png", RELATIVE_PATH);
        internal static Texture2D ScriptableVariable => Editor.FindAssetWithPath<Texture2D>("scriptable_variable.png", RELATIVE_PATH);
        internal static Texture2D Dreamblale => Editor.FindAssetWithPath<Texture2D>("dreamblade.png", RELATIVE_PATH);
        internal static Texture2D StarEmpty => Editor.FindAssetWithPath<Texture2D>("star_empty.png", RELATIVE_PATH);
        internal static Texture2D StarFull => Editor.FindAssetWithPath<Texture2D>("star_full.png", RELATIVE_PATH);
        internal static Texture2D ScriptableAd => Editor.FindAssetWithPath<Texture2D>("scriptable_ad.png", RELATIVE_PATH);
        internal static Texture2D ScriptableIap => Editor.FindAssetWithPath<Texture2D>("scriptable_iap.png", RELATIVE_PATH);
        internal static Texture2D ScriptableFirebase => Editor.FindAssetWithPath<Texture2D>("scriptable_firebase.png", RELATIVE_PATH);
        internal static Texture2D ScriptableAdjust => Editor.FindAssetWithPath<Texture2D>("scriptable_adjust.png", RELATIVE_PATH);
        internal static Texture2D ScriptableTween => Editor.FindAssetWithPath<Texture2D>("scriptable_tween.png", RELATIVE_PATH);
        internal static Texture2D ScriptableNotification => Editor.FindAssetWithPath<Texture2D>("scriptable_noti.png", RELATIVE_PATH);
        internal static Texture2D ScriptableStar => Editor.FindAssetWithPath<Texture2D>("scriptable_star.png", RELATIVE_PATH);
        internal static TextAsset ScriptableEventListenerTemplate => Editor.FindAssetWithPath<TextAsset>("ScriptableEventListenerTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        internal static TextAsset ScriptableEventTemplate => Editor.FindAssetWithPath<TextAsset>("ScriptableEventTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        internal static TextAsset ScriptableListTemplate => Editor.FindAssetWithPath<TextAsset>("ScriptableListTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        internal static TextAsset ScriptableVariableTemplate => Editor.FindAssetWithPath<TextAsset>("ScriptableVariableTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
    }
}