using UnityEngine;

namespace OrbitalDefense
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class OrbitRingRenderer : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private float radius = 2.65f;
        [SerializeField] private int segments = 96;
        [SerializeField] private float lineWidth = 0.03f;
        [SerializeField] private Color color = new(0.80f, 0.86f, 1f, 0.18f);

        public void Initialize(float ringRadius, Color ringColor)
        {
            radius = ringRadius;
            color = ringColor;
            line ??= GetComponent<LineRenderer>();
            Redraw();
        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                Redraw();
            }
        }

        private void OnEnable()
        {
            line ??= GetComponent<LineRenderer>();
            Redraw();
        }

        private void OnValidate()
        {
            line ??= GetComponent<LineRenderer>();
            Redraw();
        }

        private void Redraw()
        {
            if (line == null)
            {
                return;
            }

            int count = Mathf.Max(12, segments);
            line.loop = true;
            line.useWorldSpace = true;
            line.positionCount = count;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = color;
            line.endColor = color;

            if (line.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                line.sharedMaterial = new Material(shader);
            }

            Vector3 center = transform.position;
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                line.SetPosition(i, point);
            }
        }
    }
}
