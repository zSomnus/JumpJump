using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicWater : MonoBehaviour
{

    [System.Serializable]
    public struct Bound
    {
        public float top;
        public float right;
        public float bottom;
        public float left;
    }

    [Header("Water Settings")]
    [SerializeField] Bound bound;
    [SerializeField] int quality;

    [SerializeField] Material waterMaterial;
    [SerializeField] GameObject splash;

    Vector3[] vertices;
    Mesh mesh;

    [Header("Physics Settings")]
    [SerializeField] float sprigConstant = 0.02f;
    [SerializeField] float damping = 0.1f;
    [SerializeField] float spread = 0.1f;
    [SerializeField] float collisionVelocityFactor = 0.04f;

    float[] velocities;
    float[] accelerations;
    float[] leftDeltas;
    float[] rightDeltas;

    float timer;

    void GenerateMesh()
    {
        float range = (bound.right - bound.left) / (quality - 1);
        vertices = new Vector3[quality * 2];

        for (int i = 0; i < quality; i++)
        {
            vertices[i] = new Vector3(bound.left + (i * range), bound.top, 0);
        }

        for (int i = 0; i < quality; i++)
        {
            vertices[i + quality] = new Vector2(bound.left + (i * range), bound.bottom);
        }

        int[] template = new int[6];
        template[0] = quality;
        template[1] = 0;
        template[2] = quality + 1;
        template[3] = 0;
        template[4] = 1;
        template[5] = quality + 1;

        int marker = 0;
        int[] tris = new int[((quality - 1) * 2) * 3];

        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = template[marker++]++;

            if (marker >= 6)
            {
                marker = 0;
            }
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "Mid";

        if (waterMaterial)
        {
            meshRenderer.sharedMaterial = waterMaterial;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    void InitializePhysics()
    {
        velocities = new float[quality];
        accelerations = new float[quality];
        leftDeltas = new float[quality];
        rightDeltas = new float[quality];
    }

    private void Start()
    {
        InitializePhysics();
        GenerateMesh();
        SetBosCollider2D();
    }

    private void SetBosCollider2D()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        float width = bound.right - bound.left;
        float height = bound.top - bound.bottom;

        Vector2 offset = new Vector2((bound.right + bound.left) / 2f, (bound.top + bound.bottom) / 2f);

        col.size = new Vector2(width, height);
        col.offset = offset;
        col.isTrigger = true;
    }

    private void Update()
    {
        if (timer <= 0)
        {
            return;
        }

        timer -= Time.deltaTime;

        for (int i = 0; i < quality; i++)
        {
            float force = sprigConstant * (vertices[i].y - bound.top) + velocities[i] * damping;
            accelerations[i] = -force;
            vertices[i].y += velocities[i];
            velocities[i] += accelerations[i];
        }

        for (int i = 0; i < quality; i++)
        {
            if (i > 0)
            {
                leftDeltas[i] = spread * (vertices[i].y - vertices[i - 1].y);
                velocities[i - 1] += leftDeltas[i];
            }

            if (i < quality - 1)
            {
                rightDeltas[i] = spread * (vertices[i].y - vertices[i + 1].y);
                velocities[i + 1] += rightDeltas[i];
            }
        }

        mesh.vertices = vertices;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        Splash(collision, rb.velocity.y * collisionVelocityFactor);
    }

    private void Splash(Collider2D collision, float force)
    {
        timer = 3f;
        float radius = collision.bounds.max.x - collision.bounds.min.x;
        Vector2 center = new Vector2(collision.bounds.center.x, bound.top);

        for (int i = 0; i < quality; i++)
        {
            if (PointInsideCircle(vertices[i], center, radius))
            {
                velocities[i] = force;
            }
        }
    }

    private bool PointInsideCircle(Vector3 point, Vector2 center, float radius)
    {
        return Vector2.Distance(point, center) < radius;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 leftTop = new Vector3(bound.left, bound.top) + transform.position;
        Vector3 rightTop = new Vector3(bound.right, bound.top) + transform.position;
        Vector3 leftBottom = new Vector3(bound.left, bound.bottom) + transform.position;
        Vector3 rightBottom = new Vector3(bound.right, bound.bottom) + transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(leftTop, rightTop);
        Gizmos.DrawLine(leftTop, leftBottom);
        Gizmos.DrawLine(rightTop, rightBottom);
        Gizmos.DrawLine(leftBottom, rightBottom);
    }
}