using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class WheelSlotView : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Image rewardIconImage;
        [SerializeField, HideInInspector] private TMP_Text rewardAmountText;

        public RectTransform RewardIconTransform => rewardIconImage != null ? rewardIconImage.rectTransform : null;

        private void OnValidate()
        {
            AutoWire();
        }

        public void SetData(WheelSlotData slotData)
        {
            if (slotData == null || slotData.Reward == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (rewardIconImage != null)
            {
                rewardIconImage.sprite = slotData.Reward.Icon;
                rewardIconImage.preserveAspect = true;
                rewardIconImage.raycastTarget = false;
            }

            if (rewardAmountText != null)
            {
                ConfigureAmountText(rewardAmountText);
                rewardAmountText.enabled = slotData.Reward.RewardType != RewardType.Death;
                rewardAmountText.text = $"x{slotData.Amount}";
            }
        }

        private void ConfigureAmountText(TMP_Text amountText)
        {
            amountText.enableWordWrapping = false;
            amountText.overflowMode = TextOverflowModes.Overflow;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMin = 14f;
        }

        private void SetVisible(bool isVisible)
        {
            if (rewardIconImage != null)
            {
                rewardIconImage.enabled = isVisible;
            }

            if (rewardAmountText != null)
            {
                rewardAmountText.enabled = isVisible;
            }
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
