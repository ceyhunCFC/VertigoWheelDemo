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

        [Header("Lens")]
        [SerializeField] private RectTransform lensVisibleArea;
        [SerializeField] private RectTransform lensContentRoot;
        [SerializeField] private ZoneItemView lensItemPrefab;
        [SerializeField, HideInInspector] private ZoneItemView[] lensItems;

        [Header("Preview")]
        [SerializeField, Min(1)] private int startZone = 1;

        [Header("Animation")]
        [SerializeField, Min(0)] private int bufferItemCount = 2;
        [SerializeField, Min(0f)] private float itemGap = 30f;
        [SerializeField, Min(0.01f)] private float slideDuration = 0.3f;

        private ZoneService _zoneService;
        private bool _isAnimating;
        private int _nextZoneNumber;
        private int _nextLensZoneNumber;
        private float _itemStep;
        private float _lensItemStep;

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
            _nextZoneNumber = startZone + GetActiveItemCount(zoneItems);
            _nextLensZoneNumber = startZone + GetActiveItemCount(lensItems);
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
            AnimateLayer(sequence, zoneItems, _itemStep);
            AnimateLayer(sequence, lensItems, _lensItemStep);

            sequence.OnComplete(() =>
            {
                int enteringZoneNumber = _nextZoneNumber;
                int enteringLensZoneNumber = _nextLensZoneNumber;
                int movedCount = MoveItemsOutsideLeftToRight(zoneItems, visibleArea, _itemStep, enteringZoneNumber);
                MoveLeftMostItemsToRight(lensItems, _lensItemStep, enteringLensZoneNumber, movedCount);
                _nextZoneNumber += movedCount;
                _nextLensZoneNumber += movedCount;
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
            _nextZoneNumber = startZone + GetActiveItemCount(zoneItems);
            _nextLensZoneNumber = startZone + GetActiveItemCount(lensItems);
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

            RefreshLensItems(firstZone);
        }

        private void RefreshLensItems(int firstZone)
        {
            if (lensItems == null)
            {
                return;
            }

            for (int i = 0; i < lensItems.Length; i++)
            {
                if (lensItems[i] == null)
                {
                    continue;
                }

                int zoneNumber = firstZone + i;
                ApplyZoneData(lensItems[i], zoneNumber);
                lensItems[i].gameObject.SetActive(true);
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

            if (lensItems == null)
            {
                return;
            }

            foreach (ZoneItemView lensItem in lensItems)
            {
                if (lensItem != null)
                {
                    lensItem.transform.DOKill();
                }
            }
        }

        private void BuildItemsForVisibleArea()
        {
            AutoWire();
            RefreshItemStep();

            zoneItems = BuildLayerItems(zoneItems, zoneItemPrefab, GetContentRoot(), visibleArea, _itemStep);
            lensItems = BuildLayerItems(lensItems, lensItemPrefab, GetLensContentRoot(), visibleArea, _lensItemStep);
        }

        private ZoneItemView[] BuildLayerItems(
            ZoneItemView[] existingItems,
            ZoneItemView itemPrefab,
            RectTransform parent,
            RectTransform area,
            float itemStep)
        {
            int requiredItemCount = GetRequiredItemCount(existingItems, area, itemStep);
            if (requiredItemCount <= 0)
            {
                return existingItems;
            }

            List<ZoneItemView> items = new List<ZoneItemView>();
            if (existingItems != null)
            {
                foreach (ZoneItemView existingItem in existingItems)
                {
                    if (existingItem != null)
                    {
                        items.Add(existingItem);
                    }
                }
            }

            while (items.Count < requiredItemCount && itemPrefab != null && parent != null)
            {
                ZoneItemView item = Instantiate(itemPrefab, parent);
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

            return items.ToArray();
        }

        private void RefreshItemStep()
        {
            _itemStep = GetItemWidth(zoneItems, zoneItemPrefab) + itemGap;
            _lensItemStep = GetItemWidth(lensItems, lensItemPrefab);
        }

        private int GetRequiredItemCount(ZoneItemView[] items, RectTransform area, float itemStep)
        {
            area = area != null ? area : transform as RectTransform;
            if (area == null)
            {
                return items != null ? items.Length : 0;
            }

            if (itemStep <= 0.01f)
            {
                return items != null ? items.Length : 0;
            }

            return Mathf.CeilToInt(area.rect.width / itemStep) + bufferItemCount;
        }

        private float GetItemWidth(ZoneItemView[] items, ZoneItemView itemPrefab)
        {
            RectTransform itemTransform = null;

            if (items != null)
            {
                foreach (ZoneItemView zoneItem in items)
                {
                    itemTransform = GetItemTransform(zoneItem);
                    if (itemTransform != null && itemTransform.rect.width > 0.01f)
                    {
                        return itemTransform.rect.width;
                    }
                }
            }

            itemTransform = GetItemTransform(itemPrefab);
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

            LayoutLayerItems(lensItems, _lensItemStep);
        }

        private void LayoutLayerItems(ZoneItemView[] items, float itemStep)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                RectTransform itemTransform = GetItemTransform(items[i]);
                if (itemTransform == null)
                {
                    continue;
                }

                itemTransform.anchoredPosition = new Vector2(i * itemStep, itemTransform.anchoredPosition.y);
            }
        }

        private void AnimateLayer(Sequence sequence, ZoneItemView[] items, float itemStep)
        {
            if (sequence == null || items == null)
            {
                return;
            }

            foreach (ZoneItemView item in items)
            {
                RectTransform itemTransform = GetItemTransform(item);
                if (itemTransform == null)
                {
                    continue;
                }

                sequence.Join(itemTransform
                    .DOAnchorPosX(itemTransform.anchoredPosition.x - itemStep, slideDuration)
                    .SetEase(Ease.OutCubic));
            }
        }

        private int MoveItemsOutsideLeftToRight(ZoneItemView[] items, RectTransform area, float itemStep, int firstEnteringZoneNumber)
        {
            if (items == null || items.Length == 0)
            {
                return 0;
            }

            int movedCount = 0;
            bool movedItem;
            do
            {
                movedItem = false;
                float leftLimit = GetLeftLimit(items, area);
                float rightMostX = GetRightMostItemX(items);

                for (int i = 0; i < items.Length; i++)
                {
                    RectTransform itemTransform = GetItemTransform(items[i]);
                    if (itemTransform == null)
                    {
                        continue;
                    }

                    if (GetItemRightEdge(itemTransform) >= leftLimit)
                    {
                        continue;
                    }

                    itemTransform.anchoredPosition = new Vector2(rightMostX + itemStep, itemTransform.anchoredPosition.y);
                    ApplyZoneData(items[i], firstEnteringZoneNumber + movedCount);
                    movedCount++;
                    movedItem = true;
                    break;
                }
            }
            while (movedItem);

            return movedCount;
        }

        private void MoveLeftMostItemsToRight(ZoneItemView[] items, float itemStep, int firstEnteringZoneNumber, int moveCount)
        {
            if (items == null || items.Length == 0 || moveCount <= 0)
            {
                return;
            }

            for (int i = 0; i < moveCount; i++)
            {
                ZoneItemView leftMostItem = GetLeftMostItem(items);
                RectTransform itemTransform = GetItemTransform(leftMostItem);
                if (itemTransform == null)
                {
                    continue;
                }

                float rightMostX = GetRightMostItemX(items);
                itemTransform.anchoredPosition = new Vector2(rightMostX + itemStep, itemTransform.anchoredPosition.y);
                ApplyZoneData(leftMostItem, firstEnteringZoneNumber + i);
            }
        }

        private ZoneItemView GetLeftMostItem(ZoneItemView[] items)
        {
            ZoneItemView leftMostItem = null;
            float leftMostX = float.MaxValue;

            foreach (ZoneItemView zoneItem in items)
            {
                RectTransform itemTransform = GetItemTransform(zoneItem);
                if (itemTransform == null || itemTransform.anchoredPosition.x >= leftMostX)
                {
                    continue;
                }

                leftMostX = itemTransform.anchoredPosition.x;
                leftMostItem = zoneItem;
            }

            return leftMostItem;
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

        private float GetLeftLimit(ZoneItemView[] items, RectTransform area)
        {
            area = area != null ? area : transform as RectTransform;
            if (area == null || items == null || items.Length == 0)
            {
                return float.MinValue;
            }

            RectTransform itemParent = GetFirstItemParent(items);
            if (itemParent == null)
            {
                return float.MinValue;
            }

            Vector3 worldLeft = area.TransformPoint(new Vector3(area.rect.xMin, area.rect.center.y, 0f));
            return itemParent.InverseTransformPoint(worldLeft).x;
        }

        private float GetRightMostItemX(ZoneItemView[] items)
        {
            float rightMostX = float.MinValue;

            if (items == null)
            {
                return 0f;
            }

            foreach (ZoneItemView zoneItem in items)
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

        private RectTransform GetFirstItemParent(ZoneItemView[] items)
        {
            if (items == null)
            {
                return null;
            }

            foreach (ZoneItemView zoneItem in items)
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

        private int GetActiveItemCount(ZoneItemView[] items)
        {
            if (items == null)
            {
                return 0;
            }

            int count = 0;
            foreach (ZoneItemView item in items)
            {
                if (item != null && item.gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private RectTransform GetContentRoot()
        {
            if (contentRoot != null)
            {
                return contentRoot;
            }

            RectTransform itemParent = GetFirstItemParent(zoneItems);
            if (itemParent != null)
            {
                return itemParent;
            }

            return transform as RectTransform;
        }

        private RectTransform GetLensContentRoot()
        {
            if (lensContentRoot != null)
            {
                return lensContentRoot;
            }

            RectTransform itemParent = GetFirstItemParent(lensItems);
            if (itemParent != null)
            {
                return itemParent;
            }

            return transform as RectTransform;
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
                zoneItems = contentRoot != null
                    ? contentRoot.GetComponentsInChildren<ZoneItemView>(true)
                    : new ZoneItemView[0];
            }

            if (lensVisibleArea == null)
            {
                Transform lensArea = transform.Find("ui_mask_zone_lens_viewport");
                lensVisibleArea = lensArea as RectTransform;
            }

            if (lensContentRoot == null)
            {
                Transform lensContent = transform.Find("ui_mask_zone_lens_viewport/ui_group_zone_lens_items");
                lensContentRoot = lensContent as RectTransform;
            }

            lensItems = lensContentRoot != null
                ? lensContentRoot.GetComponentsInChildren<ZoneItemView>(true)
                : new ZoneItemView[0];

            if (lensItemPrefab == null && lensItems.Length > 0)
            {
                lensItemPrefab = lensItems[0];
            }
        }
    }
}
