using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PortalObjectPool : MonoBehaviour
{
    public static PortalObjectPool Instance { get; private set; }
    [SerializeField] GameObject portalPrefab;
    [SerializeField] int poolSize;
    List<GameObject> clones;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Debug.Log("POP started");
        clones = new();
        for(int i = 0; i < poolSize; i++)
        {
            var clone = Instantiate(portalPrefab, transform);
            clones.Add(clone);
            clone.SetActive(false);
        }
    }

    public GameObject GetObject(Vector3 position)
    {
        Debug.Log("GetObject");
        var clone = clones.Where(clone => !clone.activeInHierarchy).FirstOrDefault();
        if(clone != null)
        {
            clone.SetActive(true);
            clone.transform.position = position;
            return clone;
        }
        else
        {
            Debug.Log("No poolObject left.");
            return null;
        }

    }
}
