using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GhostCarData
{
    [SerializeField]
    List<GhostCarDataListItem> ghostCarDataList = new List<GhostCarDataListItem>();

    public void AddDataItem(GhostCarDataListItem ghostCarDataListItem)
    {
        ghostCarDataList.Add(ghostCarDataListItem);
    }

    public List<GhostCarDataListItem> GetDataList()
    {
        return ghostCarDataList;
    }
}
