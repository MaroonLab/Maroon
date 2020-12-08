﻿using System;
using System.Collections;
using System.Collections.Generic;
using GEAR.Localization;
using PlatformControls.PC;
using TMPro;
using UnityEngine;

public class SortingArray : SortingVisualization, IResetObject
{
    [SerializeField] private GameObject arrayPlace;
    [SerializeField] private GameObject bucketPrefab;

    [SerializeField] private GameObject bucketsObject;
    
    [SerializeField] private Transform leftBorder;
    [SerializeField] private Transform rightBorder;
    
    [SerializeField] private TextMeshProUGUI pseudoCodeText;
    [SerializeField] private TextMeshProUGUI swapsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    private Vector3 _placeOffset;
    private List<ArrayPlace> _elements = new List<ArrayPlace>();
    private List<SortingBucket> _buckets = new List<SortingBucket>();

    public override int Size
    {
        get => _size;
        set
        {
            _size = value;
            AdaptDetailArraySize(_size);
        }
    }

    public override void Init(int size)
    {
        base.Init(size);
        CreateBuckets();
        HideBuckets();
    }

    public override void SortingFinished()
    {
        SimulationController.Instance.StopSimulation();
    }

    private void AdaptDetailArraySize(int size)
    {
        float padding = (rightBorder.position.x - leftBorder.position.x) / size;
        _placeOffset = new Vector3(padding, 0, 0);
        Vector3 elementPosition = leftBorder.position + _placeOffset / 2;
        for(int i = 0; i < size; i++)
        {
            if (i >= _elements.Count)
            {
                var newArrayPlace = Instantiate(arrayPlace, leftBorder.parent, false) as GameObject;

                var arrayPlaceComponent = newArrayPlace.GetComponent<ArrayPlace>();
                arrayPlaceComponent.SetBaseNumber(i);
                _elements.Add(arrayPlaceComponent);
            }
            _elements[i].transform.position = elementPosition;
            elementPosition += _placeOffset;
        }

        while (_elements.Count > size)
        {
            var lastElement = _elements[_elements.Count - 1];
            Destroy(lastElement.gameObject);
            _elements.Remove(lastElement);
        }
    }

    private void CreateBuckets()
    {
        float padding = (rightBorder.position.x - leftBorder.position.x) / 10;
        Vector3 placeOffset = new Vector3(padding, 0, 0);
        Vector3 elementPosition = leftBorder.position + placeOffset / 2 - new Vector3(0, 0, 1);
        for(int i = 0; i < 10; i++)
        {
            var newBucket = Instantiate(bucketPrefab, bucketsObject.transform, false) as GameObject;
            var bucketComponent = newBucket.GetComponent<SortingBucket>();
            _buckets.Add(bucketComponent);
            bucketComponent.SetIndex(i);
            
            newBucket.transform.position = elementPosition;
            elementPosition += placeOffset;
        }
    }

    public override void HideBuckets()
    {
        bucketsObject.SetActive(false);
    }
    
    public override void ShowBuckets()
    {
        bucketsObject.SetActive(true);
    }

    public override void Insert(int fromIdx, int toIdx)
    {
        StartVisualization();
        StartCoroutine(InsertCoroutine(fromIdx, toIdx));
    }

    private IEnumerator InsertCoroutine(int fromIdx, int toIdx)
    {
        _elements[fromIdx].FadeOutSeconds(_timePerMove / 2);

        int i = toIdx;
        if (fromIdx > toIdx)
        {
            for (i = toIdx; i < fromIdx; i++)
            {
                _elements[i].MoveOutRight(_timePerMove / 2, _placeOffset / 2);
            }
        }
        else
        {
            for (i = toIdx; i > fromIdx; i--)
            {
                _elements[i].MoveOutLeft(_timePerMove / 2, _placeOffset / 2);
            }
        }
         
        yield return new WaitForSeconds(_timePerMove / 2);

        int fromValue = _elements[fromIdx].Value;
        i = fromIdx;
        if (fromIdx > toIdx)
        {
             while (i > toIdx)
             {
                 _elements[i].Value = _elements[i-1].Value;
                 i--;
             }

             _elements[toIdx].Value = fromValue;
        }
        else
        {
            while (i < toIdx)
            {
                _elements[i].Value = _elements[i+1].Value;
                i++;
            }

            _elements[toIdx].Value = fromValue;
        }

        _elements[toIdx].FadeInSeconds(_timePerMove / 2);
        
        if (fromIdx > toIdx)
        {
            for (i = toIdx + 1; i <= fromIdx; i++)
            {
                _elements[i].MoveInLeft(_timePerMove / 2, _placeOffset / 2);
            }
        }
        else
        {
            for (i = toIdx - 1; i >= fromIdx; i--)
            {
                _elements[i].MoveInRight(_timePerMove / 2, _placeOffset / 2);
            }
        }

        yield return new WaitForSeconds(_timePerMove / 2);

        _visualizationActive = false;
    }
    
    public override void Swap(int fromIdx, int toIdx)
    {
        StartVisualization();
        StartCoroutine(SwapCoroutine(fromIdx, toIdx));
    }

    private IEnumerator SwapCoroutine(int fromIdx, int toIdx)
    {
        _elements[fromIdx].FadeOutSeconds(_timePerMove / 2);
        _elements[toIdx].FadeOutSeconds(_timePerMove / 2);
        
        yield return new WaitForSeconds(_timePerMove / 2);
        
        int fromValue = _elements[fromIdx].Value;
        _elements[fromIdx].Value = _elements[toIdx].Value;
        _elements[toIdx].Value = fromValue;
        
        _elements[fromIdx].FadeInSeconds(_timePerMove / 2);
        _elements[toIdx].FadeInSeconds(_timePerMove / 2);
        
        yield return new WaitForSeconds(_timePerMove / 2);

        _visualizationActive = false;
    }

    public override void CompareGreater(int idx1, int idx2)
    {
        StartVisualization();
        _elements[idx1].HighlightForSeconds(_timePerMove);
        _elements[idx2].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void VisualizeMaxValue(int maxIndex)
    {
        StartVisualization();
        _elements[maxIndex].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void VisualizeBucketNumber(int ind, int bucket)
    {
        StartVisualization();
        _elements[ind].HighlightForSeconds(_timePerMove);
        _buckets[bucket].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void MoveToBucket(int from, int bucket)
    {
        StartVisualization();
        _elements[from].FadeOutSeconds(_timePerMove);
        _elements[from].Hidden = true;
        _buckets[bucket].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void UndoMoveToBucket(int from, int bucket, int value)
    {
        StartVisualization();
        _elements[from].Value = value;
        _elements[from].FadeInSeconds(_timePerMove);
        _elements[from].Hidden = false;
        _buckets[bucket].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void MoveFromBucket(int to, int bucket, int value)
    {
        StartVisualization();
        _elements[to].Value = value;
        _elements[to].FadeInSeconds(_timePerMove);
        _elements[to].Hidden = false;
        _buckets[bucket].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }
    
    public override void UndoMoveFromBucket(int to, int bucket)
    {
        StartVisualization();
        _elements[to].FadeOutSeconds(_timePerMove);
        _elements[to].Hidden = true;
        _buckets[bucket].HighlightForSeconds(_timePerMove);
        StartCoroutine(StopVisualizationAfterDelay());
    }

    public override void MarkCurrentSubset(int from, int to)
    {
        for (int i = 0; i < _size; ++i)
        {
            if (i >= from && i <= to)
            {
                _elements[i].MarkAsSubset();
            }
            else
            {
                _elements[i].MarkAsNotSubset();
            }
        }
    }
    
    public override void SetSwapsComparisons(int swaps, int comparisons)
    {
        string text = "";
        text += "<b>Swaps:</b> <pos=50%>" + swaps + "\n";
        text += "<b>Comparisons:</b> <pos=50%>" + comparisons;
        swapsText.text = text;
    }

    public override void DisplayPseudocode(List<string> pseudocode, int highlightLine, Dictionary<string, int> extraVars)
    {
        string highlightedCode = "";
        for (int i = 0; i < pseudocode.Count; ++i)
        {
            if (i == highlightLine)
            {
                highlightedCode += "<color=#810000>></color> " + pseudocode[i] + "\n";
            }
            else
            {
                highlightedCode += "  " + pseudocode[i] + "\n";
            }
        }

        if (extraVars != null)
        {
            foreach (var extraVar in extraVars)
            {
                if (extraVar.Value == -1)
                    break;
                if (extraVar.Key == "pInd")
                {
                    highlightedCode += "\n<style=\"sortingFunction\">" + "p" +
                                       "</style> : <style=\"sortingNumber\">" + GetElementValue(extraVar.Value) +
                                       "</style>";
                }
                else
                {
                    highlightedCode += "\n<style=\"sortingFunction\">" + extraVar.Key +
                                       "</style> : <style=\"sortingNumber\">" + extraVar.Value +
                                       "</style>";
                }
            }
        }

        pseudoCodeText.text = highlightedCode;
    }
    
    public override void SetDescription(string key)
    {
        descriptionText.text = LanguageManager.Instance.GetString(key);
    }
    
    public override void DisplayIndices(Dictionary<string, int> indices)
    {
        for (int i = 0; i < Size; ++i)
        {
            List<string> matches = new List<string>();
            foreach (var pair in indices)
            {
                if (pair.Value == i)
                {
                    matches.Add(pair.Key);
                }
            }
            _elements[i].SetIndexText(matches);
        }
    }

    private int GetElementValue(int index)
    {
        return _elements[index].Value;
    }

    public override void NewAlgorithmSelected()
    {
        base.NewAlgorithmSelected();
        HideBuckets();
    }

    protected override void FinishRunningVisualizations()
    {
        StopAllCoroutines();
        
        ResetValues();
        
        foreach (var bucket in _buckets)
        {
            bucket.ResetVisualization();
        }
        
        _visualizationActive = false;
    }

    public override void ResetVisualization()
    {
        FinishRunningVisualizations();
        foreach (var element in _elements)
        {
            element.ResetVisualization();
        }
    }

    private void ResetValues()
    {
        List<int> values = _sortingLogic.SortingValues;
        
        if (values.Count == 0)
            return;
        
        for(int i = 0; i < _elements.Count; ++i)
        {
            _elements[i].Value = values[i];
            _elements[i].FinishActiveVisualizations();
        }
    }

    public void ResetObject()
    {
        ResetVisualization();
    }
}
