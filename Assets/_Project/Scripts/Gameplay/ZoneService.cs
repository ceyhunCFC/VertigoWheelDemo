using UnityEngine;

namespace VertigoWheel.Gameplay
{
    public class ZoneService
    {
        private readonly int safeZoneInterval;
        private readonly int superZoneInterval;

        public ZoneService(int safeZoneInterval, int superZoneInterval)
        {
            this.safeZoneInterval = Mathf.Max(1, safeZoneInterval);
            this.superZoneInterval = Mathf.Max(1, superZoneInterval);
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone > 0 && zone % superZoneInterval == 0)
            {
                return ZoneType.Super;
            }

            if (zone > 0 && zone % safeZoneInterval == 0)
            {
                return ZoneType.Safe;
            }

            return ZoneType.Normal;
        }

        public int GetNextSafeZone(int currentZone)
        {
            int nextSafeZone = GetNextMilestone(currentZone, safeZoneInterval);

            while (nextSafeZone % superZoneInterval == 0)
            {
                nextSafeZone += safeZoneInterval;
            }

            return nextSafeZone;
        }

        public int GetNextSuperZone(int currentZone)
        {
            return GetNextMilestone(currentZone, superZoneInterval);
        }

        private int GetNextMilestone(int currentZone, int interval)
        {
            int zone = Mathf.Max(0, currentZone);
            return ((zone / interval) + 1) * interval;
        }
    }
}