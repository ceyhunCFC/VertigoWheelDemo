using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class WheelSpinner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, HideInInspector] private RectTransform wheelRotator;
        [SerializeField, HideInInspector] private Button spinButton;
        [SerializeField, HideInInspector] private WheelView wheelView;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = 3f;
        [SerializeField] private int minFullTurns = 4;
        [SerializeField] private int maxFullTurns = 7;
        [SerializeField] private float topPointerAngleOffset;
        [SerializeField] private bool spinClockwise = true;

        private bool isSpinning;
        private bool isInputEnabled = true;

        public bool IsSpinning => isSpinning;

        public event Action SpinStarted;
        public event Action<WheelSpinResult> SpinCompleted;

        private void Awake()
        {
            BindButton();
        }

        private void OnDestroy()
        {
            if (wheelRotator != null)
            {
                wheelRotator.DOKill();
            }

            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(SpinRandom);
            }
        }

        private void OnValidate()
        {
            if (wheelRotator == null)
            {
                Transform wheelRoot = transform.Find("WheelRoot");
                if (wheelRoot != null)
                    wheelRotator = wheelRoot as RectTransform;
            }

            if (spinButton == null)
                spinButton = GetComponentInChildren<Button>(true);

            if (wheelView == null)
                wheelView = GetComponentInChildren<WheelView>(true);
        }

        public void SpinRandom()
        {
            if (!isInputEnabled || isSpinning || wheelRotator == null || spinButton == null || wheelView == null)
            {
                return;
            }

            int slotCount = wheelView.SlotCount;
            if (slotCount <= 0)
            {
                return;
            }

            wheelView.ShuffleSlots();
            int selectedSlotIndex = UnityEngine.Random.Range(0, slotCount);
            SpinToSlot(selectedSlotIndex);
        }

        public void SpinToSlot(int selectedSlotIndex)
        {
            if (!isInputEnabled || isSpinning || wheelRotator == null || spinButton == null || wheelView == null)
            {
                return;
            }

            int slotCount = wheelView.SlotCount;
            if (slotCount <= 0)
            {
                return;
            }

            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotCount - 1);

            if (!wheelView.TryGetSpinResult(selectedSlotIndex, out WheelSpinResult selectedResult))
            {
                return;
            }

            if (!wheelView.TryGetSlotAngleFromTop(selectedSlotIndex, out float selectedSlotAngle))
            {
                return;
            }

            selectedSlotAngle += topPointerAngleOffset;

            isSpinning = true;
            spinButton.interactable = false;
            SpinStarted?.Invoke();

            int fullTurns = UnityEngine.Random.Range(minFullTurns, maxFullTurns + 1);
            float finalRotation = CalculateTargetRotation(selectedSlotAngle, fullTurns);

            wheelRotator
                .DORotate(new Vector3(0f, 0f, finalRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    isSpinning = false;
                    spinButton.interactable = isInputEnabled;
                    SpinCompleted?.Invoke(selectedResult);
                });
        }

        public void SetInputEnabled(bool isEnabled)
        {
            isInputEnabled = isEnabled;
            if (spinButton != null)
            {
                spinButton.interactable = isInputEnabled && !isSpinning;
            }
        }

        public void ResetWheelRotation()
        {
            if (wheelRotator != null)
            {
                wheelRotator.DOKill();
                wheelRotator.localRotation = Quaternion.identity;
            }
        }

        private float CalculateTargetRotation(float slotAngleFromTop, int fullTurns)
        {
            float currentRotation = NormalizeAngle(wheelRotator.localEulerAngles.z);
            float targetRotation = NormalizeAngle(slotAngleFromTop);
            float fullTurnRotation = Mathf.Max(0, fullTurns) * 360f;

            if (spinClockwise)
            {
                float clockwiseDistance = Mathf.Repeat(currentRotation - targetRotation, 360f);
                if (clockwiseDistance <= Mathf.Epsilon)
                {
                    clockwiseDistance = 360f;
                }

                return currentRotation - fullTurnRotation - clockwiseDistance;
            }

            float counterClockwiseDistance = Mathf.Repeat(targetRotation - currentRotation, 360f);
            if (counterClockwiseDistance <= Mathf.Epsilon)
            {
                counterClockwiseDistance = 360f;
            }

            return currentRotation + fullTurnRotation + counterClockwiseDistance;
        }

        private float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle, 360f);
        }

        private void BindButton()
        {
            if (spinButton == null) return;
            spinButton.onClick.RemoveListener(SpinRandom);
            spinButton.onClick.AddListener(SpinRandom);
        }
    }
}
