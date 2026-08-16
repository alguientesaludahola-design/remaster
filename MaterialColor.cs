using System;
using UnityEngine;

// Token: 0x0200003C RID: 60
public class MaterialColor : MonoBehaviour
{
	// Token: 0x06000664 RID: 1636 RVA: 0x0004777C File Offset: 0x0004597C
	private void Start()
	{
		if (this.settag)
		{
			base.tag = "NoMapColor";
		}
		if (this.render == null)
		{
			this.render = base.GetComponent<Renderer>();
		}
		this.render.materials[this.materialid].color = this.color;
	}

	// Token: 0x040005B9 RID: 1465
	public Color color;

	// Token: 0x040005BA RID: 1466
	public int materialid;

	// Token: 0x040005BB RID: 1467
	public bool settag;

	// Token: 0x040005BC RID: 1468
	private Renderer render;
}
