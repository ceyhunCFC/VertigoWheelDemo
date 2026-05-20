using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class RewardPanelView : MonoBehaviour
    {
        [SerializeField] private RectTransform rewardListRoot;
        [SerializeField] private RewardItemView rewardItemTemplate;
        [SerializeField] private RectTransform flyAnimationRoot;
        [SerializeField, Min(0.01f)] private float flyDuration = 0.45f;

        private readonly Dictionary<string, RewardItemView> rewardItems = new Dictionary<string, RewardItemView>();

        private void Awake()
        {
            AutoWire();

            if (rewardItemTemplate != null)
            {
                rewardItemTemplate.gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            AutoWire();
        }

        public void ShowReward(RewardStack stack, int previousAmount, RectTransform sourceIconTransform)
        {
            ShowReward(stack, previousAmount, sourceIconTransform, null);
        }

        public void ShowReward(RewardStack stack, int previousAmount, RectTransform sourceIconTransform, TweenCallback onComplete)
        {
            if (stack == null || stack.Reward == null)
            {
                onComplete?.Invoke();
                return;
            }

            AutoWire();
            RewardItemView itemView = GetOrCreateItem(stack.Reward);
            itemView.SetData(stack.Reward, previousAmount);
            ForceRewardListLayout();

            if (sourceIconTransform != null)
            {
                PlayFlyAnimation(stack.Reward, sourceIconTransform, itemView.IconTransform, () =>
                {
                    itemView.AnimateAmount(previousAmount, stack.Amount);
                    onComplete?.Invoke();
                });
            }
            else
            {
                itemView.AnimateAmount(previousAmount, stack.Amount);
                onComplete?.Invoke();
            }
        }

        public void Clear()
        {
            foreach (RewardItemView itemView in rewardItems.Values)
            {
                if (itemView != null)
                {
                    Destroy(itemView.gameObject);
                }
            }

            rewardItems.Clear();
        }

        private RewardItemView GetOrCreateItem(RewardDataSO reward)
        {
            string rewardKey = string.IsNullOrEmpty(reward.RewardId) ? reward.name : reward.RewardId;
            if (rewardItems.TryGetValue(rewardKey, out RewardItemView itemView) && itemView != null)
            {
                return itemView;
            }

            itemView = Instantiate(rewardItemTemplate, rewardListRoot);
            itemView.name = $"ui_reward_item_{rewardKey}";
            itemView.gameObject.SetActive(true);
            rewardItems[rewardKey] = itemView;
            return itemView;
        }

        private void PlayFlyAnimation(RewardDataSO reward, RectTransform from, RectTransform to, TweenCallback onComplete)
        {
            if (reward == null || from == null || to == null)
            {
                onComplete?.Invoke();
                return;
            }

            RectTransform parent = GetFlyAnimationRoot();
            if (parent == null)
            {
                onComplete?.Invoke();
                return;
            }

            GameObject flyObject = new GameObject("ui_image_reward_fly_icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform flyTransform = flyObject.transform as RectTransform;
            flyTransform.SetParent(parent, false);
            flyTransform.sizeDelta = from.rect.size;

            Image flyImage = flyObject.GetComponent<Image>();
            flyImage.sprite = reward.Icon;
            flyImage.preserveAspect = true;
            flyImage.raycastTarget = false;

            Canvas canvas = parent.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 startPosition = WorldToLocalPoint(parent, from.position, uiCamera);
            Vector2 targetPosition = WorldToLocalPoint(parent, to.position, uiCamera);

            flyTransform.anchorMin = new Vector2(0.5f, 0.5f);
            flyTransform.anchorMax = new Vector2(0.5f, 0.5f);
            flyTransform.pivot = new Vector2(0.5f, 0.5f);
            flyTransform.anchoredPosition = startPosition;
            flyTransform.localScale = Vector3.one;

            Sequence sequence = DOTween.Sequence();
            sequence.Join(flyTransform.DOAnchorPos(targetPosition, flyDuration).SetEase(Ease.InOutCubic));
            sequence.Join(flyTransform.DOScale(0.55f, flyDuration).SetEase(Ease.InCubic));
            sequence.OnComplete(() =>
            {
                Destroy(flyObject);
                onComplete?.Invoke();
            });
        }

        private RectTransform GetFlyAnimationRoot()
        {
            if (flyAnimationRoot != null)
            {
                return flyAnimationRoot;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            return canvas != null ? canvas.transform as RectTransform : transform as RectTransform;
        }

        private Vector2 WorldToLocalPoint(RectTransform parent, Vector3 worldPosition, Camera uiCamera)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, uiCamera, out Vector2 localPoint);
            return localPoint;
        }

        private void ForceRewardListLayout()
        {
            if (rewardListRoot == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rewardListRoot);
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            if (rewardListRoot == null)
            {
                Transform listTransform = transform.Find("ui_group_reward_list");
                rewardListRoot = listTransform as RectTransform;
            }

            if (rewardItemTemplate == null && rewardListRoot != null)
            {
                Transform templateTransform = rewardListRoot.Find("ui_reward_item_template");
                if (templateTransform != null)
                {
                    rewardItemTemplate = templateTransform.GetComponent<RewardItemView>();
                }
            }
        }
    }
}
