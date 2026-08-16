using System;
using UnityEngine;

// Token: 0x0200000F RID: 15
public class CrackRockBreak : MonoBehaviour
{
	// Token: 0x06000197 RID: 407 RVA: 0x000135C8 File Offset: 0x000117C8
	private void Start()
	{
		base.transform.parent = ((MainManager.battle != null && MainManager.battle.battlemap != null) ? MainManager.battle.battlemap.transform : MainManager.map.transform);
		this.r = base.GetComponentsInChildren<Renderer>();
		Rigidbody[] componentsInChildren = base.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.r[i].material.color = this.initialcolor;
			componentsInChildren[i].AddTorque(MainManager.RandomVector(0.5f));
			componentsInChildren[i].velocity = MainManager.RandomItemBounce(3f, 5f);
		}
	}

	// Token: 0x06000198 RID: 408 RVA: 0x0001367C File Offset: 0x0001187C
	private void Update()
	{
		if (this.timer > 0f)
		{
			this.timer -= MainManager.TieFramerate(1f);
			return;
		}
		if (this.alpha > 0f)
		{
			for (int i = 0; i < this.r.Length; i++)
			{
				this.r[i].materials[0].color = Color.Lerp(this.initialcolor, Color.clear, 1f - this.alpha / 60f);
				this.r[i].materials[1].color = Color.Lerp(Color.black, Color.clear, 1f - this.alpha / 60f);
			}
			this.alpha -= MainManager.TieFramerate(1f);
			return;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x04000123 RID: 291
	public Color initialcolor;

	// Token: 0x04000124 RID: 292
	private float alpha = 60f;

	// Token: 0x04000125 RID: 293
	private float timer = 60f;

	// Token: 0x04000126 RID: 294
	private Renderer[] r;
}
