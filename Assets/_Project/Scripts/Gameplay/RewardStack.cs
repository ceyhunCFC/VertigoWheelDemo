namespace VertigoWheel.Gameplay
{
    public class RewardStack
    {
        public RewardStack(RewardDataSO reward, int amount)
        {
            Reward = reward;
            Amount = amount;
        }

        public RewardDataSO Reward { get; }
        public int Amount { get; private set; }

        public int Add(int amount)
        {
            int previousAmount = Amount;
            Amount += amount;
            return previousAmount;
        }
    }
}
