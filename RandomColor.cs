using System;
using UnityEngine;

// Token: 0x0200004C RID: 76
public class RandomColor : MonoBehaviour
{
	// Token: 0x0600074B RID: 1867 RVA: 0x000650A4 File Offset: 0x000632A4
	private void LateUpdate()
	{
		if (this.flag == -1 || MainManager.instance.flags[this.flag])
		{
			if (this.type == RandomColor.Type.RandomFromList || this.type == RandomColor.Type.InOrder)
			{
				this.cooldown -= MainManager.TieFramerate(1f);
				if (this.cooldown <= 0f)
				{
					this.index++;
					if (this.index >= this.colors.Length)
					{
						this.index = 0;
					}
					for (int i = 0; i < this.renders.Length; i++)
					{
						if (this.type == RandomColor.Type.RandomFromList)
						{
							this.renders[i].material.color = this.colors[Random.Range(0, this.colors.Length)];
						}
						else if (this.type == RandomColor.Type.InOrder)
						{
							this.renders[i].material.color = this.colors[this.index];
						}
					}
					this.cooldown = this.frametime + Random.Range(0f, this.variant);
					return;
				}
			}
			else if (this.type == RandomColor.Type.Rainbow)
			{
				for (int j = 0; j < this.renders.Length; j++)
				{
					this.renders[j].material.color = MainManager.RainbowColor((int)this.variant);
				}
				return;
			}
		}
		else if (!this.isoff)
		{
			for (int k = 0; k < this.renders.Length; k++)
			{
				this.renders[k].material.color = this.offcolor;
			}
			if (this.setoffonce)
			{
				this.isoff = true;
			}
		}
	}

	// Token: 0x04000746 RID: 1862
	public RandomColor.Type type;

	// Token: 0x04000747 RID: 1863
	public Color[] colors;

	// Token: 0x04000748 RID: 1864
	public Color offcolor = Color.gray;

	// Token: 0x04000749 RID: 1865
	public MeshRenderer[] renders;

	// Token: 0x0400074A RID: 1866
	public int flag = -1;

	// Token: 0x0400074B RID: 1867
	public float speed;

	// Token: 0x0400074C RID: 1868
	public float frametime;

	// Token: 0x0400074D RID: 1869
	public float variant;

	// Token: 0x0400074E RID: 1870
	public bool setoffonce;

	// Token: 0x0400074F RID: 1871
	private bool isoff;

	// Token: 0x04000750 RID: 1872
	private float cooldown;

	// Token: 0x04000751 RID: 1873
	public int index;

	// Token: 0x02000278 RID: 632
	public enum Type
	{
		// Token: 0x04002101 RID: 8449
		RandomFromList,
		// Token: 0x04002102 RID: 8450
		Rainbow,
		// Token: 0x04002103 RID: 8451
		InOrder
	}
}
