﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalCreation : MonoBehaviour
{
    // check for double click
    bool one_click = false;
    float timer;
    public float doubleClickDelay = .2f;

    // ui kinda
    int stage = 0; // 0: no portals created / 1: portal a being created / 2: portal a created, portal b ready to place
    public float range;
    LineRenderer lineOfSight;

    // set size of portal
    public LineRenderer outlineA;
    public LineRenderer outlineB;
    Vector3 normA;
    Vector3 normB;
    public float offset = 0.04f;

    // portal meta
    public GameObject player;
    public GameObject gameManager;
    float numPortals;
    Portal portalA;
    Portal portalB;

    public Text text;
    public float readTime;
    //int lesson = 0;
    // Start is called before the first frame update
    void Start()
    {
        stage = 0;
        lineOfSight = gameObject.transform.GetComponentInChildren<LineRenderer>();
        outlineA.enabled = false;
        outlineB.enabled = false;
        numPortals = 0;

        //lesson++;
        text.text = "right click to create portal";
        StartCoroutine(Instruct());
    }

    IEnumerator Instruct()
    {
        //lesson++;
        text.enabled = true;
        text.text = text.text.ToUpper();
        yield return new WaitForSeconds(readTime);
        text.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Raycasting
        Ray ray = new Ray(transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction);
        Physics.Raycast(ray, out RaycastHit hit);
        lineOfSight.SetPosition(0, ray.origin);
        lineOfSight.SetPosition(1, hit.point);

        if (Input.GetButtonDown("Portal"))
        {
            if (one_click) // double click
            {
                one_click = false;
                Reset();
            }
            else // first click
            {
                one_click = true;
                timer = Time.time;

                if (hit.collider)
                {
                    if (stage == 0)
                    {
                        stage = 1;
                        StartA(hit);
                    }
                    else if (stage == 1)
                    {
                        stage = 2;
                    }
                    else if (stage == 2)
                    {
                        StartCoroutine(SpecialPortal());
                    }
                }
                else
                {
                    Reset();
                }
            }
        }

        if (stage == 1 && hit.collider && hit.normal == normA)
        {
            UpdateShape(hit);
        }
        else if (stage == 2 && hit.collider)
        {
            SetB(hit);
        }

        // reset double click
        if (one_click && Time.time - timer > doubleClickDelay)
        {
            one_click = false;
        }
    }

    void Reset()
    {
        stage = 0;
        outlineA.enabled = false;
        outlineB.enabled = false;
    }
    void StartA(RaycastHit hit)
    {
        outlineA.enabled = true;
        normA = hit.normal;
        outlineA.SetPosition(0, new Vector3(hit.point.x, hit.point.y, hit.point.z) + normA * offset);
        stage = 1;
    }

    void UpdateShape(RaycastHit hit)
    {
        Vector3 b = hit.point + normA * offset;
        Vector3 a = outlineA.GetPosition(0);
        outlineA.SetPosition(2, b);

        if (Mathf.Abs(normA.z) > .5)
        {
            outlineA.SetPosition(1, new Vector3(a.x, hit.point.y, a.z));
            outlineA.SetPosition(3, new Vector3(hit.point.x, a.y, a.z));
        }
        else if (Mathf.Abs(normA.x) > .5)
        {
            outlineA.SetPosition(1, new Vector3(a.x, a.y, b.z));
            outlineA.SetPosition(3, new Vector3(a.x, b.y, a.z));
        }
        else if (Mathf.Abs(normA.y) > .5)
        {
            outlineA.SetPosition(1, new Vector3(a.x, a.y, b.z));
            outlineA.SetPosition(3, new Vector3(b.x, a.y, a.z));
        }
    }

    void SetB(RaycastHit hit)
    {
        float height = (outlineA.GetPosition(0) - outlineA.GetPosition(1)).magnitude / 2;
        float width = (outlineA.GetPosition(1) - outlineA.GetPosition(2)).magnitude / 2;

        normB = hit.normal;
        Vector3 midB = hit.point + normB * offset;
        outlineB.enabled = true;

        if (Mathf.Abs(normB.z) > .5)
        {
            outlineB.SetPosition(0, new Vector3(midB.x - width, midB.y + height, midB.z));
            outlineB.SetPosition(1, new Vector3(midB.x + width, midB.y + height, midB.z));
            outlineB.SetPosition(2, new Vector3(midB.x + width, midB.y - height, midB.z));
            outlineB.SetPosition(3, new Vector3(midB.x - width, midB.y - height, midB.z));
        }
        else if (Mathf.Abs(normB.x) > .5)
        {
            outlineB.SetPosition(0, new Vector3(midB.x, midB.y - width, midB.z + height));
            outlineB.SetPosition(1, new Vector3(midB.x, midB.y + width, midB.z + height));
            outlineB.SetPosition(2, new Vector3(midB.x, midB.y + width, midB.z - height));
            outlineB.SetPosition(3, new Vector3(midB.x, midB.y - width, midB.z - height));
        }
        else if (Mathf.Abs(normB.y) > .5)
        {
            outlineB.SetPosition(0, new Vector3(midB.x - width, midB.y, midB.z + height));
            outlineB.SetPosition(1, new Vector3(midB.x + width, midB.y, midB.z + height));
            outlineB.SetPosition(2, new Vector3(midB.x + width, midB.y, midB.z - height));
            outlineB.SetPosition(3, new Vector3(midB.x - width, midB.y, midB.z - height));
        }
    }

    // IEnumerator in case player resets portal instead of instantiates one
    IEnumerator SpecialPortal()
    {
        yield return new WaitForSeconds(doubleClickDelay);

        if (stage == 2)
        {
            CreatePortals();
            Reset();
        }
    }
    void CreatePortals()
    {
        float height = (outlineA.GetPosition(0) - outlineA.GetPosition(1)).magnitude;
        float width = (outlineA.GetPosition(1) - outlineA.GetPosition(2)).magnitude;
        Vector3 midA = (outlineA.GetPosition(0) + outlineA.GetPosition(2)) / 2;
        Vector3 midB = (outlineB.GetPosition(0) + outlineB.GetPosition(2)) / 2;

        portalA = new Portal(width, height, midA, normA, player, "a" + Mathf.Floor(numPortals / 2));
        portalB = new Portal(width, height, midB, normB, player, "b" + Mathf.Floor(numPortals / 2));
        Portal.pairPortals(portalA, portalB);
        gameManager.GetComponent<GameManager>().portalList.Add(portalA);

        numPortals++;
    }
}
