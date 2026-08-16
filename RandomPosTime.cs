using System;
using UnityEngine;

// Token: 0x0200004D RID: 77
public class RandomPosTime : MonoBehaviour
{
	// Token: 0x0600074D RID: 1869 RVA: 0x00065259 File Offset: 0x00063459
	private void Start()
	{
		this.startpos = base.transform.position;
		if (this.animatorpart != null)
		{
			this.animatorpart.enabled = true;
		}
	}

	// Token: 0x0600074E RID: 1870 RVA: 0x00065288 File Offset: 0x00063488
	private void Update()
	{
		if (this.timer > 0f)
		{
			this.timer -= MainManager.TieFramerate(1f);
			return;
		}
		if (this.checkground)
		{
			RaycastHit raycastHit;
			Physics.Raycast(this.startpos + MainManager.RandomVector(this.size / 2f), Vector3.down, out raycastHit, 50f, 8448);
			if (raycastHit.transform != null)
			{
				base.transform.position = raycastHit.point;
			}
		}
		else
		{
			base.transform.position = this.startpos + MainManager.RandomVector(this.size * 0.5f);
		}
		if (this.animatorpart != null)
		{
			this.animatorpart.Play(this.animname);
		}
		this.timer = (this.randomizedtime ? Random.Range(this.frametime / 2f, this.frametime) : this.frametime);
	}

	// Token: 0x04000752 RID: 1874
	public Vector3 size;

	// Token: 0x04000753 RID: 1875
	private Vector3 startpos;

	// Token: 0x04000754 RID: 1876
	public float frametime;

	// Token: 0x04000755 RID: 1877
	private float timer;

	// Token: 0x04000756 RID: 1878
	public bool checkground;

	// Token: 0x04000757 RID: 1879
	public bool randomizedtime;

	// Token: 0x04000758 RID: 1880
	public Animator animatorpart;

	// Token: 0x04000759 RID: 1881
	public string animname;
}
