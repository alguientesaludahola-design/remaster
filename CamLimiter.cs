using System;
using UnityEngine;

// Token: 0x02000009 RID: 9
public class CamLimiter : MonoBehaviour
{
	// Token: 0x06000166 RID: 358 RVA: 0x00010AC0 File Offset: 0x0000ECC0
	private void Start()
	{
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(this.sizex, 50f, this.sizez);
		boxCollider.isTrigger = true;
		Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeAll;
	}

	// Token: 0x06000167 RID: 359 RVA: 0x00010B1A File Offset: 0x0000ED1A
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform == MainManager.MainCamera.transform.parent)
		{
			this.lastcampoint = MainManager.MainCamera.transform.parent.position;
		}
	}

	// Token: 0x06000168 RID: 360 RVA: 0x00010B52 File Offset: 0x0000ED52
	private void OnTriggerStay(Collider other)
	{
		if (other.transform == MainManager.MainCamera.transform.parent)
		{
			MainManager.PushAway(other.transform, this.lastcampoint);
		}
	}

	// Token: 0x040000D4 RID: 212
	public float sizex = 10f;

	// Token: 0x040000D5 RID: 213
	public float sizez = 5f;

	// Token: 0x040000D6 RID: 214
	private Vector3 lastcampoint;
}
