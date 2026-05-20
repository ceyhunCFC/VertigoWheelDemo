using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField] private Image rewardIconImage;
        [SerializeField] private TMP_Text rewardAmountText;

        public RectTransform IconTransform => rewardIconImage != null ? rewardIconImage.rectTransform : transform as RectTransform;

        private void OnValidate()
        {
            AutoWire();
        }

        public void SetData(RewardDataSO reward, int amount)
        {
            gameObject.SetActive(true);
            AutoWire();

            if (rewardIconImage != null)
            {
                rewardIconImage.sprite = reward != null ? reward.Icon : null;
                rewardIconImage.preserveAspect = true;
                rewardIconImage.raycastTarget = false;
            }

            SetAmount(amount);
        }

        public void AnimateAmount(int fromAmount, int toAmount)
        {
            AutoWire();
            transform.DOKill();

            if (rewardIconImage != null)
            {
                rewardIconImage.transform.DOKill();
                rewardIconImage.transform.DOPunchScale(Vector3.one * 0.18f, 0.25f, 6, 0.6f);
            }

            if (rewardAmountText == null)
            {
                return;
            }

            int displayAmount = fromAmount;
            DOTween.To(() => displayAmount, value =>
                {
                    displayAmount = value;
                    SetAmount(displayAmount);
                }, toAmount, 0.45f)
                .SetEase(Ease.OutCubic)
                .SetTarget(this);
        }

        private void SetAmount(int amount)
        {
            if (rewardAmountText != null)
            {
                ConfigureAmountText(rewardAmountText);
                rewardAmountText.text = $"x{amount}";
            }
        }

        private void ConfigureAmountText(TMP_Text amountText)
        {
            amountText.enableWordWrapping = false;
            amountText.overflowMode = TextOverflowModes.Overflow;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = 18f;
            amountText.fontSizeMax = 36f;
        }

        private void AutoWire()
        {
            if (rewardIconImage == null)
            {
                rewardIconImage = FindImage("ui_image_reward_icon");
            }

            if (rewardAmountText == null)
            {
                rewardAmountText = FindText("ui_text_reward_amount_value");
            }
        }

        private Image FindImage(string objectName)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image.name.Trim() == objectName)
                {
                    return image;
                }
            }

            return null;
        }

        private TMP_Text FindText(string objectName)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text.name.Trim() == objectName)
                {
                    return text;
                }
            }

            return null;
        }
    }
}
