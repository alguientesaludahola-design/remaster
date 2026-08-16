using System;
using UnityEngine;

// Token: 0x0200005B RID: 91
public class VenusBattle : MonoBehaviour
{
	// Token: 0x0600079D RID: 1949 RVA: 0x000699BB File Offset: 0x00067BBB
	public void SetUp(EntityControl parent, EntityControl targetentity)
	{
		this.entity = parent;
		this.target = targetentity;
	}

	// Token: 0x0600079E RID: 1950 RVA: 0x000699CC File Offset: 0x00067BCC
	private void LateUpdate()
	{
		if (this.entity != null && this.target != null && !this.entity.overrideanim)
		{
			MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
			if (animIDs == MainManager.AnimIDs.Venus)
			{
				int animstate = this.target.animstate;
				if (animstate != 11)
				{
					this.entity.animstate = 0;
				}
				else
				{
					this.entity.animstate = this.target.animstate;
				}
				this.target.talking = this.entity.talking;
				return;
			}
			if (animIDs != MainManager.AnimIDs.SandWyrm)
			{
				if (animIDs != MainManager.AnimIDs.SandWyrmTail)
				{
					return;
				}
				this.entity.anim.speed = this.target.anim.speed;
				this.entity.animspeed = this.target.animspeed;
			}
			if (this.entity.animstate != 14)
			{
				int animstate = this.target.animstate;
				if (animstate != 0)
				{
					if (animstate == 11)
					{
						this.entity.animstate = this.target.animstate;
						return;
					}
				}
				else if (!MainManager.battle.action)
				{
					this.entity.animstate = 0;
				}
			}
		}
	}

	// Token: 0x040007EA RID: 2026
	private EntityControl entity;

	// Token: 0x040007EB RID: 2027
	private EntityControl target;
}
V
