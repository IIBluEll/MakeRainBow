using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //단일 인스턴스를 저장할 정적 변수
    private static T instance;

    // 단일 인스턴스에 접근할 수 있는 정적 속성
    public static T Instance
    {
        get
        {
            // 인스턴스가 아직 생성되지 않았다면
            if (instance == null)
            {
                // T 타입의 객체를 찾아서 인스턴스에 할당
                instance = FindObjectOfType<T>();
            }

            // 찾은 인스턴스를 반환
            return instance;
        }
    }

}
