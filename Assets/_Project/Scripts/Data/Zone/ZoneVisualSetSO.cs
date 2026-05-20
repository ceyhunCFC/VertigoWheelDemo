using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "ZoneVisualSet", menuName = "VertigoWheel/Zone Visual Set")]
    public class ZoneVisualSetSO : ScriptableObject
    {
        [Header("Zone Visuals")]
        [SerializeField] private ZoneVisualDataSO normalZoneVisual;
        [SerializeField] private ZoneVisualDataSO safeZoneVisual;
        [SerializeField] private ZoneVisualDataSO superZoneVisual;

        public ZoneVisualDataSO GetVisual(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return safeZoneVisual;
                case ZoneType.Super:
                    return superZoneVisual;
                default:
                    return normalZoneVisual;
            }
        }
    }
}
