using System;
using UnityEngine;

// Token: 0x02000043 RID: 67
public class OmegaHand : MonoBehaviour
{
	// Token: 0x060006EB RID: 1771 RVA: 0x0005922C File Offset: 0x0005742C
	private void Start()
	{
		this.anim = base.GetComponent<Animator>();
		this.b = base.GetComponentInChildren<Collider>();
		this.rock = MainManager.GetEntity(this.rockid).npcdata;
		this.arm = base.GetComponent<LineRenderer>();
		this.startp = base.transform.position;
		this.arm.useWorldSpace = true;
		for (int i = 0; i < this.arm.positionCount; i++)
		{
			this.arm.SetPosition(i, this.startp + this.arm.GetPosition(i));
		}
		this.armbase = this.arm.GetPosition(this.arm.positionCount - 1);
	}

	// Token: 0x060006EC RID: 1772 RVA: 0x000592E8 File Offset: 0x000574E8
	private void Update()
	{
		if (this.hidden)
		{
			if (this.cd < 15f)
			{
				this.cd += MainManager.framestep;
			}
			base.transform.position = MainManager.SmoothLerp(this.tpos, this.hidepos, this.cd / 15f);
			return;
		}
		if (this.tiedeye.forcelook)
		{
			switch (this.state)
			{
			case 0:
				if (this.cd == 0f)
				{
					this.anim.CrossFadeInFixedTime("HandOpen", 1.5f);
					this.b.enabled = false;
				}
				this.cd += MainManager.framestep * (float)(this.Detected() ? 5 : 1);
				if (this.cd < 300f)
				{
					base.transform.position = MainManager.BeizierCurve3(this.startp, this.rock.transform.position - this.rockoffset, 7.5f, this.cd / 300f);
					return;
				}
				this.rock.dummy = true;
				this.rock.transform.parent = base.transform;
				this.rock.entity.LockRigid(true);
				this.rock.boxcol.enabled = false;
				this.b.enabled = true;
				this.hpos = base.transform.position;
				this.tpos = base.transform.position + Vector3.up * 5f;
				this.state++;
				this.cd = 0f;
				this.anim.CrossFadeInFixedTime("HandHold", 0.65f);
				return;
			case 1:
				if (this.cd < 200f)
				{
					this.cd += MainManager.framestep * (float)(this.Detected() ? 5 : 1);
					base.transform.position = MainManager.SmoothLerp(this.hpos, this.tpos, this.cd / 200f);
					return;
				}
				this.state++;
				return;
			case 2:
				if (this.Detected())
				{
					this.hidden = true;
					this.tpos = base.transform.position;
					this.cd = 0f;
				}
				break;
			default:
				return;
			}
		}
	}

	// Token: 0x060006ED RID: 1773 RVA: 0x00059552 File Offset: 0x00057752
	private bool Detected()
	{
		return DeadLanderOmega.detected && DeadLanderOmega.activeid == this.tiedeye.thisid;
	}

	// Token: 0x060006EE RID: 1774 RVA: 0x00059570 File Offset: 0x00057770
	private void LateUpdate()
	{
		this.arm.SetPosition(0, base.transform.position);
		for (int i = 1; i < this.arm.positionCount; i++)
		{
			this.arm.SetPosition(i, MainManager.BeizierCurve3(base.transform.position, this.armbase, this.armheight, (float)i / (float)(this.arm.positionCount - 1)));
		}
	}

	// Token: 0x04000677 RID: 1655
	public DeadLanderOmega tiedeye;

	// Token: 0x04000678 RID: 1656
	public int rockid;

	// Token: 0x04000679 RID: 1657
	public Vector3 rockoffset;

	// Token: 0x0400067A RID: 1658
	public Vector3 hidepos;

	// Token: 0x0400067B RID: 1659
	public float armheight;

	// Token: 0x0400067C RID: 1660
	private bool hidden;

	// Token: 0x0400067D RID: 1661
	private Animator anim;

	// Token: 0x0400067E RID: 1662
	private Collider b;

	// Token: 0x0400067F RID: 1663
	private NPCControl rock;

	// Token: 0x04000680 RID: 1664
	private LineRenderer arm;

	// Token: 0x04000681 RID: 1665
	private Vector3 armbase;

	// Token: 0x04000682 RID: 1666
	private Vector3 startp;

	// Token: 0x04000683 RID: 1667
	private Vector3 tpos;

	// Token: 0x04000684 RID: 1668
	private Vector3 hpos;

	// Token: 0x04000685 RID: 1669
	private int state;

	// Token: 0x04000686 RID: 1670
	private float cd;
}
