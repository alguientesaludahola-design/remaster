using System;
using UnityEngine;

// Token: 0x0200004F RID: 79
public class RenderQueue : MonoBehaviour
{
	// Token: 0x06000755 RID: 1877 RVA: 0x000655A7 File Offset: 0x000637A7
	private void Start()
	{
		base.Invoke("Set", 0.1f);
	}

	// Token: 0x06000756 RID: 1878 RVA: 0x000655BC File Offset: 0x000637BC
	private void Set()
	{
		Renderer component = base.GetComponent<Renderer>();
		if (component != null)
		{
			if (this.materials.Length != 0)
			{
				for (int i = 0; i < this.materials.Length; i++)
				{
					component.materials[this.materials[i]].renderQueue = this.queue;
				}
			}
			else
			{
				for (int j = 0; j < component.materials.Length; j++)
				{
					component.materials[j].renderQueue = this.queue;
				}
			}
		}
		base.enabled = false;
	}

	// Token: 0x0400075C RID: 1884
	public int queue = 3000;

	// Token: 0x0400075D RID: 1885
	public int[] materials;
}
