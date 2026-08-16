using System;
using UnityEngine;

// Token: 0x0200001B RID: 27
public class FaceCamera : MonoBehaviour
{
	// Token: 0x06000375 RID: 885 RVA: 0x00022980 File Offset: 0x00020B80
	private void Start()
	{
		if (this.angle)
		{
			this.rotater = new GameObject().transform;
			this.rotater.transform.parent = base.transform;
			this.rotater.transform.localPosition = Vector3.zero;
		}
		this.startsize = base.transform.localScale;
	}

	// Token: 0x06000376 RID: 886 RVA: 0x000229E4 File Offset: 0x00020BE4
	private void FixedUpdate()
	{
		if (this.angle)
		{
			if (this.rotater != null)
			{
				this.rotater.LookAt(MainManager.MainCamera.transform);
				base.transform.localScale = new Vector3(-this.startsize.x, this.startsize.y, -this.startsize.z);
				base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, this.rotater.eulerAngles.y + this.offset, base.transform.localEulerAngles.z);
			}
			return;
		}
		if (this.billboard)
		{
			base.transform.LookAt(MainManager.MainCamera.transform);
			return;
		}
		base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, MainManager.MainCamera.transform.parent.localEulerAngles.y + this.offset, base.transform.localEulerAngles.z);
	}

	// Token: 0x04000283 RID: 643
	public float offset;

	// Token: 0x04000284 RID: 644
	public bool angle;

	// Token: 0x04000285 RID: 645
	public bool billboard;

	// Token: 0x04000286 RID: 646
	private Transform rotater;

	// Token: 0x04000287 RID: 647
	private Vector3 startsize;
}
