using System;
using UnityEngine;

// Token: 0x0200004B RID: 75
public class PulsingLight : MonoBehaviour
{
	// Token: 0x06000748 RID: 1864 RVA: 0x0006506A File Offset: 0x0006326A
	private void Start()
	{
		this.light = base.GetComponent<Light>();
	}

	// Token: 0x06000749 RID: 1865 RVA: 0x00065078 File Offset: 0x00063278
	private void LateUpdate()
	{
		this.light.range = this.basevalue + Mathf.Sin(Time.time * this.speed) * this.ammount;
	}

	// Token: 0x04000742 RID: 1858
	private Light light;

	// Token: 0x04000743 RID: 1859
	public float basevalue;

	// Token: 0x04000744 RID: 1860
	public float speed;

	// Token: 0x04000745 RID: 1861
	public float ammount;
}
