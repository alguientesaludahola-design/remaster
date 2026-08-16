using System;
using UnityEngine;

// Token: 0x0200001A RID: 26
public class ExpBag : MonoBehaviour
{
	// Token: 0x06000372 RID: 882 RVA: 0x00022855 File Offset: 0x00020A55
	private void Start()
	{
		base.transform.parent.parent.GetChild(1).GetComponent<SkinnedMeshRenderer>().materials[1].SetFloat("_Outline", 0.1f);
	}

	// Token: 0x06000373 RID: 883 RVA: 0x00022888 File Offset: 0x00020A88
	private void LateUpdate()
	{
		if (!this.overridescale)
		{
			float num = (float)MainManager.instance.partyexp / (float)MainManager.instance.neededexp;
			base.transform.localScale = new Vector3(Mathf.Lerp(0.25f, 1.15f, num), Mathf.Lerp(1f, 1.25f, num), base.transform.localScale.z);
			if (num >= 0.75f)
			{
				if (this.t == null)
				{
					this.t = base.transform.parent.gameObject.AddComponent<SpriteBounce>();
					this.t.frequency = 0.01f;
					this.t.maxx = 0.1f;
					this.t.maxy = 1f;
					this.t.speed = 20f;
				}
				this.t.basescale = base.transform.localScale;
			}
		}
	}

	// Token: 0x04000281 RID: 641
	public SpriteBounce t;

	// Token: 0x04000282 RID: 642
	public bool overridescale;
}
