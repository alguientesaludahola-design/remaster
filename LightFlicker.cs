using System;
using UnityEngine;

// Token: 0x02000037 RID: 55
public class LightFlicker : MonoBehaviour
{
	// Token: 0x06000430 RID: 1072 RVA: 0x0002B092 File Offset: 0x00029292
	private void Start()
	{
		this.light = base.GetComponent<Light>();
		this.startintensity = this.light.intensity;
		this.cooldown = this.frequency + Random.Range(-this.random, this.random);
	}

	// Token: 0x06000431 RID: 1073 RVA: 0x0002B0D0 File Offset: 0x000292D0
	private void Update()
	{
		this.cooldown -= MainManager.framestep;
		if (this.cooldown >= 0f)
		{
			if (this.framecount <= 1 || Time.frameCount % this.framecount == 0)
			{
				this.light.intensity = Mathf.Lerp(this.light.intensity, this.startintensity, this.speed * MainManager.framestep);
			}
			return;
		}
		if (this.cooldown > -this.duration)
		{
			this.light.intensity = Mathf.Lerp(this.light.intensity, this.targetintensity, this.speed * MainManager.framestep);
			return;
		}
		if (Random.Range(0f, 100f) < this.fastflickerpercent && !this.flickered)
		{
			this.cooldown = (this.frequency + Random.Range(-this.random, this.random)) / this.fastdivider;
			this.flickered = true;
			return;
		}
		this.cooldown = this.frequency + Random.Range(-this.random, this.random);
		this.flickered = false;
	}

	// Token: 0x040003E5 RID: 997
	private Light light;

	// Token: 0x040003E6 RID: 998
	public int framecount = 2;

	// Token: 0x040003E7 RID: 999
	public float frequency = 700f;

	// Token: 0x040003E8 RID: 1000
	public float random = 150f;

	// Token: 0x040003E9 RID: 1001
	public float duration = 20f;

	// Token: 0x040003EA RID: 1002
	public float speed = 0.05f;

	// Token: 0x040003EB RID: 1003
	public float fastflickerpercent = 5f;

	// Token: 0x040003EC RID: 1004
	public float fastdivider = 3f;

	// Token: 0x040003ED RID: 1005
	public float targetintensity;

	// Token: 0x040003EE RID: 1006
	private float cooldown;

	// Token: 0x040003EF RID: 1007
	private float startintensity;

	// Token: 0x040003F0 RID: 1008
	private bool flickered;
}
