using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "ZoneConfig", menuName = "VertigoWheel/Zone Config")]
    public class ZoneConfigSO : ScriptableObject
    {
        [Header("Progression")]
        [SerializeField, Min(1)] private int safeZoneInterval = 5;
        [SerializeField, Min(1)] private int superZoneInterval = 30;
        [SerializeField, Min(1)] private int totalZoneCount = 60;

        public int SafeZoneInterval => safeZoneInterval;
        public int SuperZoneInterval => superZoneInterval;
        public int TotalZoneCount => totalZoneCount;
    }
}
