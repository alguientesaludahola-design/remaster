using System;
using UnityEngine;

// Token: 0x02000034 RID: 52
public class InsideSorter : MonoBehaviour
{
	// Token: 0x06000427 RID: 1063 RVA: 0x0000448F File Offset: 0x0000268F
	private void Start()
	{
	}

	// Token: 0x06000428 RID: 1064 RVA: 0x0002AE7C File Offset: 0x0002907C
	private void LateUpdate()
	{
		if (this.oldid != MainManager.instance.insideid)
		{
			if (!this.invert)
			{
				this.child.SetActive(MainManager.instance.insideid == this.insideid);
			}
			else
			{
				this.child.SetActive(MainManager.instance.insideid != this.insideid);
			}
			this.oldid = MainManager.instance.insideid;
		}
	}

	// Token: 0x040003DC RID: 988
	public int insideid = -1;

	// Token: 0x040003DD RID: 989
	private int oldid = -2;

	// Token: 0x040003DE RID: 990
	public bool invert;

	// Token: 0x040003DF RID: 991
	public GameObject child;
}
