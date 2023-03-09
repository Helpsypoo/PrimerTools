using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal class SequenceMixer
    {
        public CancellationTokenSource lastExecution = new();
        public readonly Dictionary<WeakReference<Sequence>, SequencePlayer> players = new();

        public void Mix(SequencePlayable[] allBehaviours, float time)
        {
            CancelPreviousExecution();

            var bySequence = allBehaviours
                .GroupBy(x => x.sequence)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (sequence, behaviours) in bySequence) {
                GetPlayerFor(sequence)
                    .PlayTo(time, behaviours, lastExecution.Token)
                    .Forget();
            }
        }

        public SequencePlayer GetPlayerFor(Sequence sequence)
        {
            var reference = new WeakReference<Sequence>(sequence);
            if (players.TryGetValue(reference, out var player))
                return player;

            player = new SequencePlayer(sequence);
            players.Add(reference, player);
            return player;
        }

        private void CancelPreviousExecution()
        {
            if (lastExecution is not null)
                lastExecution.Cancel();

            lastExecution = new CancellationTokenSource();
        }
    }
}
