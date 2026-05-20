using UnityEngine;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public readonly struct WheelSpinResult
    {
        public WheelSpinResult(WheelSlotData slotData, RectTransform sourceIconTransform)
        {
            SlotData = slotData;
            SourceIconTransform = sourceIconTransform;
        }

        public WheelSlotData SlotData { get; }
        public RectTransform SourceIconTransform { get; }
    }
}
