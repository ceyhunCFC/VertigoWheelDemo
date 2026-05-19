using UnityEngine;
using VertigoWheel.UI;

namespace VertigoWheel.Gameplay
{
    public class ZoneDebugController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ZoneHudView zoneHudView;

        [Header("Zone Settings")]
        [SerializeField] private int currentZone = 1;
        [SerializeField] private int safeZoneInterval = 5;
        [SerializeField] private int superZoneInterval = 30;

        private ZoneService zoneService;

        private void Awake()
        {
            zoneService = new ZoneService(safeZoneInterval, superZoneInterval);
            Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentZone++;
                Refresh();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentZone = Mathf.Max(1, currentZone - 1);
                Refresh();
            }
        }

        private void OnValidate()
        {
            if (zoneHudView == null)
            {
                zoneHudView = GetComponent<ZoneHudView>();
            }

            currentZone = Mathf.Max(1, currentZone);
            safeZoneInterval = Mathf.Max(1, safeZoneInterval);
            superZoneInterval = Mathf.Max(1, superZoneInterval);
        }

        private void Refresh()
        {
            if (zoneHudView == null)
            {
                return;
            }

            zoneHudView.SetCurrentZone(currentZone);
            zoneHudView.SetNextSafeZone(zoneService.GetNextSafeZone(currentZone));
            zoneHudView.SetNextSuperZone(zoneService.GetNextSuperZone(currentZone));
        }
    }
}