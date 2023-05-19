﻿using System;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinRewardInterVariable : AdUnitVariable
    {
        [NonSerialized] public Action completedCallback;
        [NonSerialized] public Action skippedCallback;

        private bool _registerCallback;
        public bool IsEarnRewarded { get; private set; }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return !string.IsNullOrEmpty(Id) && MaxSdk.IsRewardedInterstitialAdReady(Id);
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.ShowRewardedInterstitialAd(Id);
#endif
        }

        public override void Destroy() { }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!_registerCallback)
            {
                MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayedEvent += OnAdDisplayed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdHiddenEvent += OnAdHidden;
                MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.RewardedInterstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdReceivedRewardEvent += OnAdReceivedReward;
                MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                _registerCallback = true;
            }

            MaxSdk.LoadRewardedInterstitialAd(Id);
#endif
        }


#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { paidedCallback?.Invoke(info.Revenue, unit, info.NetworkName); }

        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { IsEarnRewarded = true; }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { C.CallActionClean(ref faildedToLoadCallback); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdSettings.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            if (AdSettings.ApplovinEnableRequestAdAfterHidden && !string.IsNullOrEmpty(Id)) MaxSdk.LoadRewardedInterstitialAd(Id);

            if (IsEarnRewarded)
            {
                C.CallActionClean(ref completedCallback);
                IsEarnRewarded = false;
                return;
            }

            C.CallActionClean(ref skippedCallback);
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            AdSettings.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }
#endif
    }
}