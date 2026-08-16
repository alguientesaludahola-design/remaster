using System;
using UnityEngine;

// Token: 0x0200002A RID: 42
public class GlowingAura : MonoBehaviour
{
	// Token: 0x060003D9 RID: 985 RVA: 0x00027FEA File Offset: 0x000261EA
	private void Start()
	{
		if (base.GetComponent<FaceCamera>() == null)
		{
			base.gameObject.AddComponent<FaceCamera>();
		}
		if (this.halos.Length == 0)
		{
			this.halos = base.GetComponentsInChildren<SpriteRenderer>();
		}
	}

	// Token: 0x060003DA RID: 986 RVA: 0x0002801C File Offset: 0x0002621C
	private void FixedUpdate()
	{
		for (int i = 0; i < this.halos.Length; i++)
		{
			switch (i)
			{
			case 0:
				this.halos[i].transform.localEulerAngles += new Vector3(0f, 0f, this.speed);
				break;
			case 1:
				this.halos[i].transform.localEulerAngles += new Vector3(0f, 0f, -this.speed);
				break;
			case 2:
				this.halos[i].transform.Rotate(this.speed, this.speed / 1.5f, this.speed / 2f);
				break;
			}
			if (this.alphavariation > 0f)
			{
				this.halos[i].color = new Color(this.color.r, this.color.g, this.color.b, Mathf.Lerp(this.color.a, this.alphavariation, Mathf.Abs(Mathf.Sin(this.speed * Time.time) * this.frequency)));
			}
		}
	}

	// Token: 0x0400035E RID: 862
	public SpriteRenderer[] halos;

	// Token: 0x0400035F RID: 863
	public float speed;

	// Token: 0x04000360 RID: 864
	public float frequency;

	// Token: 0x04000361 RID: 865
	public float alphavariation;

	// Token: 0x04000362 RID: 866
	public Color color = Color.white;
}
