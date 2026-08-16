using System;
using UnityEngine;

// Token: 0x02000008 RID: 8
[RequireComponent(typeof(BoxCollider))]
public class CamChange : MonoBehaviour
{
	// Token: 0x06000163 RID: 355 RVA: 0x00010980 File Offset: 0x0000EB80
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform == MainManager.player.transform)
		{
			if (this.offset.magnitude > 0f)
			{
				this.ogOffset = MainManager.instance.camoffset;
				MainManager.instance.camoffset = this.offset;
			}
			if (this.angle.magnitude > 0f)
			{
				this.ogAngle = MainManager.instance.camangleoffset;
				MainManager.instance.camangleoffset = this.angle;
			}
			if (this.speed > 0f)
			{
				this.ogSpd = MainManager.instance.camspeed;
				MainManager.instance.camspeed = this.speed;
			}
		}
	}

	// Token: 0x06000164 RID: 356 RVA: 0x00010A38 File Offset: 0x0000EC38
	private void OnTriggerExit(Collider other)
	{
		if (other.transform == MainManager.player.transform)
		{
			if (this.offset.magnitude > 0f)
			{
				MainManager.instance.camoffset = this.ogOffset;
			}
			if (this.angle.magnitude > 0f)
			{
				MainManager.instance.camangleoffset = this.ogAngle;
			}
			if (this.speed > 0f)
			{
				MainManager.instance.camspeed = this.ogSpd;
			}
		}
	}

	// Token: 0x040000CE RID: 206
	public Vector3 offset;

	// Token: 0x040000CF RID: 207
	public Vector3 angle;

	// Token: 0x040000D0 RID: 208
	public float speed;

	// Token: 0x040000D1 RID: 209
	private Vector3 ogOffset;

	// Token: 0x040000D2 RID: 210
	private Vector3 ogAngle;

	// Token: 0x040000D3 RID: 211
	private float ogSpd;
}
