using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    // These two classes are identical, except one extends MonoBehaviour and the other doesn't

    public class Async
    {
        protected static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        protected static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        protected static async UniTask Parallel(params UniTask[] processes)
        {
            await UniTask.WhenAll(processes);
        }

        public static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Parallel(tweenList);
        }

        public static Tween Series(params Tween[] tweenList)
        {
            return Tween.Series(tweenList);
        }
    }

    public class AsyncMonoBehaviour : MonoBehaviour
    {
        protected static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        protected static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        public static async UniTask Parallel(params UniTask[] processes)
        {
            await UniTask.WhenAll(processes);
        }

        public static async UniTask Series(params Func<UniTask>[] processes)
        {
            foreach (var process in processes)
            {
                await process();
            }
        }

        public static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Parallel(tweenList);
        }

        public static Tween Series(params Tween[] tweenList)
        {
            return Tween.Series(tweenList);
        }
    }
}
