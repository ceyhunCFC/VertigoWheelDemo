using TMPro;
using UnityEngine;

namespace VertigoWheel.UI
{
    public class ZoneHudView : MonoBehaviour
    {
        [Header("Zone Bar")]
        [SerializeField, HideInInspector] private TMP_Text currentZoneText;

        [Header("Counters")]
        [SerializeField, HideInInspector] private TMP_Text safeZoneCounterText;
        [SerializeField, HideInInspector] private TMP_Text superZoneCounterText;

        private void OnValidate()
        {
            AutoWire();
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            currentZoneText = FindText("ui_text_zone_current_value");
            safeZoneCounterText = FindText("ui_text_safe_zone_value");
            superZoneCounterText = FindText("ui_text_super_zone_value");
        }

        public void SetCurrentZone(int zone)
        {
            if (currentZoneText != null)
            {
                currentZoneText.text = zone.ToString();
            }
        }

        public void SetNextSafeZone(int zone)
        {
            if (safeZoneCounterText != null)
            {
                safeZoneCounterText.text = zone.ToString();
            }
        }

        public void SetNextSuperZone(int zone)
        {
            if (superZoneCounterText != null)
            {
                superZoneCounterText.text = zone.ToString();
            }
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

            Debug.LogWarning($"TMP text not found: {objectName}", this);
            return null;
        }
    }
}
