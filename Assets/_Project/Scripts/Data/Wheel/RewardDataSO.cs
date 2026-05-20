using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [CreateAssetMenu(fileName = "RewardData", menuName = "VertigoWheel/Wheel/Reward Data")]
    public class RewardDataSO : ScriptableObject
    {
        [SerializeField] private string rewardId;
        [SerializeField] private string displayName;
        [SerializeField] private RewardType rewardType;
        [SerializeField] private Sprite icon;

        public string RewardId => rewardId;
        public string DisplayName => displayName;
        public RewardType RewardType => rewardType;
        public Sprite Icon => icon;
    }
}
