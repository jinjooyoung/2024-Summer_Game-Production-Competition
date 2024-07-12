using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stuff : MonoBehaviour
{
    public enum StuffType
    {
        // 손질된
        PrepIngredients,
        // 손질 안된
        NotPrepared
    }
    
    public StuffType stuffType; // 현재 재료의 상태
}
