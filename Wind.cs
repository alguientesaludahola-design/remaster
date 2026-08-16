using System;
using UnityEngine;

// Token: 0x0200005D RID: 93
public class Wind : MonoBehaviour
{
	// Token: 0x060007AC RID: 1964 RVA: 0x0006A870 File Offset: 0x00068A70
	private void Start()
	{
		this.randomseed = Random.Range(0, 10);
		if (MainManager.map != null && !this.owncenter)
		{
			this.center = MainManager.map.centralpoint;
		}
		base.gameObject.isStatic = false;
		if (MainManager.nowindeffect)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (this.origins == null)
		{
			this.origins = new Vector3?(base.transform.position);
		}
		if (this.trail == null)
		{
			this.trail = base.GetComponent<TrailRenderer>();
		}
		base.transform.position = new Vector3(this.origins.Value.x, this.origins.Value.y + Random.Range(-this.VerticalOffset, this.VerticalOffset), this.origins.Value.z + Random.Range(-this.horizontalOffset, this.horizontalOffset));
		this.start = base.transform.position;
		if (Mathf.Abs(this.limit) < 0.1f)
		{
			this.limit = base.transform.position.x * -1f;
		}
		this.left = (this.limit < 0f);
		this.trail.Clear();
		this.moving = true;
	}

	// Token: 0x060007AD RID: 1965 RVA: 0x0006A9D8 File Offset: 0x00068BD8
	private void FixedUpdate()
	{
		if (this.trail != null)
		{
			this.trail.enabled = (MainManager.instance.insideid == this.insideid);
		}
		if (this.moving)
		{
			base.transform.position = new Vector3(base.transform.position.x, this.start.y + Mathf.Sin((Time.time + (float)this.randomseed) * this.bobammount) * this.bobfrequency, this.start.z);
			if (this.left)
			{
				base.transform.position += Vector3.left * this.speed;
				if (base.transform.position.x < this.limit - 5f)
				{
					this.Stop();
					return;
				}
			}
			else
			{
				base.transform.position += Vector3.right * this.speed;
				if (base.transform.position.x > this.limit + 5f)
				{
					this.Stop();
					return;
				}
			}
		}
		else
		{
			if (this.randomammount > 0f)
			{
				this.randomammount -= 1f;
				return;
			}
			if (this.trail.endWidth <= 0.5f)
			{
				this.Start();
			}
		}
	}

	// Token: 0x060007AE RID: 1966 RVA: 0x0006AB4B File Offset: 0x00068D4B
	private void LateUpdate()
	{
		this.trail.material.renderQueue = 5000;
	}

	// Token: 0x060007AF RID: 1967 RVA: 0x0006AB62 File Offset: 0x00068D62
	private void Stop()
	{
		this.randomammount = Random.Range(60f * this.trail.time, this.randomizer);
		this.moving = false;
	}

	// Token: 0x04000800 RID: 2048
	public Vector3? origins;

	// Token: 0x04000801 RID: 2049
	public int insideid = -1;

	// Token: 0x04000802 RID: 2050
	public float randomizer = 250f;

	// Token: 0x04000803 RID: 2051
	public float limit;

	// Token: 0x04000804 RID: 2052
	public float bobammount = 1f;

	// Token: 0x04000805 RID: 2053
	public float bobfrequency = 2f;

	// Token: 0x04000806 RID: 2054
	public float horizontalOffset = 7.5f;

	// Token: 0x04000807 RID: 2055
	public float VerticalOffset = 3f;

	// Token: 0x04000808 RID: 2056
	public float speed = 0.3f;

	// Token: 0x04000809 RID: 2057
	private int randomseed;

	// Token: 0x0400080A RID: 2058
	private float randomammount;

	// Token: 0x0400080B RID: 2059
	private float limitorigin;

	// Token: 0x0400080C RID: 2060
	private float offset;

	// Token: 0x0400080D RID: 2061
	private bool left;

	// Token: 0x0400080E RID: 2062
	private bool moving = true;

	// Token: 0x0400080F RID: 2063
	private bool started;

	// Token: 0x04000810 RID: 2064
	public bool owncenter;

	// Token: 0x04000811 RID: 2065
	private Vector3 start;

	// Token: 0x04000812 RID: 2066
	public Vector3 center;

	// Token: 0x04000813 RID: 2067
	private TrailRenderer trail;
}
