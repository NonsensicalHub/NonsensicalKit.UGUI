using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Editor.Table
{
    public interface ITreeNodeClass<Self>
    {
        public List<Self> Childs { get; }   //子节点链表，不可为null
        public bool IsFold { get; set; }    //是否收起
        public int Level { get; set; }           //级别，根结点为0级
        public Self Parent { get; set; }         //父节点
        public GameObject Belong { get; set; }   //所属对象
        public void UpdateInfo();           //通过Childs信息更新整棵树的Level和Parent信息
    }
}
