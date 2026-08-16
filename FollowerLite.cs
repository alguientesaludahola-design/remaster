using System;
using UnityEngine;

// Token: 0x02000026 RID: 38
public class FollowerLite : MonoBehaviour
{
	// Token: 0x060003BF RID: 959 RVA: 0x0002751E File Offset: 0x0002571E
	private void Start()
	{
		base.gameObject.layer = 9;
		base.gameObject.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		base.gameObject.AddComponent<BoxCollider>().center = new Vector3(0f, 0.5f);
	}

	// Token: 0x060003C0 RID: 960 RVA: 0x0002755E File Offset: 0x0002575E
	public void SetUp(float dist, Transform following, float spd)
	{
		this.distance = dist;
		this.parent = following;
		this.speed = spd;
	}

	// Token: 0x060003C1 RID: 961 RVA: 0x00027578 File Offset: 0x00025778
	private void LateUpdate()
	{
		if (this.parent != null)
		{
			if (this.delaycount <= 0)
			{
				if (!this.parent.gameObject.activeInHierarchy)
				{
					base.gameObject.SetActive(false);
				}
				float num = Vector3.Distance(base.transform.position, this.parent.position);
				if (num > this.tpdistance)
				{
					base.transform.position = this.parent.position;
				}
				this.inrange = (num > this.distance);
				this.delaycount = 2;
			}
			else
			{
				this.delaycount--;
			}
		}
		if (this.inrange)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, this.parent.position, MainManager.TieFramerate(this.speed));
		}
	}

	// Token: 0x0400033B RID: 827
	private float distance = 1.5f;

	// Token: 0x0400033C RID: 828
	private float speed = 0.025f;

	// Token: 0x0400033D RID: 829
	private float tpdistance = 5f;

	// Token: 0x0400033E RID: 830
	private Transform parent;

	// Token: 0x0400033F RID: 831
	private const int delay = 2;

	// Token: 0x04000340 RID: 832
	private int delaycount;

	// Token: 0x04000341 RID: 833
	private bool inrange;
}
