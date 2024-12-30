using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public int damage;
	public bool isMelee;
	public bool isRock;

	void OnCollisionEnter(Collision collision) {
		if (!isRock && collision.gameObject.tag == "Floor")    //탄피
			Destroy(gameObject, 3); //3초 뒤에 사라지기
	}

	void OnTriggerEnter(Collider other) {	//총알을 isTrigger로 바꾸었으므로
		if(!isMelee && other.gameObject.tag == "Wall")   //총알
			Destroy(gameObject);
	}
}