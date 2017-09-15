﻿using UnityEngine;
using VRTK;

public class CapacitorPlateController : VRTK_InteractableObject
{
    private GameObject resizeHeightObject1;
    private GameObject resizeHeightObject2;
    private GameObject resizeWidthObject1;
    private GameObject resizeWidthObject2;

    [SerializeField]
    private float resizeObjectSize = 0.02f;

    [SerializeField]
    private float maxHeightSize = 0.5f;

    [SerializeField]
    private float maxWidthSize = 1;

    private void Start()
    {
        resizeWidthObject1 = CreateResizeObject(Vector3.right, maxWidthSize);
        resizeWidthObject2 = CreateResizeObject(Vector3.right, maxWidthSize);
        resizeHeightObject1 = CreateResizeObject(Vector3.up, maxHeightSize);
        resizeHeightObject2 = CreateResizeObject(Vector3.up, maxHeightSize);

        EnableResizeObjects(false);

        disableWhenIdle = false;
    }

    protected override void Update()
    {
        base.Update();

        Vector3 size = GetComponent<Renderer>().bounds.size;
        Vector3 offset_x = new Vector3(size.x / 2, 0, 0);
        Vector3 offset_y = new Vector3(0, size.y / 2, 0);

        if (resizeWidthObject1 != null)
            resizeWidthObject1.transform.position = transform.position + offset_x;
        if(resizeWidthObject2 != null)
            resizeWidthObject2.transform.position = transform.position - offset_x;
        if (resizeHeightObject1 != null)
            resizeHeightObject1.transform.position = transform.position + offset_y;
        if (resizeHeightObject2 != null)
            resizeHeightObject2.transform.position = transform.position - offset_y;
    }

    private GameObject CreateResizeObject(Vector3 resizeAxis, float maxSize)
    {
        GameObject resizeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        resizeObject.GetComponent<SphereCollider>().radius = 2;
        resizeObject.transform.localScale = new Vector3(resizeObjectSize, resizeObjectSize, resizeObjectSize);
        resizeObject.GetComponent<Renderer>().material.color = Color.black;

        CapacitorPlateResizeController resizeController = resizeObject.AddComponent<CapacitorPlateResizeController>();
        resizeController.isUsable = true;
        resizeController.setCapacitorPlate(this);
        resizeController.setResizeAxsis(resizeAxis);
        resizeController.setMaxSize(maxSize);

        return resizeObject;
    }

    public void EnableResizeObjects(bool value)
    {
        resizeWidthObject1.GetComponent<Renderer>().enabled = value;
        resizeWidthObject2.GetComponent<Renderer>().enabled = value;
        resizeHeightObject1.GetComponent<Renderer>().enabled = value;
        resizeHeightObject2.GetComponent<Renderer>().enabled = value;
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        base.StartTouching(currentTouchingObject);

        EnableResizeObjects(true);
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);

        EnableResizeObjects(false);
    }
}