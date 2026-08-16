using System;
using UnityEngine;

// Token: 0x02000039 RID: 57
public class LilypadEffects : MonoBehaviour
{
	// Token: 0x06000436 RID: 1078 RVA: 0x0002B27C File Offset: 0x0002947C
	private void Start()
	{
		if (MainManager.particlelevel == 0 || MainManager.map.areaid != MainManager.Areas.WildGrasslands)
		{
			base.enabled = false;
		}
		if (!this.self)
		{
			Transform transform = Object.Instantiate<GameObject>(Resources.Load("Prefabs/Particles/LilypadMove") as GameObject).transform;
			transform.parent = base.transform;
			transform.localPosition = new Vector3(0f, 0f, -0.025f);
			transform.localEulerAngles = new Vector3(90f, 0f);
			transform.localScale = Vector3.one;
			transform.GetComponent<ParticleSystemRenderer>().sharedMaterial.renderQueue = 2000;
			base.transform.localPosition = LilypadEffects.offset;
		}
	}

	// Token: 0x040003F2 RID: 1010
	public static readonly Vector3 offset = new Vector3(0f, 0.1f);

	// Token: 0x040003F3 RID: 1011
	public bool self;

	// Token: 0x040003F4 RID: 1012
	private float cd;

	// Token: 0x040003F5 RID: 1013
	private Vector3 sp;
}
