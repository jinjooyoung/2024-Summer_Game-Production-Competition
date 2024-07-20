using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public enum PlateType
    {
        // 깨끗한
        Clean,
        // 더러운
        Dirty
    }

    public PlateType plateType;
}
