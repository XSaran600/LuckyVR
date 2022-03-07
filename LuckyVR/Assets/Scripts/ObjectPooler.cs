using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPooler : NetworkBehaviour
{
    // Pool
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler instance;

    // Singleton
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Create the gameobjects in the pools
    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool _pool in pools)
        {
            Queue<GameObject> _objectPool = new Queue<GameObject>();

            for (int i = 0; i < _pool.size; i++)
            {
                GameObject _obj = Instantiate(_pool.prefab);
                _obj.SetActive(false);
                _objectPool.Enqueue(_obj);
            }

            poolDictionary.Add(_pool.tag, _objectPool);
        }
    }

    // Destroy the gameobjects in the pools
    private void OnDestroy()
    {
        foreach (Pool _pool in pools)
        {
            for (int i = 0; i < _pool.size; i++)
            {
                Destroy(poolDictionary[_pool.tag].Dequeue());
            }
        }
    }

    // Spawn the objects from the pool
    public void SpawnFromPool(string _tag, Vector3 _position, Quaternion _rotation)
    {
        if (!hasAuthority)
            return;

        CmdSpawnFromPool(_tag, _position, _rotation);
    }
    [Command]
    void CmdSpawnFromPool(string _tag, Vector3 _position, Quaternion _rotation)
    {
        RpcSpawnFromPool(_tag, _position, _rotation);
    }
    [ClientRpc]
    void RpcSpawnFromPool(string _tag, Vector3 _position, Quaternion _rotation)
    {
        // Check to see if the tag is valid
        if (!poolDictionary.ContainsKey(_tag))
        {
            Debug.LogWarning("Tag " + _tag + "does not exist.");
            return;
        }

        GameObject _objectToSpawn = poolDictionary[_tag].Dequeue();

        _objectToSpawn.SetActive(true);
        _objectToSpawn.transform.position = _position;
        _objectToSpawn.transform.rotation = _rotation;

        poolDictionary[_tag].Enqueue(_objectToSpawn);
    }

    // DeSpawn the objects from the pool
    public void DeSpawnFromPool(string _tag)
    {
        if (!hasAuthority)
            return;

        CmdDeSpawnFromPool(_tag);
    }
    [Command]
    void CmdDeSpawnFromPool(string _tag)
    {
        RpcDeSpawnFromPool(_tag);
    }
    [ClientRpc]
    void RpcDeSpawnFromPool(string _tag)
    {
        // Check to see if the tag is valid
        if (!poolDictionary.ContainsKey(_tag))
        {
            Debug.LogWarning("Tag " + _tag + "does not exist.");
            return;
        }

        for (int i = 0; i < poolDictionary[_tag].Count; i++)
        {
            GameObject _objectToSpawn = poolDictionary[_tag].Dequeue();
            _objectToSpawn.SetActive(false);

            poolDictionary[_tag].Enqueue(_objectToSpawn);
        }
    }
}
