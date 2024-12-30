using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : Bullet {
	Rigidbody rigid;
	float angularPower = 2;
	float scaleValue = 0.1f;
	bool isShoot;

	void Awake() {
		rigid = GetComponent<Rigidbody>();
		StartCoroutine(GainPowerTimer());
		StartCoroutine(GainPower());
	}

	IEnumerator GainPowerTimer() {	//발사 타이밍 제어
		yield return new WaitForSeconds(2.2f);
		isShoot = true;
	}

	IEnumerator GainPower() {	//발사 전 기 모으기
		while (!isShoot) {
			angularPower += 0.02f;
			scaleValue += 0.005f;
			if (scaleValue < 1)
				transform.localScale = Vector3.one * scaleValue;
			rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);	//Acceleration : 가속도 형태?
			yield return null;	//while문 안에 넣지 않으면 게임 정지 문제 발생 가능
		}
	}
}