using System;

using UnityEngine;

namespace MoshitinEncoded
{
    /// <summary>
    /// A timer for timed events.
    /// </summary>
    [Serializable]
    public class Timer
    {
        [Tooltip("Default duration in seconds.")]
        [SerializeField] private float _Duration = 0f;

        [Tooltip("Whether it works with the time scale or not.")]
        [SerializeField] private bool _IsScaled = true;

        private float _StartTime = float.MinValue;
        private float _PauseStartTime = float.MinValue;
        private float _PausedTime = 0;
        private bool _IsPaused = false;
        private float _DurationInUse = 0f;

        /// <summary>
        /// Default duration in seconds.
        /// </summary>
        public float Duration { get => _Duration; set => _Duration = Mathf.Abs(value); }

        /// <summary>
        /// Duration being used in seconds.
        /// </summary>
        public float DurationInUse => _DurationInUse;

        /// <summary>
        /// Whether the timer has been paused or not.
        /// </summary>
        public bool IsPaused => _IsPaused;
        
        /// <summary>
        /// Whether the timer has over or not yet.
        /// </summary>
        public bool IsOver => ElapsedTime >= _DurationInUse;

        /// <summary>
        /// Elapsed time in seconds.
        /// </summary>
        public float ElapsedTime => Mathf.Clamp(GetCurrentTime() - _StartTime - _PausedTime, 0, _DurationInUse);

        /// <summary>
        /// Elapsed time from zero to one.
        /// </summary>
        public float ElapsedTime01 => Mathf.InverseLerp(0, _DurationInUse, ElapsedTime);

        /// <summary>
        /// Remaining time in seconds.
        /// </summary>
        public float RemainingTime => _DurationInUse - ElapsedTime;

        /// <summary>
        /// Remaining time from one to zero.
        /// </summary>
        public float RemainingTime01 => 1 - ElapsedTime01;

        public Timer() { }

        public Timer(bool isScaled)
        {
            _IsScaled = isScaled;
        }

        /// <summary>
        /// Play the timer with the default duration.
        /// </summary>
        public void Play()
        {
            Play(_Duration);
        }

        /// <summary>
        /// Play the timer with a custom duration.
        /// </summary>
        /// <param name="duration"> Duration of the timer in seconds. </param>
        public void Play(float duration)
        {
            _StartTime = GetCurrentTime();
            _DurationInUse = Mathf.Abs(duration);
            _PausedTime = 0;
        }

        public void Pause()
        {
            _IsPaused = true;
            _PauseStartTime = GetCurrentTime();
        }

        public void Unpause()
        {
            if (!_IsPaused)
            {
                return;
            }

            _IsPaused = false;
            _PausedTime += GetCurrentTime() - _PauseStartTime;
        }

        private float GetCurrentTime() => _IsScaled ? Time.time : Time.unscaledTime;
    }
}
