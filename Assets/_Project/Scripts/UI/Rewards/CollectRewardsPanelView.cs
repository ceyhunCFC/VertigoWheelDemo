using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class CollectRewardsPanelView : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField, HideInInspector] private TextMeshProUGUI rewardNameValue;
        [SerializeField, HideInInspector] private TextMeshProUGUI rewardAmountValue;
        [SerializeField, HideInInspector] private Image rewardIconImage;
        [SerializeField, HideInInspector] private RectTransform rewardIconTransform;
        [SerializeField, HideInInspector] private RectTransform chestTransform;
        [SerializeField, HideInInspector] private RectTransform lightRotator;

        [Header("Animation")]
        [SerializeField] private float iconMoveDuration = 0.45f;
        [SerializeField] private float itemHoldDuration = 0.65f;
        [SerializeField] private float lightRotateDuration = 8f;
        [SerializeField] private Vector2 chestSpawnOffset = new Vector2(0f, 45f);

        private readonly List<RewardStack> rewardQueue = new List<RewardStack>();
        private Sequence sequence;
        private Tween lightTween;
        private Action completed;
        private Vector2 iconTargetPosition;
        private Vector2 iconTargetSize;
        private Vector3 iconTargetScale;
        private int currentRewardIndex;
        private bool isAnimating;

        private void Awake()
        {
            AutoWire();
            CacheIconTarget();
        }

        private void OnValidate()
        {
            AutoWire();
        }

        private void OnDestroy()
        {
            KillAnimations();
        }

        public void Show(IReadOnlyList<RewardStack> rewards, Action onCompleted)
        {
            gameObject.SetActive(true);
            AutoWire();
            KillAnimations();
            rewardQueue.Clear();
            completed = onCompleted;

            if (rewards != null)
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    if (rewards[i] != null && rewards[i].Reward != null && rewards[i].Amount > 0)
                    {
                        rewardQueue.Add(rewards[i]);
                    }
                }
            }

            if (rewardQueue.Count == 0)
            {
                Complete();
                return;
            }

            CacheIconTarget();
            currentRewardIndex = 0;
            PlayItem(currentRewardIndex);
        }

        public void Hide()
        {
            KillAnimations();
            HideImmediate();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ShowNextItem();
        }

        private void ShowNextItem()
        {
            if (isAnimating)
            {
                return;
            }

            currentRewardIndex++;
            if (currentRewardIndex >= rewardQueue.Count)
            {
                Complete();
                return;
            }

            PlayItem(currentRewardIndex);
        }

        private void PlayItem(int index)
        {
            if (index >= rewardQueue.Count)
            {
                Complete();
                return;
            }

            isAnimating = true;
            RewardStack stack = rewardQueue[index];
            SetReward(stack);
            ResetIconToChest();
            StartLight();

            if (rewardIconTransform == null)
            {
                sequence = DOTween.Sequence();
                sequence.AppendInterval(itemHoldDuration);
                sequence.AppendCallback(() => isAnimating = false);
                return;
            }

            sequence = DOTween.Sequence();
            sequence.Append(rewardIconTransform.DOAnchorPos(iconTargetPosition, iconMoveDuration).SetEase(Ease.OutBack));
            sequence.Join(rewardIconTransform.DOScale(iconTargetScale, iconMoveDuration).SetEase(Ease.OutBack));
            sequence.AppendInterval(itemHoldDuration);
            sequence.AppendCallback(() => isAnimating = false);
        }

        private void SetReward(RewardStack stack)
        {
            if (rewardNameValue != null)
            {
                string displayName = string.IsNullOrEmpty(stack.Reward.DisplayName) ? stack.Reward.name : stack.Reward.DisplayName;
                rewardNameValue.text = displayName.ToUpperInvariant();
            }

            if (rewardAmountValue != null)
            {
                rewardAmountValue.text = $"x{stack.Amount:N0}";
            }

            if (rewardIconImage != null)
            {
                rewardIconImage.gameObject.SetActive(true);
                rewardIconImage.type = Image.Type.Simple;
                rewardIconImage.preserveAspect = true;
                rewardIconImage.sprite = stack.Reward.Icon;
                rewardIconImage.enabled = stack.Reward.Icon != null;
                Color iconColor = rewardIconImage.color;
                iconColor.a = 1f;
                rewardIconImage.color = iconColor;

                if (stack.Reward.Icon == null)
                {
                    Debug.LogWarning($"[CollectRewardsPanelView] Reward icon is missing for {stack.Reward.name}.", stack.Reward);
                }
            }
            else
            {
                Debug.LogWarning("[CollectRewardsPanelView] Reward icon image was not found. Check ui_image_collect_reward_icon name.", this);
            }
        }

        private void CacheIconTarget()
        {
            if (rewardIconTransform == null)
            {
                return;
            }

            iconTargetPosition = rewardIconTransform.anchoredPosition;
            iconTargetSize = rewardIconTransform.sizeDelta;
            iconTargetScale = rewardIconTransform.localScale;
            if (iconTargetScale == Vector3.zero)
            {
                iconTargetScale = Vector3.one;
            }
        }

        private void ResetIconToChest()
        {
            if (rewardIconTransform == null || chestTransform == null)
            {
                if (chestTransform == null)
                {
                    Debug.LogWarning("[CollectRewardsPanelView] Chest transform was not found. Check ui_image_collect_rewards_chest name.", this);
                }

                return;
            }

            Vector3 chestWorldPosition = chestTransform.TransformPoint(chestSpawnOffset);
            Vector2 chestLocalPosition = WorldToLocalPoint(chestWorldPosition, rewardIconTransform.parent as RectTransform);
            rewardIconTransform.sizeDelta = iconTargetSize;
            rewardIconTransform.anchoredPosition = chestLocalPosition;
            rewardIconTransform.localScale = Vector3.zero;
        }

        private Vector2 WorldToLocalPoint(Vector3 worldPosition, RectTransform targetParent)
        {
            if (targetParent == null)
            {
                return Vector2.zero;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPoint, camera, out Vector2 localPoint);
            return localPoint;
        }

        private void StartLight()
        {
            if (lightRotator == null)
            {
                return;
            }

            if (lightTween == null)
            {
                lightTween = lightRotator.DORotate(new Vector3(0f, 0f, -360f), lightRotateDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
        }

        private void Complete()
        {
            isAnimating = false;
            Hide();
            completed?.Invoke();
            completed = null;
        }

        private void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        private void KillAnimations()
        {
            isAnimating = false;
            sequence?.Kill();
            sequence = null;
            lightTween?.Kill();
            lightTween = null;
        }

        private void AutoWire()
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string objectName = texts[i].name.Trim().ToLowerInvariant();
                if (rewardNameValue == null && objectName.Contains("collect_reward_name_value"))
                {
                    rewardNameValue = texts[i];
                }
                else if (rewardAmountValue == null && objectName.Contains("collect_reward_amount_value"))
                {
                    rewardAmountValue = texts[i];
                }
            }

            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                string objectName = images[i].name.Trim().ToLowerInvariant();
                if (rewardIconImage == null && objectName.Contains("collect_reward_icon"))
                {
                    rewardIconImage = images[i];
                    rewardIconTransform = images[i].rectTransform;
                }
                else if (chestTransform == null && objectName.Contains("collect_rewards_chest"))
                {
                    chestTransform = images[i].rectTransform;
                }
            }

            RectTransform[] transforms = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                string objectName = transforms[i].name.Trim().ToLowerInvariant();
                if (lightRotator == null && objectName.Contains("collect_reward_light_rotator"))
                {
                    lightRotator = transforms[i];
                }
            }
        }
    }
}
