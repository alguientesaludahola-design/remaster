using System;
using UnityEngine;

// Token: 0x02000054 RID: 84
public class SpinAround : MonoBehaviour
{
	// Token: 0x06000769 RID: 1897 RVA: 0x00066478 File Offset: 0x00064678
	private void Start()
	{
		if (this.gettarget)
		{
			this.target = base.transform.parent;
		}
		if (this.randoms != 0f)
		{
			this.sf *= Random.Range(-this.randoms, this.randoms);
			this.ss *= Random.Range(-this.randoms, this.randoms);
		}
		if (this.randomy != 0f)
		{
			this.yf *= Random.Range(-this.randomy, this.randomy);
			this.ys *= Random.Range(-this.randomy, this.randomy);
		}
	}

	// Token: 0x0600076A RID: 1898 RVA: 0x00066534 File Offset: 0x00064734
	public void StartUp(Transform spintarget, float spinfrequency, float spinspeed, float yfrequency, float yspeed, float yoffset)
	{
		this.target = spintarget;
		this.sf = spinfrequency;
		this.ss = spinspeed;
		this.yf = yfrequency;
		this.ys = yspeed;
		this.offset = yoffset;
	}

	// Token: 0x0600076B RID: 1899 RVA: 0x00066564 File Offset: 0x00064764
	private void Update()
	{
		if (this.requiresflag == -1 || MainManager.instance.flags[this.requiresflag])
		{
			if (this.itself != Vector3.zero)
			{
				if (this.local)
				{
					base.transform.localEulerAngles += this.itself * MainManager.framestep;
				}
				else
				{
					base.transform.Rotate(this.itself * MainManager.framestep);
				}
				if (this.itselfandtarget && this.target != null)
				{
					this.SpinAroundTarget();
					return;
				}
			}
			else if (this.target != null)
			{
				this.SpinAroundTarget();
			}
		}
	}

	// Token: 0x0600076C RID: 1900 RVA: 0x00066620 File Offset: 0x00064820
	private void SpinAroundTarget()
	{
		base.transform.position = this.target.transform.position + new Vector3(Mathf.Sin(Time.time * this.sf) * this.ss, Mathf.Sin(Time.time * this.yf) * this.ys + this.offset, -Mathf.Cos(Time.time * this.sf) * this.ss);
	}

	// Token: 0x04000791 RID: 1937
	public Transform target;

	// Token: 0x04000792 RID: 1938
	public Vector3 itself;

	// Token: 0x04000793 RID: 1939
	public bool itselfandtarget;

	// Token: 0x04000794 RID: 1940
	public bool gettarget;

	// Token: 0x04000795 RID: 1941
	public bool local;

	// Token: 0x04000796 RID: 1942
	public int requiresflag = -1;

	// Token: 0x04000797 RID: 1943
	public float sf;

	// Token: 0x04000798 RID: 1944
	public float ss;

	// Token: 0x04000799 RID: 1945
	public float yf;

	// Token: 0x0400079A RID: 1946
	public float ys;

	// Token: 0x0400079B RID: 1947
	public float offset;

	// Token: 0x0400079C RID: 1948
	public float randoms;

	// Token: 0x0400079D RID: 1949
	public float randomy;
}
