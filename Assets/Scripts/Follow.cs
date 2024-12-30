//Follow 스크립트 파일

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {
    public Transform target;    //따라갈 목표
    public Vector3 offset;  //위치 오프셋

    void Update() {
        transform.position = target.position + offset;  //카메라 위치 = 기존 위치 + 목표 위치
    }
}