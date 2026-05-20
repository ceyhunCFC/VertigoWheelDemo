using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "ZoneVisual", menuName = "VertigoWheel/Zone Visual Data")]
    public class ZoneVisualDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private ZoneType zoneType;

        [Header("Zone Bar")]
        [SerializeField] private Sprite zoneBarSprite;
        [SerializeField] private Color textColor = Color.white;

        [Header("Wheel")]
        [SerializeField] private Sprite wheelSprite;

        [Header("Rules")]
        [SerializeField] private bool canExit;

        public ZoneType ZoneType => zoneType;
        public Sprite ZoneBarSprite => zoneBarSprite;
        public Color TextColor => textColor;
        public Sprite WheelSprite => wheelSprite;
        public bool CanExit => canExit;
    }
}
