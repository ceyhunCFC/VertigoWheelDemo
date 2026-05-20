using System.Collections.Generic;

namespace VertigoWheel.Gameplay
{
    public class RewardInventory
    {
        private readonly Dictionary<string, RewardStack> rewardStacks = new Dictionary<string, RewardStack>();

        public RewardStack AddReward(RewardDataSO reward, int amount, out int previousAmount)
        {
            previousAmount = 0;
            if (reward == null || amount <= 0)
            {
                return null;
            }

            string rewardKey = string.IsNullOrEmpty(reward.RewardId) ? reward.name : reward.RewardId;
            if (!rewardStacks.TryGetValue(rewardKey, out RewardStack stack))
            {
                stack = new RewardStack(reward, 0);
                rewardStacks.Add(rewardKey, stack);
            }

            previousAmount = stack.Add(amount);
            return stack;
        }

        public void Clear()
        {
            rewardStacks.Clear();
        }
    }
}
