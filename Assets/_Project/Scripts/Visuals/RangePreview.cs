using UnityEngine;

namespace OrbitalDefense
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RangePreview : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private int segments = 96;
        [SerializeField] private float lineWidth = 0.035f;
        [SerializeField] private Color previewColor = new(0.35f, 0.85f, 1f, 0.72f);

        private Transform target;
        private float radius;

        private void Awake()
        {
            line ??= GetComponent<LineRenderer>();
            ConfigureLine();
            Hide();
        }

        private void LateUpdate()
        {
            if (target == null || radius <= 0f)
            {
                Hide();
                return;
            }

            DrawCircle(target.position, radius);
        }

        public void Show(Transform followTarget, float range)
        {
            target = followTarget;
            radius = Mathf.Max(0f, range);

            if (target == null || radius <= 0f)
            {
                Hide();
                return;
            }

            if (line != null)
            {
                line.enabled = true;
            }

            DrawCircle(target.position, radius);
        }

        public void Hide()
        {
            target = null;
            radius = 0f;
            if (line != null)
            {
                line.enabled = false;
            }
        }

        private void ConfigureLine()
        {
            if (line == null)
            {
                return;
            }

            line.loop = true;
            line.useWorldSpace = true;
            line.positionCount = Mathf.Max(12, segments);
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = previewColor;
            line.endColor = previewColor;
            line.sortingOrder = 30;

            if (line.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                line.sharedMaterial = new Material(shader);
            }
        }

        private void DrawCircle(Vector3 center, float circleRadius)
        {
            if (line == null)
            {
                return;
            }

            int count = Mathf.Max(12, segments);
            if (line.positionCount != count)
            {
                line.positionCount = count;
            }

            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * circleRadius, Mathf.Sin(angle) * circleRadius, 0f);
                line.SetPosition(i, point);
            }
        }
    }
}
