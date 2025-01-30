using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SummonableObject : MonoBehaviour
{
    [SerializeField] EnvironmentSpawner.EnvironmentType environmentType;
    [SerializeField] Material stencilMaterial;
    [SerializeField] float destroyVelocityThreshold = 2f;
    [SerializeField] GameObject destroyedParent;
    List<GameObject> pieces = new List<GameObject>();
    void Start()
    {
        pieces = destroyedParent.GetComponentsInChildren<Transform>()
            .Where(t => t != destroyedParent.transform)
            .Select(t => t.gameObject)
            .ToList();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > destroyVelocityThreshold && collision.gameObject.name.Contains("Effect"))
        {
            destroyedParent.SetActive(true);
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(FadePieces());

            Vector3 explosionCenter = transform.position;
            float explosionForce = 30f;
            float explosionRadius = 2f;

            foreach (var piece in pieces)
            {
                Rigidbody rb;
                if (!piece.TryGetComponent<Rigidbody>(out rb))
                {
                    rb = piece.AddComponent<Rigidbody>();
                }
                rb.useGravity = false;
                rb.linearDamping = 5;
                rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius);
            }

            // Make effect mesh transparent
            InteractionManager.Instance.PlaceStencil(collision.gameObject, environmentType, stencilMaterial);
        }
    }
    // Fade the pieces out
    IEnumerator FadePieces()
    {
        float elapsedTime = 0f;
        float duration = 1f;
        foreach (var piece in pieces)
        {
            SetMeshRendererTransparent(piece.GetComponent<MeshRenderer>(), true);
        }
        while (elapsedTime < duration)

        {
            float alpha = 1f - (elapsedTime / duration);
            foreach (var piece in pieces)
            {
                foreach (var material in piece.GetComponent<MeshRenderer>().materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }

            }
            elapsedTime += Time.deltaTime;
            yield return null;

        }
        foreach (var piece in pieces)
        {
            piece.SetActive(false);
        }
    }
    void SetMeshRendererTransparent(MeshRenderer meshRenderer, bool enable)
    {
        foreach (var material in meshRenderer.materials)
        {
            if (enable)
            {
                material.SetInt("_Surface", 1);  // 1 = transparent
                material.SetInt("_Blend", 0);    // 0 = alpha blend
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = 3000;

                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                material.SetInt("_Surface", 0);  // 0 = opaque

                material.SetInt("_Blend", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = -1;  // Default render queue

                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
    }
}
