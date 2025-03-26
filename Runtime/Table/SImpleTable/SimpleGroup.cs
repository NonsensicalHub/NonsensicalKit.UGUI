using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.UGUI.Table
{
    public class SimpleGroup : MonoBehaviour
    {
        [SerializeField] private GameObject m_prefab;
        [SerializeField] private GameObject m_group;

        [SerializeField] private UnityEvent<int, GameObject> m_onNewObject;

        public UnityEvent<int, GameObject> OnNewObject
        {
            get => m_onNewObject;
            set => m_onNewObject = value;
        }

        private readonly Queue<GameObject> _activeObjects = new();
        private readonly Queue<GameObject> _inactiveObjects = new();

        public List<GameObject> Objects => _activeObjects.ToList();

        private void Awake()
        {
            if (m_prefab.transform.parent != null && m_prefab.transform.parent == m_group.transform)
            {
                m_prefab.SetActive(false);
            }
        }

        public void Create(int count)
        {
            Clear();
            Append(count);
        }

        public void Append(int count)
        {
            int baseNum = _activeObjects.Count;
            for (int i = 0; i < count; i++)
            {
                GameObject newObj;
                if (_inactiveObjects.Count > 0)
                {
                    newObj = _inactiveObjects.Dequeue();
                }
                else
                {
                    newObj = Instantiate(m_prefab, m_group.transform, false);
                }

                newObj.SetActive(true);
                _activeObjects.Enqueue(newObj);
                m_onNewObject?.Invoke(baseNum + i, newObj);
            }
        }

        public void Clear()
        {
            foreach (var obj in _activeObjects)
            {
                obj.SetActive(false);
                _inactiveObjects.Enqueue(obj);
            }

            _activeObjects.Clear();
        }
    }
}
