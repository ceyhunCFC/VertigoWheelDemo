using UnityEngine;
using VertigoWheel.UI;

namespace VertigoWheel.Gameplay
{
    public class ZoneDebugController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, HideInInspector] private ZoneHudView zoneHudView;
        [SerializeField, HideInInspector] private WheelSkinView wheelSkinView;
        [SerializeField, HideInInspector] private ExitButtonView exitButtonView;
        [SerializeField, HideInInspector] private WheelSpinner wheelSpinner;
        [SerializeField, HideInInspector] private ZoneBarView zoneBarView;

        [Header("Zone Settings")]
        [SerializeField] private ZoneConfigSO zoneConfig;
        [SerializeField] private int currentZone = 1;

        private ZoneService zoneService;

        private void Awake()
        {
            AutoWire();
            zoneService = CreateZoneService();

            if (wheelSpinner != null)
            {
                wheelSpinner.SpinStarted += Refresh;
                wheelSpinner.SpinCompleted += OnSpinCompleted;
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

            if (exitButtonView == null)
            {
                exitButtonView = GetComponentInChildren<ExitButtonView>(true);
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

            if (zoneHudView != null)
            {
                zoneHudView.SetNextSafeZone(zoneService.GetNextSafeZone(currentZone));
                zoneHudView.SetNextSuperZone(zoneService.GetNextSuperZone(currentZone));
            }

            if (wheelSkinView != null)
            {
                wheelSkinView.SetZoneType(zoneType);
            }

            if (exitButtonView != null)
            {
                bool isSafeExitZone = zoneType == ZoneType.Safe || zoneType == ZoneType.Super;
                bool isWheelIdle = wheelSpinner == null || !wheelSpinner.IsSpinning;
                exitButtonView.SetInteractable(isSafeExitZone && isWheelIdle);
            }
        }

        private void OnSpinCompleted(int selectedSlotIndex)
        {
            currentZone++;

            if (zoneBarView != null)
            {
                zoneBarView.Advance();
            }

            Refresh();
        }

        private ZoneService CreateZoneService()
        {
            return new ZoneService(zoneConfig);
        }
    }
}
