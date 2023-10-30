using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Simulation
{
    public class LandscapeItem : MonoBehaviour
    {
        [SerializeField]
        private Landscape _landscape;

        public Landscape landscape
        {
            get => _landscape ? _landscape : FindLandscape();
            set => _landscape = value;
        }

        private Landscape FindLandscape()
        {
            // Check a few possibilities for where the landscape might be
            // Child of parent of parent. (The way sims are structured as of 10/30/2023)
            var landscapeCandidates = transform.parent.parent.GetComponentsInChildren<Landscape>();
            if (landscapeCandidates.Length > 1) Debug.LogWarning("FindLandscape: Multiple landscapes found, using first one");
            if (landscapeCandidates.Length > 0) return landscapeCandidates[0];
            
            // Parent
            var landscapeCandidate = transform.GetComponentInParent<Landscape>();
            if (landscapeCandidate != null) return landscapeCandidate;
            
            // Child of parent
            landscapeCandidates = transform.parent.GetComponentsInChildren<Landscape>();
            if (landscapeCandidates.Length > 1) Debug.LogWarning("FindLandscape: Multiple landscapes found, using first one");
            if (landscapeCandidates.Length > 0) return landscapeCandidates[0];
            
            // Find all
            landscapeCandidates = FindObjectsOfType<Landscape>();
            if (landscapeCandidates.Length > 1) Debug.LogWarning("FindLandscape: Multiple landscapes found, using first one");
            if (landscapeCandidates.Length > 0) return landscapeCandidates[0];
            
            Debug.LogError("FindLandscape: No landscape found");
            return null;
        }

        [Button]
        public void TouchGround()
        {
            transform.localPosition = landscape.GetGroundAtWorldPoint(transform.localPosition);
        }
    }
}
