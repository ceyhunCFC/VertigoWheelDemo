using UnityEngine;
using VertigoWheel.UI;

namespace VertigoWheel.Gameplay
{
    public class ZoneDebugController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ZoneHudView zoneHudView;
        [SerializeField] private WheelSkinView wheelSkinView;
        [SerializeField] private ExitButtonView exitButtonView;
        [SerializeField] private WheelSpinner wheelSpinner;

        [Header("Zone Settings")]
        [SerializeField] private int currentZone = 1;
        [SerializeField] private int safeZoneInterval = 5;
        [SerializeField] private int superZoneInterval = 30;


        private ZoneService zoneService;

        private void Awake()
        {
            zoneService = new ZoneService(safeZoneInterval, superZoneInterval);

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

            if (wheelSkinView == null)
            {
                wheelSkinView = GetComponentInChildren<WheelSkinView>(true);
            }

            if (exitButtonView == null)
            {
                exitButtonView = GetComponent<ExitButtonView>();
            }

            if (wheelSpinner == null)
            {
                wheelSpinner = GetComponentInChildren<WheelSpinner>(true);
            }

            currentZone = Mathf.Max(1, currentZone);
            safeZoneInterval = Mathf.Max(1, safeZoneInterval);
            superZoneInterval = Mathf.Max(1, superZoneInterval);
        }

        private void Refresh()
        {
            if (zoneService == null)
            {
                zoneService = new ZoneService(safeZoneInterval, superZoneInterval);
            }

            ZoneType zoneType = zoneService.GetZoneType(currentZone);

            if (zoneHudView != null)
            {
                zoneHudView.SetCurrentZone(currentZone);
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
            Refresh();
        }
    }
}