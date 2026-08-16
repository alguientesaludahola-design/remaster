using System;
using UnityEngine;

// Token: 0x02000035 RID: 53
public class KeepAngle : MonoBehaviour
{
	// Token: 0x0600042A RID: 1066 RVA: 0x0002AF09 File Offset: 0x00029109
	private void Start()
	{
		if (this.getatstart)
		{
			this.angle = base.transform.eulerAngles;
		}
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x0002AF24 File Offset: 0x00029124
	private void LateUpdate()
	{
		base.transform.eulerAngles = this.angle;
	}

	// Token: 0x040003E0 RID: 992
	public bool getatstart;

	// Token: 0x040003E1 RID: 993
	public Vector3 angle;
}
