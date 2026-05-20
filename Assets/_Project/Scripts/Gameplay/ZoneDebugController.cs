using DG.Tweening;
using UnityEngine;
using VertigoWheel.UI;

namespace VertigoWheel.Gameplay
{
    public class ZoneDebugController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, HideInInspector] private ZoneHudView zoneHudView;
        [SerializeField, HideInInspector] private WheelView wheelView;
        [SerializeField, HideInInspector] private WheelSkinView wheelSkinView;
        [SerializeField, HideInInspector] private ExitButtonView exitButtonView;
        [SerializeField, HideInInspector] private ExitConfirmPanelView exitConfirmPanelView;
        [SerializeField, HideInInspector] private RewardPanelView rewardPanelView;
        [SerializeField, HideInInspector] private CollectRewardsPanelView collectRewardsPanelView;
        [SerializeField, HideInInspector] private GameOverPanelView gameOverPanelView;
        [SerializeField, HideInInspector] private WheelSpinner wheelSpinner;
        [SerializeField, HideInInspector] private ZoneBarView zoneBarView;
        [SerializeField, HideInInspector] private RectTransform wheelContainer;

        [Header("Zone Settings")]
        [SerializeField] private ZoneConfigSO zoneConfig;
        [SerializeField] private int currentZone = 1;

        [Header("Wheel Transition")]
        [SerializeField, Min(0f)] private float wheelTransitionDistance = 900f;
        [SerializeField, Min(0f)] private float wheelExitDuration = 0.28f;
        [SerializeField, Min(0f)] private float wheelEnterDuration = 0.32f;

        private ZoneService zoneService;
        private readonly RewardInventory rewardInventory = new RewardInventory();
        private Sequence wheelTransitionSequence;
        private bool isWheelTransitioning;
        private bool hasWheelHomePosition;
        private Vector2 wheelHomePosition;

        private void Awake()
        {
            AutoWire();
            zoneService = CreateZoneService();

            if (wheelSpinner != null)
            {
                wheelSpinner.SpinStarted += Refresh;
                wheelSpinner.SpinCompleted += OnSpinCompleted;
            }

            if (gameOverPanelView != null)
            {
                gameOverPanelView.GiveUpClicked += RestartGame;
                gameOverPanelView.ReviveClicked += ContinueAfterRevive;
            }

            if (exitButtonView != null)
            {
                exitButtonView.Clicked += ShowExitConfirmPanel;
            }

            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.CollectClicked += CollectRewardsAndRestart;
                exitConfirmPanelView.GoBackClicked += HideExitConfirmPanel;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (wheelSpinner != null)
            {
                wheelSpinner.SpinStarted -= Refresh;
                wheelSpinner.SpinCompleted -= OnSpinCompleted;
            }

            if (gameOverPanelView != null)
            {
                gameOverPanelView.GiveUpClicked -= RestartGame;
                gameOverPanelView.ReviveClicked -= ContinueAfterRevive;
            }

            if (exitButtonView != null)
            {
                exitButtonView.Clicked -= ShowExitConfirmPanel;
            }

            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.CollectClicked -= CollectRewardsAndRestart;
                exitConfirmPanelView.GoBackClicked -= HideExitConfirmPanel;
            }

            wheelTransitionSequence?.Kill();
        }

        private void OnValidate()
        {
            AutoWire();
            currentZone = Mathf.Max(1, currentZone);
        }

        private void AutoWire()
        {
            if (zoneHudView == null)
            {
                zoneHudView = GetComponent<ZoneHudView>();
            }

            if (wheelSkinView == null)
            {
                wheelSkinView = GetComponentInChildren<WheelSkinView>(true);
            }

            if (wheelView == null)
            {
                wheelView = GetComponentInChildren<WheelView>(true);
            }

            if (wheelContainer == null && wheelView != null)
            {
                wheelContainer = wheelView.transform as RectTransform;
            }

            CacheWheelHomePosition();

            if (exitButtonView == null)
            {
                exitButtonView = GetComponentInChildren<ExitButtonView>(true);
            }

            if (exitConfirmPanelView == null)
            {
                exitConfirmPanelView = GetComponentInChildren<ExitConfirmPanelView>(true);
            }

            if (exitConfirmPanelView == null)
            {
                exitConfirmPanelView = FindObjectOfType<ExitConfirmPanelView>(true);
            }

            if (rewardPanelView == null)
            {
                rewardPanelView = GetComponentInChildren<RewardPanelView>(true);
            }

            if (collectRewardsPanelView == null)
            {
                collectRewardsPanelView = GetComponentInChildren<CollectRewardsPanelView>(true);
            }

            if (collectRewardsPanelView == null)
            {
                collectRewardsPanelView = FindObjectOfType<CollectRewardsPanelView>(true);
            }

            if (gameOverPanelView == null)
            {
                gameOverPanelView = GetComponentInChildren<GameOverPanelView>(true);
            }

            if (gameOverPanelView == null)
            {
                gameOverPanelView = FindObjectOfType<GameOverPanelView>(true);
            }

            if (wheelSpinner == null)
            {
                wheelSpinner = GetComponentInChildren<WheelSpinner>(true);
            }

            if (zoneBarView == null)
            {
                zoneBarView = GetComponentInChildren<ZoneBarView>(true);
            }
        }

        private void Refresh()
        {
            if (zoneService == null)
            {
                zoneService = CreateZoneService();
            }

            ZoneType zoneType = zoneService.GetZoneType(currentZone);

            ApplyHud();
            ApplyWheelZone(zoneType);
            ApplyExitButton(zoneType);
        }

        private void ApplyHud()
        {
            if (zoneHudView == null)
            {
                return;
            }

            zoneHudView.SetNextSafeZone(zoneService.GetNextSafeZone(currentZone));
            zoneHudView.SetNextSuperZone(zoneService.GetNextSuperZone(currentZone));
        }

        private void ApplyExitButton(ZoneType zoneType)
        {
            if (exitButtonView != null)
            {
                bool isSafeExitZone = zoneType == ZoneType.Safe || zoneType == ZoneType.Super;
                bool isWheelIdle = (wheelSpinner == null || !wheelSpinner.IsSpinning) && !isWheelTransitioning;
                exitButtonView.SetInteractable(isSafeExitZone && isWheelIdle);
            }
        }

        private void OnSpinCompleted(WheelSpinResult spinResult)
        {
            if (wheelSpinner != null)
            {
                wheelSpinner.SetInputEnabled(false);
            }

            WheelSlotData slotData = spinResult.SlotData;
            if (IsDeathReward(slotData))
            {
                Debug.Log($"[ZoneDebugController] Death selected: {slotData.Reward.DisplayName}. Game over.");
                if (exitButtonView != null)
                {
                    exitButtonView.SetVisible(false);
                }

                if (gameOverPanelView == null)
                {
                    AutoWire();
                }

                if (gameOverPanelView != null)
                {
                    gameOverPanelView.Show();
                }
                else
                {
                    Debug.LogError("[ZoneDebugController] GameOverPanelView not found in scene.", this);
                }

                return;
            }

            if (slotData != null && slotData.Reward != null)
            {
                RewardStack stack = rewardInventory.AddReward(slotData.Reward, slotData.Amount, out int previousAmount);
                if (rewardPanelView != null)
                {
                    rewardPanelView.ShowReward(stack, previousAmount, spinResult.SourceIconTransform, AdvanceToNextZone);
                    return;
                }
            }

            AdvanceToNextZone();
        }

        private void AdvanceToNextZone()
        {
            currentZone++;

            if (zoneBarView != null)
            {
                zoneBarView.Advance();
            }

            PlayWheelZoneTransition();
        }

        private ZoneService CreateZoneService()
        {
            return new ZoneService(zoneConfig);
        }

        private bool IsDeathReward(WheelSlotData slotData)
        {
            if (slotData == null || slotData.Reward == null)
            {
                return false;
            }

            if (slotData.Reward.RewardType == RewardType.Death)
            {
                return true;
            }

            string rewardName = $"{slotData.Reward.RewardId} {slotData.Reward.DisplayName} {slotData.Reward.name}".ToLowerInvariant();
            return rewardName.Contains("death") || rewardName.Contains("bomb");
        }

        private void RestartGame()
        {
            ResetWheelTransitionState();
            rewardInventory.Clear();

            if (rewardPanelView != null)
            {
                rewardPanelView.Clear();
            }

            if (gameOverPanelView != null)
            {
                gameOverPanelView.Hide();
            }

            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.Hide();
            }

            if (collectRewardsPanelView != null)
            {
                collectRewardsPanelView.Hide();
            }

            if (exitButtonView != null)
            {
                exitButtonView.SetVisible(true);
            }

            if (wheelSpinner != null)
            {
                wheelSpinner.ResetWheelRotation();
                wheelSpinner.SetInputEnabled(true);
            }

            currentZone = 1;
            ApplyWheelZoneReset(ZoneType.Normal);

            if (zoneBarView != null)
            {
                zoneBarView.SetStartZone(currentZone);
            }

            Refresh();
        }

        private void ApplyWheelZone(ZoneType zoneType)
        {
            if (wheelView != null)
            {
                wheelView.SetZoneType(zoneType);
            }
            else if (wheelSkinView != null)
            {
                wheelSkinView.SetZoneType(zoneType);
            }
        }

        private void ApplyWheelZoneReset(ZoneType zoneType)
        {
            if (wheelView != null)
            {
                wheelView.ResetZoneType(zoneType);
            }
            else if (wheelSkinView != null)
            {
                wheelSkinView.SetZoneType(zoneType);
            }
        }

        private void PlayWheelZoneTransition()
        {
            if (zoneService == null)
            {
                zoneService = CreateZoneService();
            }

            ZoneType nextZoneType = zoneService.GetZoneType(currentZone);

            if (wheelContainer == null)
            {
                AutoWire();
            }

            if (wheelContainer == null)
            {
                Refresh();
                return;
            }

            CacheWheelHomePosition();
            wheelTransitionSequence?.Kill();
            isWheelTransitioning = true;
            if (wheelSpinner != null)
            {
                wheelSpinner.SetInputEnabled(false);
            }

            ApplyHud();
            ApplyExitButton(nextZoneType);

            Vector2 originalPosition = hasWheelHomePosition ? wheelHomePosition : wheelContainer.anchoredPosition;
            Vector2 exitPosition = originalPosition + Vector2.down * wheelTransitionDistance;
            Vector2 enterPosition = originalPosition + Vector2.down * wheelTransitionDistance;

            wheelTransitionSequence = DOTween.Sequence();
            wheelTransitionSequence.Append(wheelContainer.DOAnchorPos(exitPosition, wheelExitDuration).SetEase(Ease.InCubic));
            wheelTransitionSequence.AppendCallback(() =>
            {
                ApplyWheelZone(nextZoneType);

                if (wheelSpinner != null)
                {
                    wheelSpinner.ResetWheelRotation();
                }

                wheelContainer.anchoredPosition = enterPosition;
            });
            wheelTransitionSequence.Append(wheelContainer.DOAnchorPos(originalPosition, wheelEnterDuration).SetEase(Ease.OutCubic));
            wheelTransitionSequence.OnComplete(() =>
            {
                isWheelTransitioning = false;
                if (wheelSpinner != null)
                {
                    wheelSpinner.SetInputEnabled(true);
                }

                Refresh();
            });
        }

        private void CacheWheelHomePosition()
        {
            if (hasWheelHomePosition || wheelContainer == null)
            {
                return;
            }

            wheelHomePosition = wheelContainer.anchoredPosition;
            hasWheelHomePosition = true;
        }

        private void ResetWheelTransitionState()
        {
            wheelTransitionSequence?.Kill();
            wheelTransitionSequence = null;
            isWheelTransitioning = false;

            if (wheelContainer == null)
            {
                AutoWire();
            }

            if (wheelContainer != null)
            {
                CacheWheelHomePosition();
                wheelContainer.DOKill();
                wheelContainer.anchoredPosition = hasWheelHomePosition ? wheelHomePosition : Vector2.zero;
            }
        }

        private void ShowExitConfirmPanel()
        {
            if (wheelSpinner != null && wheelSpinner.IsSpinning)
            {
                return;
            }

            ZoneType zoneType = zoneService != null ? zoneService.GetZoneType(currentZone) : ZoneType.Normal;
            if (zoneType != ZoneType.Safe && zoneType != ZoneType.Super)
            {
                return;
            }

            if (exitConfirmPanelView == null)
            {
                AutoWire();
            }

            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.Show();
            }
        }

        private void HideExitConfirmPanel()
        {
            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.Hide();
            }

            Refresh();
        }

        private void CollectRewardsAndRestart()
        {
            if (exitConfirmPanelView != null)
            {
                exitConfirmPanelView.Hide();
            }

            if (collectRewardsPanelView == null)
            {
                AutoWire();
            }

            var rewardStacks = rewardInventory.Stacks;
            if (collectRewardsPanelView != null && rewardStacks.Count > 0)
            {
                collectRewardsPanelView.Show(rewardStacks, RestartGame);
                return;
            }

            RestartGame();
        }

        private void ContinueAfterRevive()
        {
            if (gameOverPanelView != null)
            {
                gameOverPanelView.Hide();
            }

            if (exitButtonView != null)
            {
                exitButtonView.SetVisible(true);
            }

            if (wheelSpinner != null)
            {
                wheelSpinner.SetInputEnabled(true);
            }

            Refresh();
        }
    }
}
