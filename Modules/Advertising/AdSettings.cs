using System.Collections.Generic;
using Pancake.Apex;
using Pancake.ExLib;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Pancake.Monetization
{
    [HideMonoScript]
    [EditorIcon("scriptable_ad")]
    public class AdSettings : ScriptableObject
    {
        [Message("Default, ad will be auto loading.")] [Range(5, 100), SerializeField]
        private float adCheckingInterval = 8f;

        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;

        [HorizontalLine]
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        [Message("Admob plugin was imported")]
#else
        [Message("Admob plugin not found", Style = MessageStyle.Warning)]
#endif
        [SerializeField]
        private EAdNetwork currentNetwork = EAdNetwork.Applovin;


        [Header("[admob]")] [SerializeField, Label("Test Mode"), Order(4)]
        private bool admobEnableTestMode;

        [SerializeField, Array, ShowIf(nameof(admobEnableTestMode)), Label("Devices Test"), Order(5)]
        private List<string> admobDevicesTest;

        [SerializeField, Label("Client"), Order(6)] private AdmobClient admobClient;
        [SerializeField, Label("Banner"), Order(7)] private AdmobBannerVariable admobBanner;
        [SerializeField, Label("Interstitial"), Order(8)] private AdmobInterVariable admobInter;
        [SerializeField, Label("Rewarded"), Order(9)] private AdmobRewardVariable admobReward;
        [SerializeField, Label("Inter Rewarded"), Order(10)] private AdmobRewardInterVariable admobRewardInter;

        [HorizontalLine]
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        [Message("Applovin plugin was imported")]
#else
        [Message("Applovin plugin not found", Style = MessageStyle.Warning)]
#endif
        [SerializeField, Label("App Open"), Order(11)]
        private AdmobAppOpenVariable admobAppOpen;

        public List<string> AdmobDevicesTest => admobDevicesTest;
        public bool AdmobEnableTestMode => admobEnableTestMode;
        public AdmobClient AdmobClient => admobClient;
        public AdmobBannerVariable AdmobBanner => admobBanner;
        public AdmobInterVariable AdmobInter => admobInter;
        public AdmobRewardVariable AdmobReward => admobReward;
        public AdmobRewardInterVariable AdmobRewardInter => admobRewardInter;
        public AdmobAppOpenVariable AdmobAppOpen => admobAppOpen;


        [Header("[applovin]")] [SerializeField, TextArea, Order(14)] private string sdkKey;

        [SerializeField, Label("Client"), Order(15)] private ApplovinAdClient applovinClient;
        [SerializeField, Label("Banner"), Order(16)] private ApplovinBannerVariable applovinBanner;
        [SerializeField, Label("Interstitial"), Order(17)] private ApplovinInterVariable applovinInter;
        [SerializeField, Label("Rewarded"), Order(18)] private ApplovinRewardVariable applovinReward;
        [SerializeField, Label("Inter Rewarded"), Order(19)] private ApplovinRewardInterVariable applovinRewardInter;
        [SerializeField, Label("App Open"), Order(20)] private ApplovinAppOpenVariable applovinAppOpen;
        [SerializeField, Label("Age Restricted"), Order(21)] private bool applovinEnableAgeRestrictedUser;

        public string SDKKey => sdkKey;
        public ApplovinAdClient ApplovinClient => applovinClient;
        public ApplovinBannerVariable ApplovinBanner => applovinBanner;
        public ApplovinInterVariable ApplovinInter => applovinInter;
        public ApplovinRewardVariable ApplovinReward => applovinReward;
        public ApplovinRewardInterVariable ApplovinRewardInter => applovinRewardInter;
        public ApplovinAppOpenVariable ApplovinAppOpen => applovinAppOpen;
        public bool ApplovinEnableAgeRestrictedUser => applovinEnableAgeRestrictedUser;

        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }


#if UNITY_EDITOR

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        [SerializeMethod]
        [Color(1,
            0.16f,
            0.16f,
            0.66f,
            Target = ColorTarget.Background)]
        [HorizontalGroup("admob-uninstall")]
        [Order(3)]
        private void Uninstall_AdmobSdk()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group)) ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets", "GoogleMobileAds"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets", "GoogleMobileAds.meta"));

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/Android", "googlemobileads-unity.aar"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/Android", "googlemobileads-unity.aar.meta"));

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/Android", "GoogleMobileAdsPluginAndroidlib"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/Android", "GoogleMobileAdsPluginAndroidlib.meta"));

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/iOS", "GADUAdNetworkExtras.h"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/iOS", "GADUAdNetworkExtras.h.meta"));

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/iOS", "unity-plugin-library.a"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets/Plugins/iOS", "unity-plugin-library.a.meta"));

            AssetDatabase.Refresh();
        }

        [SerializeMethod]
        [Color(1f,
            0.72f,
            0.42f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("admob-uninstall")]
        [Order(2)]
        private void OpenGoogleAdmobSetting() { UnityEditor.EditorApplication.ExecuteMenuItem("Assets/Google Mobile Ads/Settings..."); }
#else
        [SerializeMethod]
        [Color(0.31f,
            0.98f,
            0.48f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("admob-install")]
        [Order(2)]
        private void Install_AdmobSdk()
        {
            DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing admob sdk");
            AssetDatabase.ImportPackage(GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/admob.unitypackage"), false);
        }
        
        [SerializeMethod]
        [Color(1f,
            0.72f,
            0.42f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("admob-install")]
        [Order(3)]
        private void AddAdmobSymbol()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                AssetDatabase.Refresh();
            }
        }
#endif

#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        [SerializeMethod]
        [Color(1,
            0.16f,
            0.16f,
            0.66f,
            Target = ColorTarget.Background)]
        [HorizontalGroup("applovin-uninstall")]
        [Order(13)]
        private void Uninstall_ApplovinSdk()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group)) ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");

            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets", "MaxSdk"));
            FileUtil.DeleteFileOrDirectory(System.IO.Path.Combine("Assets", "MaxSdk.meta"));

            AssetDatabase.Refresh();
        }

        [SerializeMethod]
        [Color(1f,
            0.72f,
            0.42f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("applovin-uninstall")]
        [Order(12)]
        private void OpenApplovinManager() { UnityEditor.EditorApplication.ExecuteMenuItem("AppLovin/Integration Manager"); }
#else
        [SerializeMethod]
        [Color(0.31f,
            0.98f,
            0.48f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("applovin-install")]
        [Order(12)]
        private void Install_ApplovinSdk()
        {
            DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing <color=#FF77C6>applovin</color> sdk");
            AssetDatabase.ImportPackage(GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/applovin.unitypackage"), false);
        }
        
        [SerializeMethod]
        [Color(1f,
            0.72f,
            0.42f,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("applovin-install")]
        [Order(13)]
        private void AddApplovinSymbol()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                AssetDatabase.Refresh();
            }
        }
#endif

        private string GetPathInCurrentEnvironent(string fullRelativePath)
        {
            var upmPath = $"Packages/com.pancake.heart/{fullRelativePath}";
            var normalPath = $"Assets/heart/{fullRelativePath}";
            return !System.IO.File.Exists(System.IO.Path.GetFullPath(upmPath)) ? normalPath : upmPath;
        }
        
        [UnityEngine.ContextMenu("Copy Admob Test AppId")]
        protected void FillDefaultTestId()
        {
            "ca-app-pub-3940256099942544~3347511713".CopyToClipboard();
            DebugEditor.Toast("[Admob] Copy AppId Test Id Success!");
        }
        
#endif
    }
}