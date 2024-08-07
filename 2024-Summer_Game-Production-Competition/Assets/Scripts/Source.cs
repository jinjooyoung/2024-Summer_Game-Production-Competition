using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Source : MonoBehaviour
{
    public enum SourceType
    {
        // 캐찹
        Ketchup,
        // 마요네즈
        Myonnaise,
        // 머스타드
        Mustard,
        // 아무것도 없음
        Nothing
    }

    public SourceType sourceType;
    public bool isDepleted = false;
}
