using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "WheelDataSet", menuName = "VertigoWheel/Wheel/Wheel Data Set")]
    public class WheelDataSetSO : ScriptableObject
    {
        [SerializeField] private WheelDataSO bronzeWheel;
        [SerializeField] private WheelDataSO silverWheel;
        [SerializeField] private WheelDataSO goldenWheel;

        public WheelDataSO GetWheelData(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return silverWheel;
                case ZoneType.Super:
                    return goldenWheel;
                default:
                    return bronzeWheel;
            }
        }
    }
}
