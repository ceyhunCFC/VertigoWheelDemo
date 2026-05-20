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

        private bool isSpinning;

        public bool IsSpinning => isSpinning;

        public event Action SpinStarted;
        public event Action<WheelSlotData> SpinCompleted;

        private void Awake()
        {
            BindButton();
        }

        private void OnDestroy()
        {
            if (wheelRotator != null)
                wheelRotator.DOKill();

            if (spinButton != null)
                spinButton.onClick.RemoveListener(SpinRandom);
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
            if (isSpinning || wheelView == null) return;

            int slotCount = wheelView.SlotCount;
            if (slotCount <= 0) return;

            wheelView.ShuffleSlots();
            int selectedSlotIndex = UnityEngine.Random.Range(0, slotCount);
            SpinToSlot(selectedSlotIndex);
        }

        public void SpinToSlot(int selectedSlotIndex)
        {
            if (isSpinning || wheelRotator == null || spinButton == null || wheelView == null) return;

            int slotCount = wheelView.SlotCount;
            if (slotCount <= 0) return;

            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slotCount - 1);

            isSpinning = true;
            spinButton.interactable = false;
            SpinStarted?.Invoke();

            float anglePerSlot = 360f / slotCount;
            float selectedSlotAngle = (selectedSlotIndex * anglePerSlot) + topPointerAngleOffset;
            int fullTurns = UnityEngine.Random.Range(minFullTurns, maxFullTurns + 1);
            float targetRotation = (fullTurns * 360f) + selectedSlotAngle;

            wheelRotator
                .DORotate(new Vector3(0f, 0f, -targetRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    isSpinning = false;
                    spinButton.interactable = true;

                    wheelView.TryGetSlotData(selectedSlotIndex, out WheelSlotData slotData);
                    SpinCompleted?.Invoke(slotData);
                });
        }

        private void BindButton()
        {
            if (spinButton == null) return;
            spinButton.onClick.RemoveListener(SpinRandom);
            spinButton.onClick.AddListener(SpinRandom);
        }
    }
}
