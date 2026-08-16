using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003F RID: 63
public class MidPos : MonoBehaviour
{
	// Token: 0x06000694 RID: 1684 RVA: 0x0004A480 File Offset: 0x00048680
	private void Start()
	{
		if (this.getfromchild)
		{
			this.GetTransform();
		}
		if (this.parenttransform != null)
		{
			if (this.ismodel)
			{
				this.entity = this.parenttransform.parent.parent.parent.GetComponent<EntityControl>();
				return;
			}
			this.entity = this.parenttransform.GetComponent<EntityControl>();
		}
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0004A4E4 File Offset: 0x000486E4
	private void GetTransform()
	{
		List<Transform> list = new List<Transform>(base.GetComponentsInChildren<Transform>());
		list.Remove(base.transform);
		this.links = list.ToArray();
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x0004A518 File Offset: 0x00048718
	private void LateUpdate()
	{
		if (this.getstartandendfromlink)
		{
			this.start = this.links[0].transform.position;
			this.end = this.links[this.links.Length - 1].transform.position;
			this.localpos = false;
		}
		for (int i = 0; i < this.links.Length; i++)
		{
			float t = (float)i / (float)(this.links.Length - 1);
			if (this.middle.magnitude > 0.1f)
			{
				Vector3 vector = this.middle;
				if (this.entity != null && this.entity.flip)
				{
					vector = new Vector3(-vector.x, vector.y, -vector.z);
				}
				Vector3 mid = Vector3.Lerp(this.localpos ? (base.transform.position + this.start) : this.start, this.localpos ? (base.transform.position + this.end) : this.end, 0.5f) + vector;
				this.links[i].position = MainManager.BeizierCurve3(this.localpos ? (base.transform.position + this.start) : this.start, this.localpos ? (base.transform.position + this.end) : this.end, mid, t);
			}
			else
			{
				this.links[i].position = Vector3.Lerp(this.localpos ? (base.transform.position + this.start) : this.start, this.localpos ? (base.transform.position + this.end) : this.end, t);
			}
		}
	}

	// Token: 0x040005E8 RID: 1512
	public Transform[] links;

	// Token: 0x040005E9 RID: 1513
	public Vector3 start;

	// Token: 0x040005EA RID: 1514
	public Vector3 end;

	// Token: 0x040005EB RID: 1515
	public Vector3 middle;

	// Token: 0x040005EC RID: 1516
	public bool localpos;

	// Token: 0x040005ED RID: 1517
	public bool getstartandendfromlink;

	// Token: 0x040005EE RID: 1518
	public bool getfromchild;

	// Token: 0x040005EF RID: 1519
	public bool ismodel;

	// Token: 0x040005F0 RID: 1520
	public Transform parenttransform;

	// Token: 0x040005F1 RID: 1521
	private EntityControl entity;
}
