﻿using UnityEngine;
using UnityEngine.EventSystems;
using Vector3 = UnityEngine.Vector3;

public class UIItemDragHandlerSimple : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("General Settings")]
    public Canvas parentCanvas;
    public float returnTime = 1f;
    public GameObject generatedObject;
    
    [Header("Other Affected GameObjects")]
    public GameObject ChangeLayerObject;
    
    [Header("Particle Movement Restrictions")]
    public GameObject minPosition2d;
    public GameObject maxPosition2d;
    public GameObject minPosition3d;
    public GameObject maxPosition3d;

    private Vector3 _initialPosition;
    private Vector3 _initialMousePosition;

    private bool _slowlyReturnToOrigin = false;
    private Vector3 _returnDirection;
    private float _time;
    
    private float _currentCharge = 0f;
    private bool _fixedPosition = false;

    private bool _allowAddingCharges = true;

    private void Update()
    {
        if (_slowlyReturnToOrigin)
        {
            transform.position += Time.deltaTime * _returnDirection / returnTime;
            _time += Time.deltaTime;

            if (_time >= returnTime)
            {
                transform.position = _initialPosition;
                _slowlyReturnToOrigin = false;
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _initialPosition = transform.position;
        if (ChangeLayerObject)
        {
            ChangeLayerObject.layer = 0; //Default Layer
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        var screenPoint = Input.mousePosition;
        var finish = parentCanvas.worldCamera.ScreenToWorldPoint(screenPoint);
        finish.z = 0f;
        transform.position = finish;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, 100f);

        var hitVectorField = false;
        foreach (var hit in hits)
        {
            if(!hit.transform.CompareTag("VectorField"))
                continue;

            if (!_allowAddingCharges)
                return;

            hitVectorField = true;
            ShowObject(hit.point, hit.transform.parent);
            transform.position = _initialPosition;
        }

        if (!hitVectorField)
        {
            _slowlyReturnToOrigin = true;
            _time = 0f;
            _returnDirection = _initialPosition - transform.position;
        }
        
        if (ChangeLayerObject)
        {
            ChangeLayerObject.layer = 2; //Ignore Raycast
        }
    }

    public void SetObjectToOrigin()
    {
        if (!_allowAddingCharges)
            return;

        var vecFields = GameObject.FindGameObjectsWithTag("VectorField"); //should be 2 -> one 2d and one 3d, where only one should be active at a time

        foreach (var vecField in vecFields)
        {
            if(!vecField.activeInHierarchy) continue;
            ShowObject(vecField.transform.position, vecField.transform.parent);
        }
    }

    protected virtual void ShowObject(Vector3 position, Transform parent)
    {
        if (generatedObject == null) return;
//        var obj = Instantiate(generatedObject, position, Quaternion.identity, parent);

        var obj = generatedObject;
        
        obj.transform.parent = parent;
        obj.transform.position = position;
        obj.SetActive(true);
    }
}