using System;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class LightSorter : MonoBehaviour
{
	// Token: 0x06000433 RID: 1075 RVA: 0x0002B250 File Offset: 0x00029450
	private void Start()
	{
		this.meshes = base.gameObject.GetComponentsInChildren<MeshRenderer>();
	}

	// Token: 0x06000434 RID: 1076 RVA: 0x0002B263 File Offset: 0x00029463
	private void LateUpdate()
	{
		if (Time.frameCount % 2 == 0)
		{
			MainManager.SortLights(this.meshes);
		}
	}

	// Token: 0x040003F1 RID: 1009
	private MeshRenderer[] meshes;
}
