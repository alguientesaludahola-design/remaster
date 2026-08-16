using System;
using UnityEngine;

// Token: 0x0200001E RID: 30
public class FakeWall : MonoBehaviour
{
	// Token: 0x06000381 RID: 897 RVA: 0x00023ADC File Offset: 0x00021CDC
	private void OnTriggerEnter(Collider other)
	{
		if (MainManager.player != null && other.transform == MainManager.player.transform && !this.inside)
		{
			this.inside = true;
			MainManager.PlaySound("Glow", -1, 0.8f, 1f);
		}
	}

	// Token: 0x06000382 RID: 898 RVA: 0x00023B32 File Offset: 0x00021D32
	private void OnTriggerExit(Collider other)
	{
		if (MainManager.player != null && other.transform == MainManager.player.transform && this.inside)
		{
			this.inside = false;
		}
	}

	// Token: 0x040002AC RID: 684
	private bool inside;
}
