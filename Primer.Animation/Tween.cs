using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween(Action<float> lerp) : IDisposable
    {
        public static Tween empty = new(_ => {});
        public static Tween noop = empty with { duration = 0 };

        public IEasing easing { get; init; } = IEasing.defaultMethod;

        public float delay = 0f;

        #region public float duration;
        internal bool isCalculated { get; set; } = false;
        internal int ms { get; set; } = 500;

        public int milliseconds {
            get => ms;
            set {
                if (isCalculated) {
                    Debug.LogWarning("Forcing the duration of a calculated tween");
                    isCalculated = false;
                }

                ms = value;
            }
        }

        public float seconds {
            get => milliseconds / 1000f;
            set => milliseconds = (int)(value * 1000);
        }

        public float duration {
            get => milliseconds / 1000f;
            set => milliseconds = (int)(value * 1000);
        }
        #endregion

        public float totalDuration => duration + delay;
        internal float tStart => 1 / totalDuration * delay;

        public virtual void Evaluate(float t)
        {
            if (delay is not 0) {
                if (t < tStart)
                    return;

                t = Mathf.Clamp01(PrimerMath.Remap(tStart, 1, 0, 1, t));
            }

            lerp(easing.Evaluate(t));
        }

        #region public void Play();
        public async void PlayAndForget(CancellationToken ct = default) => await Play_Internal(ct);

        public  UniTask Play(CancellationToken ct) => Play_Internal(ct);

        private async UniTask Play_Internal(CancellationToken ct = default)
        {
            if (!Application.isPlaying) {
                Evaluate(1);
                return;
            }

            var startTime = Time.time;
            var delayAndDuration = totalDuration;

            if (TryEvaluate(0))
                return;

            while (!ct.IsCancellationRequested && Time.time < startTime + delayAndDuration) {
                var t = (Time.time - startTime) / delayAndDuration;

                if (TryEvaluate(t))
                    return;

                await UniTask.DelayFrame(1, cancellationToken: ct);

                if (ct.IsCancellationRequested)
                    return;
            }

            TryEvaluate(1);
        }

        private bool TryEvaluate(float t)
        {
            try {
                Evaluate(t);
            }
            catch (MissingReferenceException) {
                // The object we're tweening has been destroyed
                return true;
            }

            return false;
        }
        #endregion

        public virtual void Dispose() {}

        // This method makes the class awaitable
        public UniTask.Awaiter GetAwaiter()
        {
            return Play_Internal().GetAwaiter();
        }

        public static implicit operator Tween(Action<float> value)
        {
            return new Tween(value);
        }
    }
}
