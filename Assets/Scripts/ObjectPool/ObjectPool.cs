using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	// 미리 생성해서 채워 넣을 슬라임 프리팹을 연결하면 됩니다.
	public string prefabName;

	// 슬라임을 몇 개체 만들어 놓을지 정하는 변수입니다.
	public int poolSize = 20;

	// _available은 사용 대기 중인 오브젝트 배열입니다.
	private List<GameObject> _available = new List<GameObject>();
	// _inUse 는 현재 화면에 나타나서 사용중인 오브젝트 배열입니다.
	private List<GameObject> _inUse = new List<GameObject>();


	// poolSize 풀 사이즈 만큼 미리 슬라임을 생성해놓습니다.
	public void PreloadPool()
	{
		for(int i = 0; i< poolSize ; i++)
		{
			// 새로운 게임 오브젝트를 생성하는 스크립트 Instantiate
			GameObject po = Instantiate(Resources.Load( prefabName) as GameObject);
			_available.Add(po);
			// 
			po.transform.SetParent(transform);

			//po.transform.parent = transform;
			po.SetActive(false);
		}
	}

	// 몬스터 오브젝트 하나를 풀에서 꺼내오는 함수입니다.
	public GameObject GetObject(Vector3 position = new Vector3())
	{
		// 이 작업을 수행하는 동안 _available 배열을 잠구어 놓습니다.
		// 여러 군데서 동시에 GetObject()를 호출하면 충돌이 일어나기 때문에 이를 예방하기 위함입니다.
		lock(_inUse){

		lock(_available)
		{
			if (_available.Count != 0)
			{
				GameObject po = _available[0];
				_inUse.Add(po);
				_available.RemoveAt(0);
					SetUp(po, position);
				return po;
			}
			else
				{
					Debug.Log ("_available Count : " + _available.Count);
					Debug.Log ("_inUse Count : " + _inUse.Count);
					Debug.Log ("Prefab Create : " + prefabName);
				// 오브젝트 풀에 남은 슬라임이 없으면 새로 생성해서 오브젝트 풀에 등록합니다.
				GameObject po = Instantiate(Resources.Load(prefabName) as GameObject);
				_inUse.Add(po);
				// 전체 오브젝트 풀 사이즈가 늘어났으므로 풀을 1칸 늘립니다.
				poolSize++;
				
					SetUp(po, position);
				return po;
			}
		}
		}
	}
	
	public void ReleaseObject(GameObject po)
	{
		// 풀에 다시 넣기 전에, 오브젝트를 비활성화 시킵니다.
		CleanUp(po);
		lock (_inUse)
		{
		lock (_available)
		{
			_available.Add(po);
			_inUse.Remove(po);
		}
		}
	}

	public virtual void SetUp(GameObject po, Vector3 position)
	{
		// 오브젝트 풀(_available)에 담겨있을 때는 비활성화 상태이므로
		// po.gameObject.SetActive(true); 로 게임 오브젝트를 활성화시킵니다.
		po.transform.localPosition = position;
		po.SetActive(true);
	}

	public virtual void CleanUp(GameObject po)
	{
		po.transform.SetParent(transform);
		//po.transform.parent = transform;
		po.SetActive(false);
	}
}
