using System;
using UnityEngine;

namespace VertigoWheel.Gameplay
{
    [Serializable]
    public class WheelSlotData
    {
        [SerializeField] private RewardDataSO reward;
        [SerializeField, Min(1)] private int amount = 1;

        public RewardDataSO Reward => reward;
        public int Amount => amount;
    }
}
