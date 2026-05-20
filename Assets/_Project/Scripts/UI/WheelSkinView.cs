using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Gameplay;

namespace VertigoWheel.UI
{
    public class WheelSkinView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, HideInInspector] private Image wheelBaseImage;

        [Header("Wheel Sprites")]
        [SerializeField] private Sprite bronzeWheelSprite;
        [SerializeField] private Sprite silverWheelSprite;
        [SerializeField] private Sprite goldenWheelSprite;

        private void OnValidate()
        {
            if (wheelBaseImage == null)
            {
                wheelBaseImage = FindImage("ui_image_spin_base");
            }
        }

        public void SetZoneType(ZoneType zoneType)
        {
            if (wheelBaseImage == null)
            {
                return;
            }

            wheelBaseImage.sprite = GetWheelSprite(zoneType);
            wheelBaseImage.preserveAspect = true;
            wheelBaseImage.raycastTarget = false;
        }

        private Sprite GetWheelSprite(ZoneType zoneType)
        {
            switch (zoneType)
            {
                case ZoneType.Safe:
                    return silverWheelSprite;

                case ZoneType.Super:
                    return goldenWheelSprite;

                default:
                    return bronzeWheelSprite;
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

            Debug.LogWarning($"Image not found: {objectName}", this);
            return null;
        }
    }
}
