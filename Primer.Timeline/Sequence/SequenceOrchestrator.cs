using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal static class SequenceOrchestrator
    {
        private static readonly SingleExecutionGuarantee executionGuarantee = new();
        private static readonly Dictionary<Sequence, SequencePlayer> players = new();
        private static readonly List<UniTask> tasks = new();
        private static float lastSecond = 0;

        public static float end => lastSecond;

        public static UniTask AllSequencesFinished() => UniTask.WhenAll(tasks);

        public static void PlayTo(SequencePlayable[] behaviours, float time)
        {
            var lastTime = behaviours.Max(x => x.end) + 1;

            if (lastTime > lastSecond)
                lastSecond = lastTime;

            PlayBehaviours(behaviours, time);

            if (behaviours.Count(x => x.start <= time) == 0)
                PrimerTimeline.DisposeEphemeralObjects();
        }

        private static void PlayBehaviours(IEnumerable<SequencePlayable> allBehaviours, float time)
        {
            var ct = executionGuarantee.NewExecution();

            tasks.Clear();

            var bySequence = allBehaviours
                .Where(x => x.trackTarget is not null)
                .GroupBy(x => x.trackTarget)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (sequence, behaviours) in bySequence) {
                var task = GetPlayerFor(sequence).PlayTo(time, behaviours, ct);
                tasks.Add(task);
            }
        }

        public static void Clear()
        {
            foreach (var player in players.Values) {
                player.Reset().Forget();
                player.Clean();
            }

            players.Clear();
            tasks.Clear();
        }

        internal static SequencePlayer GetPlayerFor(Sequence sequence)
        {
            if (players.TryGetValue(sequence, out var player))
                return player;

            player = new SequencePlayer(sequence);
            players.Add(sequence, player);
            return player;
        }
    }
}
