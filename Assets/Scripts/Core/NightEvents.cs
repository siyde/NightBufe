using System.Collections.Generic;

namespace NightBufe.Core
{
    /// <summary>
    /// The radio whisper: fires once if the radio is on at any moment inside
    /// [triggerProgress, triggerProgress + window].
    /// </summary>
    public class RadioWhisperTracker
    {
        public float TriggerProgress { get; }
        public float Window { get; }
        public bool HasFired { get; private set; }

        public RadioWhisperTracker(float triggerProgress, float window)
        {
            TriggerProgress = triggerProgress;
            Window = window;
        }

        /// <summary>Returns true exactly once, on the frame the whisper should be heard.</summary>
        public bool Update(bool radioOn, float progress)
        {
            if (HasFired || !radioOn) return false;
            if (progress < TriggerProgress || progress > TriggerProgress + Window) return false;
            HasFired = true;
            return true;
        }

        public bool IsMissed(float progress) => !HasFired && progress > TriggerProgress + Window;
    }

    /// <summary>
    /// Customers arrive in order. Each one appears when its arrival progress
    /// is reached and the counter is free.
    /// </summary>
    public class CustomerQueue
    {
        private readonly IReadOnlyList<float> _arrivals;

        public int NextIndex { get; private set; }
        public int Count => _arrivals.Count;
        public bool IsDone => NextIndex >= _arrivals.Count;

        public CustomerQueue(IReadOnlyList<float> arrivals)
        {
            _arrivals = arrivals;
        }

        /// <summary>Returns the index of the next customer to show, or -1.</summary>
        public int TryDequeue(float progress, bool counterBusy)
        {
            if (counterBusy || IsDone) return -1;
            if (progress < _arrivals[NextIndex]) return -1;
            return NextIndex++;
        }
    }

    /// <summary>
    /// Tension 0..1 drives the lighting (warm yellow → sickly green, flicker)
    /// and post-processing weight. Base comes from the night; spikes decay.
    /// </summary>
    public class TensionModel
    {
        public float BaseTension { get; set; }
        public float Spike { get; private set; }
        public float DecayPerSecond { get; set; } = 0.25f;

        public float Value
        {
            get
            {
                float v = BaseTension + Spike;
                return v < 0f ? 0f : (v > 1f ? 1f : v);
            }
        }

        public TensionModel(float baseTension = 0f)
        {
            BaseTension = baseTension;
        }

        public void AddSpike(float amount)
        {
            Spike += amount;
            if (Spike > 1f) Spike = 1f;
        }

        public void Tick(float deltaSeconds)
        {
            if (Spike <= 0f) return;
            Spike -= DecayPerSecond * deltaSeconds;
            if (Spike < 0f) Spike = 0f;
        }
    }
}
