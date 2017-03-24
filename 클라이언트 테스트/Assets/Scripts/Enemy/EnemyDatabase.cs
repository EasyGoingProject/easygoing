// EnemyData를 에셋데이터베이스로 변환, Load에 사용될 클래스
// Assets/Data/EnemyDB에 저장되어 있음

using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Database/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    // 시리얼라이즈된 구조체 배열
    // 에셋데이터베이스에서 확인 가능
    public EnemyData[] enemyDatas;

    // 인덱스를 통해서 구조체를 받아옴
    public EnemyData Get(int _index)
    {
        return enemyDatas[_index];
    }

    // 적타입을 통해서 구조체를 받아옴
    public EnemyData Get(EnemyType enemyType)
    {
        return Array.Find(enemyDatas, x => x.enemyType == enemyType);
    }
}
