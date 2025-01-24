using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SummonableObject : MonoBehaviour
{
    [SerializeField] EnvironmentSpawner.EnvironmentType environmentType;
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
            InteractionManager.Instance.PlaceStencil(collision.gameObject, environmentType);
        }
    }
    // Fade the pieces out
    IEnumerator FadePieces()
    {
        float elapsedTime = 0f;
        float duration = 1f;

        while (elapsedTime < duration)
        {
            float alpha = 1f - (elapsedTime / duration);
            foreach (var piece in pieces)
            {
                piece.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, alpha);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        foreach (var piece in pieces)
        {
            piece.SetActive(false);
        }
    }
}
