using System;
using UnityEngine;

// Token: 0x02000014 RID: 20
public class DummyControl : MonoBehaviour
{
	// Token: 0x060001B2 RID: 434 RVA: 0x0000448F File Offset: 0x0000268F
	private void Start()
	{
	}

	// Token: 0x060001B3 RID: 435 RVA: 0x00014AC4 File Offset: 0x00012CC4
	private void FixedUpdate()
	{
		base.transform.eulerAngles += this.spin;
		if (this.scalespeed > -1f)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.targetscale, this.scalespeed);
		}
		if (this.alivetime > -1f)
		{
			this.alivetime = Mathf.Clamp(this.alivetime - 1f, 0f, float.PositiveInfinity);
			if (this.alivetime == 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x00014B67 File Offset: 0x00012D67
	private void OnTriggerEnter(Collider other)
	{
		if (this.type == DummyControl.Type.Icecle && other.tag == "Ground")
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x0400015F RID: 351
	public DummyControl.Type type;

	// Token: 0x04000160 RID: 352
	public Vector3 spin;

	// Token: 0x04000161 RID: 353
	public Vector3 targetscale;

	// Token: 0x04000162 RID: 354
	public float scalespeed = -1f;

	// Token: 0x04000163 RID: 355
	public Rigidbody rigid;

	// Token: 0x04000164 RID: 356
	public float alivetime = -1f;

	// Token: 0x020000D8 RID: 216
	public enum Type
	{
		// Token: 0x04000E02 RID: 3586
		None,
		// Token: 0x04000E03 RID: 3587
		Icecle
	}
}
