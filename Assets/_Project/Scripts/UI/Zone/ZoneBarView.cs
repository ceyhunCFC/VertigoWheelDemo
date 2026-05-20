using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class ZoneBarView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ZoneConfigSO zoneConfig;
        [SerializeField] private ZoneVisualSetSO zoneVisualSet;

        [Header("Items")]
        [SerializeField] private RectTransform visibleArea;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private ZoneItemView zoneItemPrefab;
        [SerializeField, HideInInspector] private ZoneItemView[] zoneItems;

        [Header("Preview")]
        [SerializeField, Min(1)] private int startZone = 1;

        [Header("Animation")]
        [SerializeField, Min(0)] private int bufferItemCount = 2;
        [SerializeField, Min(0f)] private float itemGap = 30f;
        [SerializeField, Min(0.01f)] private float slideDuration = 0.3f;

        private ZoneService _zoneService;
        private bool _isAnimating;
        private int _nextZoneNumber;
        private float _itemStep;

        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            AutoWire();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            AutoWire();

            if (zoneConfig == null || zoneVisualSet == null)
            {
                return;
            }

            _zoneService = new ZoneService(zoneConfig);
            BuildItemsForVisibleArea();
            LayoutItems();
            SetStartZone(startZone);
            _nextZoneNumber = startZone + GetValidItemCount();
        }

        public void Advance()
        {
            if (_isAnimating || zoneItems == null || zoneItems.Length == 0)
            {
                return;
            }

            _isAnimating = true;
            KillItemTweens();
            RefreshItemStep();

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < zoneItems.Length; i++)
            {
                RectTransform itemTransform = GetItemTransform(zoneItems[i]);
                if (itemTransform == null)
                {
                    continue;
                }

                sequence.Join(itemTransform
                    .DOAnchorPosX(itemTransform.anchoredPosition.x - _itemStep, slideDuration)
                    .SetEase(Ease.OutCubic));
            }

            sequence.OnComplete(() =>
            {
                MoveItemsOutsideLeftToRight();
                _isAnimating = false;
            });
        }

        public void SetStartZone(int zone)
        {
            startZone = Mathf.Max(1, zone);

            if (_zoneService == null)
            {
                if (zoneConfig == null)
                {
                    return;
                }

                _zoneService = new ZoneService(zoneConfig);
            }

            RefreshItems(startZone);
            _nextZoneNumber = startZone + GetValidItemCount();
        }

        private void RefreshItems(int firstZone)
        {
            if (zoneItems == null || zoneVisualSet == null)
            {
                return;
            }

            for (int i = 0; i < zoneItems.Length; i++)
            {
                if (zoneItems[i] == null)
                {
                    continue;
                }

                int zoneNumber = firstZone + i;
                ApplyZoneData(zoneItems[i], zoneNumber);
                zoneItems[i].gameObject.SetActive(true);
            }
        }

        private void KillItemTweens()
        {
            if (zoneItems == null)
            {
                return;
            }

            foreach (ZoneItemView zoneItem in zoneItems)
            {
                if (zoneItem != null)
                {
                    zoneItem.transform.DOKill();
                }
            }
        }

        private void BuildItemsForVisibleArea()
        {
            AutoWire();
            RefreshItemStep();

            int requiredItemCount = GetRequiredItemCount();
            if (requiredItemCount <= 0)
            {
                return;
            }

            List<ZoneItemView> items = new List<ZoneItemView>(zoneItems ?? new ZoneItemView[0]);
            RectTransform parent = GetContentRoot();

            while (items.Count < requiredItemCount && zoneItemPrefab != null && parent != null)
            {
                ZoneItemView item = Instantiate(zoneItemPrefab, parent);
                item.name = $"ui_zone_item_{items.Count:00}";
                items.Add(item);
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    items[i].gameObject.SetActive(i < requiredItemCount);
                }
            }

            if (items.Count > requiredItemCount)
            {
                items.RemoveRange(requiredItemCount, items.Count - requiredItemCount);
            }

            zoneItems = items.ToArray();
        }

        private void RefreshItemStep()
        {
            _itemStep = GetItemWidth() + itemGap;
        }

        private int GetRequiredItemCount()
        {
            RectTransform area = visibleArea != null ? visibleArea : transform as RectTransform;
            if (area == null)
            {
                return zoneItems != null ? zoneItems.Length : 0;
            }

            float itemStep = GetItemWidth() + itemGap;
            if (itemStep <= 0.01f)
            {
                return zoneItems != null ? zoneItems.Length : 0;
            }

            return Mathf.CeilToInt(area.rect.width / itemStep) + bufferItemCount;
        }

        private float GetItemWidth()
        {
            RectTransform itemTransform = null;

            if (zoneItems != null)
            {
                foreach (ZoneItemView zoneItem in zoneItems)
                {
                    itemTransform = GetItemTransform(zoneItem);
                    if (itemTransform != null && itemTransform.rect.width > 0.01f)
                    {
                        return itemTransform.rect.width;
                    }
                }
            }

            itemTransform = GetItemTransform(zoneItemPrefab);
            if (itemTransform != null && itemTransform.rect.width > 0.01f)
            {
                return itemTransform.rect.width;
            }

            return 100f;
        }

        private void LayoutItems()
        {
            if (zoneItems == null)
            {
                return;
            }

            for (int i = 0; i < zoneItems.Length; i++)
            {
                RectTransform itemTransform = GetItemTransform(zoneItems[i]);
                if (itemTransform == null)
                {
                    continue;
                }

                itemTransform.anchoredPosition = new Vector2(i * _itemStep, itemTransform.anchoredPosition.y);
            }
        }

        private void MoveItemsOutsideLeftToRight()
        {
            if (zoneItems == null || zoneItems.Length == 0)
            {
                return;
            }

            bool movedItem;
            do
            {
                movedItem = false;
                float leftLimit = GetLeftLimit();
                float rightMostX = GetRightMostItemX();

                for (int i = 0; i < zoneItems.Length; i++)
                {
                    RectTransform itemTransform = GetItemTransform(zoneItems[i]);
                    if (itemTransform == null)
                    {
                        continue;
                    }

                    if (GetItemRightEdge(itemTransform) >= leftLimit)
                    {
                        continue;
                    }

                    itemTransform.anchoredPosition = new Vector2(rightMostX + _itemStep, itemTransform.anchoredPosition.y);
                    ApplyZoneData(zoneItems[i], _nextZoneNumber);
                    _nextZoneNumber++;
                    movedItem = true;
                    break;
                }
            }
            while (movedItem);
        }

        private void ApplyZoneData(ZoneItemView zoneItem, int zoneNumber)
        {
            if (zoneItem == null || _zoneService == null || zoneVisualSet == null)
            {
                return;
            }

            ZoneType zoneType = _zoneService.GetZoneType(zoneNumber);
            ZoneVisualDataSO visualData = zoneVisualSet.GetVisual(zoneType);
            zoneItem.SetData(zoneNumber, visualData);
        }

        private float GetLeftLimit()
        {
            RectTransform area = visibleArea != null ? visibleArea : transform as RectTransform;
            if (area == null || zoneItems == null || zoneItems.Length == 0)
            {
                return float.MinValue;
            }

            RectTransform itemParent = GetFirstItemParent();
            if (itemParent == null)
            {
                return float.MinValue;
            }

            Vector3 worldLeft = area.TransformPoint(new Vector3(area.rect.xMin, area.rect.center.y, 0f));
            return itemParent.InverseTransformPoint(worldLeft).x;
        }

        private float GetRightMostItemX()
        {
            float rightMostX = float.MinValue;

            if (zoneItems == null)
            {
                return 0f;
            }

            foreach (ZoneItemView zoneItem in zoneItems)
            {
                RectTransform itemTransform = GetItemTransform(zoneItem);
                if (itemTransform != null && itemTransform.anchoredPosition.x > rightMostX)
                {
                    rightMostX = itemTransform.anchoredPosition.x;
                }
            }

            return rightMostX > float.MinValue ? rightMostX : 0f;
        }

        private float GetItemRightEdge(RectTransform itemTransform)
        {
            return itemTransform.anchoredPosition.x + itemTransform.rect.width * (1f - itemTransform.pivot.x);
        }

        private RectTransform GetFirstItemParent()
        {
            if (zoneItems == null)
            {
                return null;
            }

            foreach (ZoneItemView zoneItem in zoneItems)
            {
                RectTransform itemTransform = GetItemTransform(zoneItem);
                if (itemTransform != null)
                {
                    return itemTransform.parent as RectTransform;
                }
            }

            return null;
        }

        private RectTransform GetItemTransform(ZoneItemView zoneItem)
        {
            return zoneItem != null ? zoneItem.transform as RectTransform : null;
        }

        private RectTransform GetContentRoot()
        {
            if (contentRoot != null)
            {
                return contentRoot;
            }

            RectTransform itemParent = GetFirstItemParent();
            if (itemParent != null)
            {
                return itemParent;
            }

            return transform as RectTransform;
        }

        private int GetValidItemCount()
        {
            if (zoneItems == null)
            {
                return 0;
            }

            int count = 0;
            foreach (ZoneItemView zoneItem in zoneItems)
            {
                if (zoneItem != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void AutoWire()
        {
            if (visibleArea == null)
            {
                visibleArea = transform as RectTransform;
            }

            if (contentRoot == null)
            {
                contentRoot = transform.Find("ui_group_zone_items") as RectTransform;
            }

            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }

            if (zoneItems == null || zoneItems.Length == 0)
            {
                zoneItems = GetComponentsInChildren<ZoneItemView>(true);
            }
        }
    }
}
