using Primer.Animation;
using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Shapes
{
    [ExecuteAlways]
    public class PrimerBracket2 : MonoBehaviour, IMeshController
    {
        public const string PREFAB_NAME = "Bracket2";

        #region Children objects
        [SerializeField, PrefabChild]
        private Transform leftTip;
        [SerializeField, PrefabChild]
        private Transform leftBar;
        [SerializeField, PrefabChild]
        private Transform leftCenter;
        [SerializeField, PrefabChild]
        private Transform rightCenter;
        [SerializeField, PrefabChild]
        private Transform rightBar;
        [SerializeField, PrefabChild]
        private Transform rightTip;
        [SerializeField, PrefabChild]
        private Transform latexContainer;
        [SerializeField, PrefabChild]
        private LatexComponent latex;
        #endregion

        #region public float width;
        [SerializeField, HideInInspector]
        private float _width = 1f;

        [ShowInInspector]
        [PropertyOrder(-1)]
        [PropertyRange(0.01f, 10f)]
        public float width
        {
            get => _width;
            set
            {
                _width = value;
                Refresh();
            }
        }
        #endregion

        #region public Color color;
        [ShowInInspector]
        [PropertyOrder(-1)]
        public Color color {
            get => this.GetColor();
            set => this.SetColor(value);
        }
        #endregion

        #region public Material material;
        [ShowInInspector]
        [PropertyOrder(-1)]
        public Material material {
            get => this.GetMaterial();
            set => this.SetMaterial(value);
        }
        #endregion

        [Title("Details")]
        [PropertyOrder(-1)]
        public float tipWidth = 0.39f;

        #region public Vector3 anchor;
        [Title("Anchor")]
        public ScenePoint anchorPoint = Vector3.zero;

        public Vector3 anchor
        {
            get => anchorPoint.vector;
            set => anchorPoint.vector = value;
        }
        #endregion

        #region public Vector3 left;
        [Title("Left")]
        public ScenePoint leftPoint = new Vector3(-1, 0, 1);

        public Vector3 left
        {
            get => leftPoint.vector;
            set => leftPoint.vector = value;
        }
        #endregion

        #region public Vector3 right;
        [Title("Right")]
        public ScenePoint rightPoint = new Vector3(1, 0, 1);

        public Vector3 right
        {
            get => rightPoint.vector;
            set => rightPoint.vector = value;
        }
        #endregion

        #region public Vector3 label;
        [Title("Label")]
        [ShowInInspector]
        public string label {
            get => latex?.latex ?? "";
            set {
                var isActive = !string.IsNullOrWhiteSpace(value);
                latex.gameObject.SetActive(isActive);
                if (isActive) latex.latex = value;
            }
        }

        [ShowInInspector]
        public Vector3 labelOffset {
            get => latexContainer?.transform.localPosition ?? Vector3.zero;
            set => latexContainer.transform.localPosition = value;
        }
        [ShowInInspector]
        public Quaternion labelRotation {
            get => latex?.transform.localRotation ?? Quaternion.identity;
            set => latex.transform.localRotation = value;
        }

        [ShowInInspector]
        public float labelSize {
            get => latex?.transform.localScale.x ?? 1;
            set => latex.transform.localScale = Vector3.one * value;
        }

        public bool hasLabel => latex.gameObject.activeSelf;
        #endregion


        #region Unity events
        private void OnEnable()
        {
            anchorPoint.onChange = Refresh;
            leftPoint.onChange = Refresh;
            rightPoint.onChange = Refresh;
        }

        private void OnDisable()
        {
            anchorPoint.onChange = null;
            leftPoint.onChange = null;
            rightPoint.onChange = null;
        }

        private void OnValidate()
        {
            Refresh();
        }

        private void Update()
        {
            if (ScenePoint.CheckTrackedObject(anchorPoint, leftPoint, rightPoint))
            {
                Refresh();
            }
        }
        #endregion


        public PrimerBracket2 SetPoints(Vector3? anchor = null, Vector3? left = null, Vector3? right = null, bool? isGlobal = null)
        {
            if (anchor.HasValue) this.anchor = anchor.Value;
            if (left.HasValue) this.left = left.Value;
            if (right.HasValue) this.right = right.Value;

            if (!isGlobal.HasValue)
                return this;

            anchorPoint.isWorldPosition = isGlobal.Value;
            leftPoint.isWorldPosition = isGlobal.Value;
            rightPoint.isWorldPosition = isGlobal.Value;
            return this;
        }

        public Tween Animate(
            Vector3Provider anchorEnd = null,
            Vector3Provider leftEnd = null,
            Vector3Provider rightEnd = null)
        {
            var anchorTween = anchorPoint.Tween(anchorEnd);
            var leftTween = leftPoint.Tween(leftEnd);
            var rightTween = rightPoint.Tween(rightEnd);

            return new Tween(t => {
                if (anchorTween is not null)
                    anchorPoint.vector = anchorTween(t);

                if (leftTween is not null)
                    leftPoint.vector = leftTween(t);

                if (rightTween is not null)
                    rightPoint.vector = rightTween(t);

                Refresh();
            });
        }

        public Tween GrowFromAnchor()
        {
            Refresh();
            var storedScale = transform.localScale;
            transform.localScale = Vector3.zero;
            return transform.ScaleTo(storedScale);
        }

        // This method is marked as performance intensive because it logs a warning 🤦
        // ReSharper disable Unity.PerformanceAnalysis
        [Title("Controls", horizontalLine: false)]
        [Button("Refresh", ButtonSizes.Large)]
        public void Refresh()
        {
            if (leftTip == null || leftBar == null || rightBar == null || rightTip == null || gameObject.IsPreset())
                return;

            var self = transform;
            var parent = self.parent;

            var anchorLocal = anchorPoint.GetLocalPosition(parent);
            var leftLocal = leftPoint.GetLocalPosition(parent);
            var rightLocal = rightPoint.GetLocalPosition(parent);

            // mouth is the open side of the bracket
            var mouth = leftLocal - rightLocal;
            var toLeft = leftLocal - anchorLocal;
            var toRight = rightLocal - anchorLocal;

            // Cross returns a vector that is orthogonal (perpendicular) to both input parameters
            var upwards = Vector3.Cross(toLeft, toRight);
            var forward = Vector3.Cross(upwards, mouth);
            var center = Vector3.Project(toLeft, forward);

            var leftLength = BarLength(toLeft, center);
            var rightLength = BarLength(toRight, center);

            if (leftLength < 0.01f || rightLength < 0.01f || Mathf.Abs(leftLength + rightLength) > mouth.magnitude) {
                Debug.LogWarning("Bracket may look broken");
            }

            leftBar.localScale = new Vector3(leftLength, 1, 1);
            rightBar.localScale = new Vector3(rightLength, 1, 1);

            self.rotation = Quaternion.LookRotation(forward, upwards);
            self.localScale = new Vector3(width, width, center.magnitude);

            self.position = anchorPoint.GetWorldPosition(parent);
            leftTip.position = leftPoint.GetWorldPosition(parent);
            rightTip.position = rightPoint.GetWorldPosition(parent);

            FixLatexScale();
        }

        [Button(ButtonSizes.Large)]
        public void FlipLabel()
        {
            latex.transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        [Button(ButtonSizes.Large)]
        private void CopyValues()
        {
            GUIUtility.systemCopyBuffer = @$"
.SetPoints(
    anchor: {anchorPoint.vector.ToCode()},
    left: {leftPoint.vector.ToCode()},
    right: {rightPoint.vector.ToCode()}
)
            ".Trim();
        }

        private float BarLength(Vector3 toSide, Vector3 center)
        {
            var diff = toSide - center;
            var distance = diff.magnitude;
            var tipsWidth = tipWidth * width * 2;
            return (distance - tipsWidth) / width;
        }

        private void FixLatexScale()
        {
            var parentScale = transform.localScale;

            if (parentScale.x is 0 || parentScale.y is 0 || parentScale.z is 0)
                return;

            latexContainer.localScale = new Vector3(
                1 / parentScale.x,
                1 / parentScale.y,
                1 / parentScale.z
            );
        }

        MeshRenderer[] IMeshController.GetMeshRenderers()
        {
            return GetComponentsInChildren<MeshRenderer>();
        }
    }
}
