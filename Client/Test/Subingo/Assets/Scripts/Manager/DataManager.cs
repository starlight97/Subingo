using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

public class DataManager
{
    public static readonly DataManager instance = new DataManager();

    public UnityEvent<string, float> onDataLoadComplete = new UnityEvent<string, float>();
    public UnityEvent onDataLoadFinished = new UnityEvent();

    private Dictionary<int, RawData> dicDatas = new Dictionary<int, RawData>();
    private List<DatapathData> dataPathList;

    private DataManager()
    {
    }

    public void Init()
    {
        var datapath = "Datas/datapath_data";
        var asset = Resources.Load<TextAsset>(datapath);
        var json = asset.text;
        var datas = JsonConvert.DeserializeObject<DatapathData[]>(json);

        this.dataPathList = datas.ToList();
    }
    public void LoadData<T>(string json) where T : RawData
    {
        var datas = JsonConvert.DeserializeObject<T[]>(json);
        datas.ToDictionary(x => x.id).ToList().ForEach(x => dicDatas.Add(x.Key, x.Value));
    }

    public void LoadAllData()
    {
        App.instance.StartCoroutine(LoadAllDataRoutine());
    }
    private IEnumerator LoadAllDataRoutine()
    {
        int idx = 0;
        // ������ ������ �׽�Ʈ �뵵
        if (dataPathList.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                this.onDataLoadComplete.Invoke("ȣ���� ������ ����", (float)(i + 1) / 10);
                yield return new WaitForSeconds(0.1f);
            }
        }

        foreach (var data in this.dataPathList)
        {
            var path = string.Format("Datas/{0}", data.res_name);
            ResourceRequest req = Resources.LoadAsync<TextAsset>(path);
            yield return req;
            float progress = (float)(idx + 1) / this.dataPathList.Count;
            this.onDataLoadComplete.Invoke(data.res_name, progress);
            TextAsset asset = (TextAsset)req.asset;
            Debug.Log(req.asset);
            var typeDef = Type.GetType(data.type);
            this.GetType().GetMethod(nameof(LoadData))
                .MakeGenericMethod(typeDef).Invoke(this, new string[] { asset.text });

            yield return new WaitForSeconds(0.3f);
            idx++;
        }
        yield return null;

        this.onDataLoadFinished.Invoke();
    }


    // id ������ ��ųʸ����� �˻��ϴ� �޼ҵ�
    public T GetData<T>(int id) where T : RawData
    {
        var collection = this.dicDatas.Values.Where(x => x.GetType().Equals(typeof(T)));
        return (T)collection.ToList().Find(x => x.id == id);
    }

    // Ư�� ������ �׷��� �˻��ϰ������ �ش� ������ Ÿ���� ȣ���ϸ� �ش� ������Ÿ�� ��ü�� ��ȯ�Ѵ�.
    // ex(GetDatas<Student>()) = ��ųʸ��� ����� �����͵��� StudentŸ���� ���� ��ü�鸸 ��ȯ
    public IEnumerable<T> GetDataList<T>() where T : RawData
    {
        IEnumerable<RawData> col = this.dicDatas.Values.Where(x => x.GetType().Equals(typeof(T)));
        return col.Select(x => (T)Convert.ChangeType(x, typeof(T)));
    }

}