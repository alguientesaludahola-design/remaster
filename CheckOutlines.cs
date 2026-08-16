using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class CheckOutlines : MonoBehaviour
{
	// Token: 0x0600018B RID: 395 RVA: 0x00013062 File Offset: 0x00011262
	public void SetUp(Transform mesh, float outline)
	{
		this.baseoutline = outline;
		this.mainmesh = mesh;
	}

	// Token: 0x0600018C RID: 396 RVA: 0x00013074 File Offset: 0x00011274
	private void Start()
	{
		MeshRenderer[] array = Object.FindObjectsOfType<MeshRenderer>();
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j < array[i].materials.Length; j++)
			{
				if (array[i].materials[j].shader == MainManager.outlinemain.shader && array[i].materials[j] != MainManager.fakelight)
				{
					array[i].materials[j].color = Color.black;
					if (array[i].transform.parent != this.mainmesh)
					{
						array[i].materials[j].SetFloat("_Outline", array[i].gameObject.transform.localScale.magnitude / 2f * this.baseoutline);
					}
					else
					{
						array[i].materials[j].SetFloat("_Outline", this.baseoutline * 2f);
					}
				}
			}
		}
	}

	// Token: 0x0600018D RID: 397 RVA: 0x0001317D File Offset: 0x0001137D
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.O) && Application.isEditor)
		{
			this.Start();
		}
	}

	// Token: 0x04000109 RID: 265
	public Transform mainmesh;

	// Token: 0x0400010A RID: 266
	public float baseoutline = 20f;
}
