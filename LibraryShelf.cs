using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class LibraryShelf : MonoBehaviour
{
	// Token: 0x0600042D RID: 1069 RVA: 0x0002AF37 File Offset: 0x00029137
	private void Start()
	{
		this.objs = new List<GameObject>();
		base.transform.localPosition = new Vector3(-2.6f, 0.8f, 1.4f);
		this.Refresh();
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x0002AF6C File Offset: 0x0002916C
	public void Refresh()
	{
		GameObject[] array = this.objs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
		this.objs = new List<GameObject>();
		for (int j = 0; j < MainManager.instance.flagvar[15]; j++)
		{
			if (this.objs.Count < MainManager.instance.flagvar[15])
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.book);
				gameObject.transform.parent = base.transform;
				gameObject.transform.localEulerAngles = new Vector3(0f, 90f, -90f);
				gameObject.transform.localScale = new Vector3(0.75f, 0.6f, 0.5f);
				gameObject.transform.localPosition = new Vector3(0.4f * (float)this.objs.Count - ((j < 14) ? 0f : 5.2f), 0f, (j < 14) ? 0f : 1.72f);
				this.objs.Add(gameObject);
			}
		}
	}

	// Token: 0x040003E2 RID: 994
	public GameObject book;

	// Token: 0x040003E3 RID: 995
	private List<GameObject> objs;

	// Token: 0x040003E4 RID: 996
	private const int breakpoint = 14;
}
