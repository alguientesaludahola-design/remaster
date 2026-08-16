using System;
using UnityEngine;

// Token: 0x02000018 RID: 24
public class EntityTie : MonoBehaviour
{
	// Token: 0x06000269 RID: 617 RVA: 0x00020E50 File Offset: 0x0001F050
	private void LateUpdate()
	{
		if (this.delay > 0f)
		{
			this.delay -= MainManager.TieFramerate(1f);
			return;
		}
		if (MainManager.map.entities.Length > this.entityid && MainManager.map.entities[this.entityid] != null && MainManager.map.entities[this.entityid].gameObject.activeInHierarchy)
		{
			MainManager.map.entities[this.entityid].transform.parent = base.transform;
			MainManager.map.entities[this.entityid].alwaysactive = true;
			MainManager.map.entities[this.entityid].transform.localPosition = this.offset;
			if (this.angle.magnitude > 0.1f)
			{
				MainManager.map.entities[this.entityid].transform.localEulerAngles = this.angle;
			}
			base.enabled = false;
		}
	}

	// Token: 0x0400026E RID: 622
	public int entityid;

	// Token: 0x0400026F RID: 623
	public float delay = 4f;

	// Token: 0x04000270 RID: 624
	public Vector3 offset;

	// Token: 0x04000271 RID: 625
	public Vector3 angle;
}
