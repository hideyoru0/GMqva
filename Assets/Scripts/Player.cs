using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public float speed;
	public GameObject[] weapons;
	public bool[] hasWeapons;
	public GameObject[] grenades;
	public int hasGrenades;
	public GameObject grenadeObj;
	public Camera followCamera;
	public GameManager manager;

	public int ammo;
	public int coin;
	public int health;
	public int score;

	public int maxAmmo;
	public int maxCoin;
	public int maxHealth;
	public int maxHasGrenades;

	float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
	bool fDown;
	bool gDown;
	bool rDown;
	bool iDown;
	bool sDown1;	//망치
	bool sDown2;	//권총
	bool sDown3;	//기관총

    bool isJump;
	bool isDodge;
	bool isSwap;
	bool isReload;
	bool isFireReady = true;
	bool isBorder;
	bool isDamage;
	bool isShop;
	bool isDead = false;

    Vector3 moveVec;
	Vector3 dodgeVec;

    Animator anim;
    Rigidbody rigid;
	MeshRenderer[] meshs;

	GameObject nearObject;
	public Weapon equipWeapon;
	int equipWeaponIndex = -1;
	float fireDelay;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();  //Player 자식 오브젝트에 있으므로
		meshs = GetComponentsInChildren<MeshRenderer>();
    }

    void Update() {
        GetInput();
        Move();
        Turn();
        Jump();
		Grenade();
		Attack();
		Reload();
		Dodge();
		Swap();
		Interation();
    }

    void GetInput() {
        hAxis = Input.GetAxisRaw("Horizontal");	//좌우 방향키
        vAxis = Input.GetAxisRaw("Vertical");	//상하 방향키
        wDown = Input.GetButton("Walk");	//shift 키
		jDown = Input.GetButtonDown("Jump");	//스페이스바
		fDown = Input.GetButton("Fire1");   //마우스 왼쪽 클릭
		gDown = Input.GetButtonDown("Fire2");   //마우스 오른쪽 클릭
		rDown = Input.GetButtonDown("Reload");	//R키
		iDown = Input.GetButtonDown("Interation");	//E키
		sDown1 = Input.GetButton("Swap1");	//번호 1번 키
		sDown2 = Input.GetButton("Swap2");	//번호 2번 키
		sDown3 = Input.GetButton("Swap3");	//번호 3번 키
	}

    void Move() {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;  //normalized : 방향 값이 1로 보정된 벡터

		if (isDodge)	//회피 중일때는
			moveVec = dodgeVec; //회피하는 중인 방향으로 유지

		if (isSwap || isReload || !isFireReady || isDead)	//무기 교체, 재장전, 공격 중, 사망일때는
			moveVec = Vector3.zero;	//멈추기

		if (!isBorder)
			transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

		anim.SetBool("isRun", (moveVec != Vector3.zero));   //이동을 멈추면
        anim.SetBool("isWalk", wDown);
    }

    void Turn() {
		//키보드로 회전
		transform.LookAt(transform.position + moveVec); //나아갈 방향 보기

		//마우스로 회전
		if (fDown && !isDead) {
			Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, 100)) {
				Vector3 nextVec = rayHit.point - transform.position;
				nextVec.y = 0;
				transform.LookAt(transform.position + nextVec);
			}
		}
    }

    void Jump() {
        if (jDown && (moveVec == Vector3.zero) && !isJump && !isDodge && !isSwap && !isDead) {	//움직이지 않고 점프
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
			anim.SetBool("isJump", true);
			anim.SetTrigger("doJump");
			isJump = true;
        }
    }

	void Grenade() {
		if (hasGrenades == 0)
			return;

		if (gDown && !isReload && !isSwap && !isDead) {
			Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, 100)) {
				Vector3 nextVec = rayHit.point - transform.position;
				nextVec.y = 10;

				GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
				Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
				rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
				rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

				hasGrenades--;
				grenades[hasGrenades].SetActive(false);
			}
		}
	}

	void Attack() {
		if (equipWeapon == null)
			return;

		fireDelay += Time.deltaTime;
		isFireReady = (equipWeapon.rate < fireDelay);

		if (fDown && isFireReady && !isDodge && !isSwap && !isReload && !isShop && !isDead) {
			equipWeapon.Use();
			anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
			fireDelay = 0;
		}
	}

	void Reload() {
		if (equipWeapon == null)
			return;

		if (equipWeapon.type == Weapon.Type.Melee)
			return;

		if (ammo == 0)
			return;

		if (rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead) {
			anim.SetTrigger("doReload");
			isReload = true;

			Invoke("ReloadOut", 3);
		}
	}

	void ReloadOut() { 
		int reAmmo = (ammo < equipWeapon.maxAmmo) ? ammo : equipWeapon.maxAmmo;
		equipWeapon.curAmmo = reAmmo;
		ammo -= reAmmo;
		isReload = false;
	}

	void Dodge() {
		if (jDown && (moveVec != Vector3.zero) && !isJump && !isDodge && !isSwap && !isDead) {    //이동하면서 점프
			dodgeVec = moveVec;
			speed *= 2;
			anim.SetTrigger("doDodge");
			isDodge = true;

			Invoke("DodgeOut", 0.4f);
		}
	}

	void DodgeOut() {
		speed *= 0.5f;
		isDodge = false;
	}

	void Swap() {
		if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))	return;
		if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))	return;
		if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))	return;

		int weaponIndex = -1;
		if (sDown1) weaponIndex = 0;
		if (sDown2) weaponIndex = 1;
		if (sDown3) weaponIndex = 2;

		if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isDead) {
			if (equipWeapon != null)
				equipWeapon.gameObject.SetActive(false);

			equipWeaponIndex = weaponIndex;
			equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
			equipWeapon.gameObject.SetActive(true);

			anim.SetTrigger("doSwap");
			isSwap = true;
			Invoke("SwapOut", 0.4f);
		}
	}

	void SwapOut() {
		isSwap = false;
	}

	void Interation() {
		if (iDown && nearObject != null && !isJump && !isDodge && !isDead) {
			if (nearObject.tag == "Weapon") {
				Item item = nearObject.GetComponent<Item>();
				int weaponIndex = item.value;
				hasWeapons[weaponIndex] = true;

				Destroy(nearObject);
			}
			else if(nearObject.tag == "Shop") {
				Shop shop = nearObject.GetComponent<Shop>();
				shop.Enter(this);
				isShop = true;
			}
		}
	}

	void FreezeRotation() {
		rigid.angularVelocity = Vector3.zero;
	}

	void StopToWall() {
		Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
		isBorder = Physics.Raycast(transform.position, moveVec, 5, LayerMask.GetMask("Wall"));
	}

	void FixedUpdate() {
		FreezeRotation();
		StopToWall();
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Floor") {
			anim.SetBool("isJump", false);
			isJump = false;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == "Item") {
			Item item = other.GetComponent<Item>();
			switch(item.type) {
				case Item.Type.Ammo:
					ammo += item.value;
					if (ammo > maxAmmo)
						ammo = maxAmmo;
					break;
				case Item.Type.Coin:
					coin += item.value;
					if (coin > maxCoin)
						coin = maxCoin;
					break;
				case Item.Type.Heart:
					health += item.value;
					if (health > maxHealth)
						health = maxHealth;
					break;
				case Item.Type.Grenade:
					if (hasGrenades == maxHasGrenades)
						return;

					grenades[hasGrenades].SetActive(true);
					hasGrenades += item.value;
					if (hasGrenades > maxHasGrenades)
						hasGrenades = maxHasGrenades;
					break;
			}
			Destroy(other.gameObject);
		}
		else if (other.tag == "EnemyBullet") {
			if (!isDamage) {
				Bullet enemyBullet = other.GetComponent<Bullet>();
				health -= enemyBullet.damage;


				bool isBossAtk = other.name == "Boss Melee Area";
				StartCoroutine(OnDamage(isBossAtk));
			}

			if (other.GetComponent<Rigidbody>() != null)    //Rigidbody 유무를 판단(미사일)
				Destroy(other.gameObject);
		}
	}

	IEnumerator OnDamage(bool isBossAtk) {
		isDamage = true;
		foreach(MeshRenderer mesh in meshs) {
			mesh.material.color = Color.yellow;
		}

		if (isBossAtk)
			rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

		if (health <= 0 && !isDead)
			OnDie();

		yield return new WaitForSeconds(1f);

		isDamage = false;

		foreach (MeshRenderer mesh in meshs) {
			mesh.material.color = Color.white;
		}

		if (isBossAtk)
			rigid.velocity = Vector3.zero;
	}

	void OnDie() {
		anim.SetTrigger("doDie");
		isDead = true;
		manager.GameOver();
	}

	void OnTriggerStay(Collider other) {
		if (other.tag == "Weapon" || other.tag == "Shop")
			nearObject = other.gameObject;
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == "Weapon")
			nearObject = null;
		else if (other.tag == "Shop") {
			Shop shop = nearObject.GetComponent<Shop>();
			shop.Exit();
			isShop = false;
			nearObject = null;
		}
	}
}