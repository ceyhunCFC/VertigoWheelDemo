using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "WheelData", menuName = "VertigoWheel/Wheel/Wheel Data")]
    public class WheelDataSO : ScriptableObject
    {
        [SerializeField] private ZoneType zoneType;
        [SerializeField] private Sprite wheelSprite;
        [SerializeField] private WheelSlotData[] slots = new WheelSlotData[8];

        public ZoneType ZoneType => zoneType;
        public Sprite WheelSprite => wheelSprite;
        public int SlotCount => slots != null ? slots.Length : 0;

        public WheelSlotData GetSlot(int index)
        {
            if (slots == null || slots.Length == 0)
            {
                return null;
            }

            if (index < 0 || index >= slots.Length)
            {
                return null;
            }

            return slots[index];
        }
    }
}
