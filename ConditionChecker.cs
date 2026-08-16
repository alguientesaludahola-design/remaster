using System;
using UnityEngine;

// Token: 0x0200000D RID: 13
public class ConditionChecker : MonoBehaviour
{
	// Token: 0x0600018F RID: 399 RVA: 0x000131A8 File Offset: 0x000113A8
	private void Start()
	{
		this.startpos = base.transform.position;
		this.startangle = base.transform.eulerAngles;
		this.data = base.GetComponent<NPCControl>();
		if (!this.dontdelete && MainManager.CheckIfCanExist(this.requires, this.limit, this.regionID))
		{
			if (this.data != null)
			{
				this.data.entity.iskill = true;
			}
			else
			{
				if (base.GetComponent<Animator>() != null)
				{
					Object.Destroy(base.GetComponent<Animator>());
				}
				base.transform.position = new Vector3(0f, 9999f, 0f);
				base.gameObject.SetActive(false);
			}
		}
		if (this.spriteflagchange > -1 && MainManager.instance.flags[this.spriteflagchange])
		{
			SpriteRenderer component = base.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				component.sprite = this.spritetochange;
			}
		}
	}

	// Token: 0x06000190 RID: 400 RVA: 0x000132A4 File Offset: 0x000114A4
	private void LateUpdate()
	{
		if (this.livetime < 30f)
		{
			this.livetime += MainManager.TieFramerate(1f);
		}
		if (!MainManager.IsPaused() || this.pauseactive || this.livetime < 20f)
		{
			if (this.delay == 0)
			{
				if (this.activepos.magnitude > 0.1f && MainManager.CheckIfCanExist(this.requires, this.limit, this.regionID))
				{
					if (!this.worldpos)
					{
						base.transform.localPosition = this.activepos;
					}
					else
					{
						base.transform.position = this.activepos;
					}
				}
				this.delay = 5;
				return;
			}
			this.delay--;
		}
	}

	// Token: 0x0400010B RID: 267
	public int[] requires = new int[]
	{
		-1
	};

	// Token: 0x0400010C RID: 268
	public int[] limit = new int[]
	{
		-1
	};

	// Token: 0x0400010D RID: 269
	public int regionID = -1;

	// Token: 0x0400010E RID: 270
	public int spriteflagchange = -1;

	// Token: 0x0400010F RID: 271
	public Vector3 activepos;

	// Token: 0x04000110 RID: 272
	[HideInInspector]
	public Vector3 startpos;

	// Token: 0x04000111 RID: 273
	[HideInInspector]
	public Vector3 startangle;

	// Token: 0x04000112 RID: 274
	private int delay;

	// Token: 0x04000113 RID: 275
	public Sprite spritetochange;

	// Token: 0x04000114 RID: 276
	private float livetime;

	// Token: 0x04000115 RID: 277
	private NPCControl data;

	// Token: 0x04000116 RID: 278
	public bool dontdelete;

	// Token: 0x04000117 RID: 279
	public bool worldpos;

	// Token: 0x04000118 RID: 280
	public bool pauseactive;
}
