/*
 * @authors: liangjian
 * @desc:	
*/
using UnityEngine;
using System.Collections;
using System;
 
public class SortAngle : IComparable<SortAngle> {
    public int Index;
    public float Angle;
    public int CompareTo(SortAngle item)
    {
        return item.Angle.CompareTo(Angle);
    }
}