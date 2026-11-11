using System;

namespace NightBufe.Core
{
    /// <summary>
    /// Maps real seconds to in-game time between 23:00 and the morning ezan.
    /// </summary>
    public class NightClock
    {
        public const int DefaultStartMinute = 23 * 60;
        public const int DefaultEndMinute = 6 * 60;

        public float RealDurationSeconds { get; }
        public int StartMinuteOfDay { get; }
        public int EndMinuteOfDay { get; }
        public float Progress { get; private set; }
        public bool IsFinished => Progress >= 1f;

        /// <summary>In-game minutes between start and end, crossing midnight if needed.</summary>
        public int TotalGameMinutes
        {
            get
            {
                int total = EndMinuteOfDay - StartMinuteOfDay;
                return total <= 0 ? total + 24 * 60 : total;
            }
        }

        public NightClock(float realDurationSeconds, float startProgress = 0f,
            int startMinuteOfDay = DefaultStartMinute, int endMinuteOfDay = DefaultEndMinute)
        {
            if (realDurationSeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(realDurationSeconds));
            RealDurationSeconds = realDurationSeconds;
            StartMinuteOfDay = startMinuteOfDay;
            EndMinuteOfDay = endMinuteOfDay;
            Progress = Clamp01(startProgress);
        }

        /// <summary>Advances the clock. Returns true on the frame the night finishes.</summary>
        public bool Advance(float realSeconds)
        {
            if (IsFinished || realSeconds <= 0f) return false;
            Progress = Clamp01(Progress + realSeconds / RealDurationSeconds);
            return IsFinished;
        }

        public int CurrentMinuteOfDay
        {
            get
            {
                int elapsed = (int)Math.Floor(Progress * TotalGameMinutes);
                return (StartMinuteOfDay + elapsed) % (24 * 60);
            }
        }

        /// <summary>"HH:mm" for the current in-game time.</summary>
        public string FormatTime() => FormatMinuteOfDay(CurrentMinuteOfDay);

        public static string FormatMinuteOfDay(int minuteOfDay)
        {
            minuteOfDay = ((minuteOfDay % 1440) + 1440) % 1440;
            return $"{minuteOfDay / 60:00}:{minuteOfDay % 60:00}";
        }

        /// <summary>Parses "HH:mm" (e.g. "02:30") into minutes since midnight.</summary>
        public static bool TryParseTime(string hhmm, out int minuteOfDay)
        {
            minuteOfDay = 0;
            if (string.IsNullOrEmpty(hhmm)) return false;
            var parts = hhmm.Split(':');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out int h) || !int.TryParse(parts[1], out int m)) return false;
            if (h < 0 || h > 23 || m < 0 || m > 59) return false;
            minuteOfDay = h * 60 + m;
            return true;
        }

        /// <summary>Progress for an "HH:mm" string; falls back to 0 if it can't be parsed.</summary>
        public float ProgressAt(string hhmm) => TryParseTime(hhmm, out int minute) ? ProgressAt(minute) : 0f;

        /// <summary>Progress (0..1) at which the given in-game time is reached.</summary>
        public float ProgressAt(int minuteOfDay)
        {
            int delta = minuteOfDay - StartMinuteOfDay;
            if (delta < 0) delta += 24 * 60;
            return Clamp01((float)delta / TotalGameMinutes);
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
