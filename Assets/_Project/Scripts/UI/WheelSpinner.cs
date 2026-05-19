using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoWheel.UI
{
    public class WheelSpinner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform wheelRotator;
        [SerializeField] private Button spinButton;

        [Header("Spin Settings")]
        [SerializeField] private int slotCount = 8;
        [SerializeField] private float spinDuration = 3f;
        [SerializeField] private int minFullTurns = 4;
        [SerializeField] private int maxFullTurns = 7;

        private bool isSpinning;

        public bool IsSpinning => isSpinning;

        public event Action<int> SpinCompleted;

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
                {
                    wheelRotator = wheelRoot as RectTransform;
                }
            }

            if (spinButton == null)
            {
                spinButton = GetComponentInChildren<Button>(true);
            }
        }

        public void SpinRandom()
        {
            if (isSpinning)
            {
                return;
            }

            int selectedSlotIndex = UnityEngine.Random.Range(0, slotCount);
            SpinToSlot(selectedSlotIndex);
        }

        public void SpinToSlot(int selectedSlotIndex)
        {
            if (isSpinning || wheelRotator == null || spinButton == null)
            {
                return;
            }

            isSpinning = true;
            spinButton.interactable = false;

            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotCount - 1);

            float anglePerSlot = 360f / slotCount;
            float selectedSlotAngle = selectedSlotIndex * anglePerSlot;
            int fullTurns = UnityEngine.Random.Range(minFullTurns, maxFullTurns + 1);

            float targetRotation = (fullTurns * 360f) + selectedSlotAngle;

            wheelRotator
                .DORotate(new Vector3(0f, 0f, -targetRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    isSpinning = false;
                    spinButton.interactable = true;
                    SpinCompleted?.Invoke(selectedSlotIndex);
                });
        }

        private void BindButton()
        {
            if (spinButton == null)
            {
                return;
            }

            spinButton.onClick.RemoveListener(SpinRandom);
            spinButton.onClick.AddListener(SpinRandom);
        }
    }
}