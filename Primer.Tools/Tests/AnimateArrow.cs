using System.Threading.Tasks;
using JetBrains.Annotations;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Tools.Tests
{
    public class AnimateArrow : Triggerable
    {
        public Vector3 start = Vector3.one;
        public Vector3 end = Vector3.one * 3;

        private PrimerArrow2 arrow;

        public Vector3? startInitial;
        public Vector3? endInitial;


        public override void Cleanup()
        {
            arrow = GetComponent<PrimerArrow2>();

            if (startInitial is not null)
                arrow.startPoint.value = startInitial.Value;

            if (endInitial is not null)
                arrow.endPoint.value = endInitial.Value;

            arrow.Recalculate();
        }

        public override void Prepare()
        {
            arrow = GetComponent<PrimerArrow2>();
            startInitial = arrow.start;
            endInitial = arrow.end;
        }


        [UsedImplicitly]
        public async Task Animate()
        {
            await arrow.Animate(start, end).Play();
        }
    }
}
