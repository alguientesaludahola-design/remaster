using System;
using UnityEngine;

// Token: 0x02000051 RID: 81
public class ShadowLite : MonoBehaviour
{
	// Token: 0x0600075E RID: 1886 RVA: 0x000660B0 File Offset: 0x000642B0
	private void Start()
	{
		this.shadow = new GameObject("shadow").AddComponent<SpriteRenderer>();
		this.shadow.transform.parent = base.transform;
		this.shadow.sprite = MainManager.shadowsprite;
		this.shadow.color = new Color(1f, 1f, 1f, this.shadowammount);
		this.shadow.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x00066141 File Offset: 0x00064341
	public void SetUp(float opacity, float size)
	{
		this.shadowsize = size;
		this.shadowammount = opacity;
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x00066154 File Offset: 0x00064354
	private void LateUpdate()
	{
		if (this.shadow.isVisible)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out raycastHit, 10f, 8448))
			{
				this.shadow.transform.position = new Vector3(base.transform.position.x, raycastHit.point.y + 0.025f, base.transform.position.z);
				this.shadow.transform.localScale = Vector3.ClampMagnitude(Vector3.one * this.shadowsize * Mathf.Clamp(1f - Mathf.Abs(raycastHit.point.y - base.transform.position.y) / 10f, 0f, float.PositiveInfinity), 1f);
				this.shadow.transform.LookAt(this.shadow.transform.position + raycastHit.normal);
			}
			this.shadow.enabled = (raycastHit.transform != null);
		}
	}

	// Token: 0x04000783 RID: 1923
	private SpriteRenderer shadow;

	// Token: 0x04000784 RID: 1924
	public float shadowammount = 0.4f;

	// Token: 0x04000785 RID: 1925
	public float shadowsize = 1f;
}
