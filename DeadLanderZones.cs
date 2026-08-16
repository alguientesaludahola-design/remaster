using System;
using UnityEngine;

// Token: 0x02000011 RID: 17
public class DeadLanderZones : MonoBehaviour
{
	// Token: 0x060001A6 RID: 422 RVA: 0x00014794 File Offset: 0x00012994
	private void OnTriggerEnter(Collider other)
	{
		if (this.onlyrock)
		{
			if (other.tag == "PushRock")
			{
				DeadLanderOmega.GetOmega(this.id).ForceLook(this.setoffset);
				return;
			}
		}
		else if (MainManager.player != null && MainManager.player.transform == other.transform)
		{
			DeadLanderOmega.activeid = this.id;
		}
	}

	// Token: 0x0400014D RID: 333
	public bool onlyrock;

	// Token: 0x0400014E RID: 334
	public Vector3 setoffset;

	// Token: 0x0400014F RID: 335
	public int id;
}
