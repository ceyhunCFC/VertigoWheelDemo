using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class WheelView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WheelDataSetSO wheelDataSet;

        [Header("References")]
        [SerializeField] private Image wheelBaseImage;
        [SerializeField] private WheelSlotView[] slotViews;

        private WheelDataSO currentWheelData;
        private WheelSlotData[] activeSlots;

        private void OnValidate()
        {
            AutoWire();
        }

        public void SetZoneType(ZoneType zoneType)
        {
            if (wheelDataSet == null) return;
            SetWheelData(wheelDataSet.GetWheelData(zoneType));
        }

        public int SlotCount => activeSlots != null ? activeSlots.Length : 0;

        public bool TryGetSlotData(int slotIndex, out WheelSlotData slotData)
        {
            if (activeSlots == null || slotIndex < 0 || slotIndex >= activeSlots.Length)
            {
                slotData = null;
                return false;
            }
            slotData = activeSlots[slotIndex];
            return slotData != null && slotData.Reward != null;
        }

        public bool TryGetSpinResult(int slotIndex, out WheelSpinResult result)
        {
            result = default;
            if (!TryGetSlotData(slotIndex, out WheelSlotData slotData))
            {
                return false;
            }

            RectTransform sourceIconTransform = null;
            if (slotViews != null && slotIndex >= 0 && slotIndex < slotViews.Length && slotViews[slotIndex] != null)
            {
                sourceIconTransform = slotViews[slotIndex].RewardIconTransform;
            }

            result = new WheelSpinResult(slotData, sourceIconTransform);
            return true;
        }

        public void ShuffleSlots()
        {
            if (activeSlots == null || activeSlots.Length == 0) return;

            for (int i = activeSlots.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                WheelSlotData tmp = activeSlots[i];
                activeSlots[i] = activeSlots[j];
                activeSlots[j] = tmp;
            }

            RefreshSlots();
        }

        private void SetWheelData(WheelDataSO wheelData)
        {
            if (currentWheelData == wheelData && activeSlots != null)
            {
                RefreshSlots();
                return;
            }

            currentWheelData = wheelData;
            if (currentWheelData == null) return;

            if (wheelBaseImage != null)
            {
                wheelBaseImage.sprite = currentWheelData.WheelSprite;
                wheelBaseImage.preserveAspect = true;
                wheelBaseImage.raycastTarget = false;
            }

            int count = currentWheelData.SlotCount;
            activeSlots = new WheelSlotData[count];
            for (int i = 0; i < count; i++)
            {
                activeSlots[i] = currentWheelData.GetSlot(i);
            }

            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (slotViews == null || activeSlots == null) return;

            for (int i = 0; i < slotViews.Length; i++)
            {
                if (slotViews[i] == null) continue;
                slotViews[i].SetData(i < activeSlots.Length ? activeSlots[i] : null);
            }
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            if (wheelBaseImage == null)
            {
                wheelBaseImage = FindImage("ui_image_spin_base");
            }

            Transform rewardRoot = transform.Find("ui_group_wheel_rewards");
            if (rewardRoot != null)
            {
                slotViews = rewardRoot.GetComponentsInChildren<WheelSlotView>(true);
            }
            else if (slotViews == null || slotViews.Length == 0)
            {
                slotViews = GetComponentsInChildren<WheelSlotView>(true);
            }
        }

        private Image FindImage(string objectName)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image.name.Trim() == objectName)
                {
                    return image;
                }
            }

            return null;
        }
    }
}
