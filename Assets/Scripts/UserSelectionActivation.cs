using System;
using System.Collections;
using UnityEngine;

public class UserSelectionActivation : MonoBehaviour
{
    public int mode = 0;
    
    [SerializeField] private GameObject customCutoutManager;
    [SerializeField] private GameObject customFrameMesh;
    
    private Vector3 meshOriginalScale;

    private void Start()
    {
        meshOriginalScale = customFrameMesh.transform.localScale;
    }

    public void ActivateUserSelection()
    {
        if (mode == 0)
        {
            mode = 1;
        }
        else
        {
            mode = 0;
        }
        
        StartCoroutine(EnableUserSelection());
    }

    private IEnumerator EnableUserSelection()
    {
        if (mode == 1)
        {
            customFrameMesh.transform.localScale *= 0.5f;
            yield return new WaitForSeconds(0.1f);
            customCutoutManager.SetActive(true);
        }
        else
        {
            customCutoutManager.SetActive(false);
            customFrameMesh.transform.localScale = meshOriginalScale;
        }
    }
}
