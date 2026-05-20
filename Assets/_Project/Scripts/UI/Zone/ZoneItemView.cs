using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class ZoneItemView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, HideInInspector] private Image backgroundImage;
        [SerializeField, HideInInspector] private TMP_Text zoneValueText;

        private void OnValidate()
        {
            AutoWire();
            DisableUnnecessaryRaycasts();
        }

        public void SetData(int zoneNumber, ZoneVisualDataSO visualData)
        {
            AutoWire();

            if (zoneValueText != null)
            {
                zoneValueText.text = zoneNumber.ToString();
                zoneValueText.color = visualData != null ? visualData.TextColor : Color.white;
            }

            if (backgroundImage != null)
            {
                backgroundImage.sprite = visualData != null ? visualData.ZoneBarSprite : null;
            }
        }

        private void AutoWire()
        {
            if (backgroundImage == null)
            {
                backgroundImage = FindImage("ui_image_zone_item_bg");
            }

            if (zoneValueText == null)
            {
                zoneValueText = FindText("ui_text_zone_item_value");
            }
        }

        private void DisableUnnecessaryRaycasts()
        {
            if (backgroundImage != null)
            {
                backgroundImage.raycastTarget = false;
            }

            if (zoneValueText != null)
            {
                zoneValueText.raycastTarget = false;
            }
        }

        private Image FindImage(string objectName)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image.name == objectName)
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
                if (text.name == objectName)
                {
                    return text;
                }
            }

            return null;
        }
    }
}
