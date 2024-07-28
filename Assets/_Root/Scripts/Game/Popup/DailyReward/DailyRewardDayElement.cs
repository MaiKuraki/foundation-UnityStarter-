using System;
using System.Collections.Generic;
using Alchemy.Serialization;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Common;
using Pancake.Localization;
using TMPro;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    using UnityEngine;

    [AlchemySerialize]
    public partial class DailyRewardDayElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textDay;
        [SerializeField] private LocaleTextComponent localeTextDay;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject claimedOverlayObject;
        [SerializeField] private Image boderCurrentDay;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Image imageCircleClaimed;
        [SerializeField] private Image imageTickClaimed;
        [AlchemySerializeField, NonSerialized] private Dictionary<EDailyRewardDayStatus, GameObject> _dayStatusViews = new();

        private DailyRewardData _data;
        private Action _refreshAllElement;

        public int Day => _data.day;

        public void Setup(DailyRewardData data, Func<EDailyRewardType, DayRewardComponent> getObjectRewardFunc, Color claimableColor, Action refreshAllElement)
        {
            _data = data;
            _refreshAllElement = refreshAllElement;
            localeTextDay.UpdateArgs($"{data.day}");
            content.RemoveAllChildren();
            foreach (var reward in data.rewards)
            {
                var prefab = getObjectRewardFunc.Invoke(reward.typeReward);
                var dayReward = Instantiate(prefab, content);
                dayReward.Setup(reward.amout);
            }

            int currentDay = UserData.GetDailyRewardDay();

            if (UserData.IsDailyRewardNewDay())
            {
                if (_data.day == currentDay)
                {
                    textDay.color = claimableColor;
                    if (_data.day != 7)
                    {
                        boderCurrentDay.gameObject.SetActive(true);
                        LMotion.Create(0f, 1f, 1f).WithEase(Ease.Linear).WithLoops(-1, LoopType.Yoyo).BindToColorA(boderCurrentDay).AddTo(gameObject);
                    }
                    else
                    {
                        boderCurrentDay.gameObject.SetActive(false);
                    }

                    _dayStatusViews[EDailyRewardDayStatus.Claimable].SetActive(true);
                    var button = _dayStatusViews[EDailyRewardDayStatus.Claimable].GetComponent<Button>();
                    button.onClick.RemoveListener(OnButtonClaimPressed);
                    button.onClick.AddListener(OnButtonClaimPressed);
                }
                else if (_data.day < currentDay)
                {
                    textDay.color = defaultColor;
                    _dayStatusViews[EDailyRewardDayStatus.Claimed].SetActive(true);
                }
                else
                {
                    textDay.color = defaultColor;
                    _dayStatusViews[EDailyRewardDayStatus.Locked].SetActive(true);
                }
            }
            else
            {
                if (UserData.GetStatusAllDayClaimedInWeek())
                {
                    textDay.color = defaultColor;
                    _dayStatusViews[EDailyRewardDayStatus.Claimed].SetActive(true);
                }
                else
                {
                    if (_data.day < currentDay)
                    {
                        textDay.color = defaultColor;
                        _dayStatusViews[EDailyRewardDayStatus.Claimed].SetActive(true);
                    }
                    else
                    {
                        textDay.color = defaultColor;
                        _dayStatusViews[EDailyRewardDayStatus.Locked].SetActive(true);
                    }
                }
            }
        }

        private async void OnButtonClaimPressed()
        {
            UserData.SetStatusAllDayClaimedInWeek(false);
            _dayStatusViews[EDailyRewardDayStatus.Claimable].SetActive(false);
            foreach (var reward in _data.rewards)
            {
                // todo claim reward
            }

            if (UserData.GetDailyRewardDay() == 7)
            {
                UserData.SetDailyRewardDay(1); // reset day
                UserData.NextWeekDailyReward(); // next week
                UserData.SetStatusAllDayClaimedInWeek(true);
            }
            else UserData.NextDayDailyReward(); // next day

            UserData.SetDailyRewardLastTimeUpdate();

            // wait animation
            await PlayAnimationClaimed();
            _refreshAllElement?.Invoke();
        }

        private async UniTask PlayAnimationClaimed()
        {
            _dayStatusViews[EDailyRewardDayStatus.Claimed].SetActive(true);
            imageCircleClaimed.fillAmount = 0;
            imageTickClaimed.fillAmount = 0;
            await LMotion.Create(0f, 1f, 0.4f)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => { LMotion.Create(0f, 1f, 0.3f).WithEase(Ease.OutQuad).BindToFillAmount(imageTickClaimed).AddTo(gameObject); })
                .BindToFillAmount(imageCircleClaimed)
                .AddTo(gameObject);
        }
    }
}