
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StepManager : MonoBehaviour
{
    public static StepManager INS;
    [SerializeField] private List<GameObject> StepPrefabs;//生成台阶的预制物
    [SerializeField] private Vector3 PossitonDiffValue;//生成的位置差值
    [SerializeField] private Vector3 ScaleDiffValue;//生成的位置差值
    [SerializeField] private Vector3 SpawnRange;//生成的范围;
    private Transform LastInsStep;//上一个生成的台阶

    [SerializeField] float  MaxSpawnStepHeight;//台阶生成的台阶高度
    private void Awake()
    {
        INS = this;
        SpawnSteps();
    }

    private Vector3 SpawnDir=Vector3.right;
    
    
    
    public void SpawnSteps()
    {
        if (!LastInsStep)//判断是否为第一个台阶
        {
            var RandomStep = StepPrefabs[GetRandomLimit(StepPrefabs.Count)];
            var Player = GameObject.FindGameObjectWithTag("Player").transform;
            var RandomPossiton=new Vector3(Player.position.x+GetRandomLimit(10)+ GetRandomValue(PossitonDiffValue.x),
                0+GetRandomLimit((int)PossitonDiffValue.y),
                Player.position.z+GetRandomLimit(10)+GetRandomValue(PossitonDiffValue.z));
            LastInsStep = Instantiate(RandomStep,RandomPossiton,Quaternion.identity).transform;
        }

        for (int i = 0; i < MaxSpawnStepHeight/PossitonDiffValue.y/2; i++)
        {
                var RandomStep = StepPrefabs[GetRandomLimit(StepPrefabs.Count)];
                
                
                
                var RandomPossiton = LastInsStep.position + SpawnDir*GetRandomLimit(10)+ new Vector3(GetRandomLimit((int)PossitonDiffValue.x),
                                         0+GetRandomLimit((int)PossitonDiffValue.y),
                                         GetRandomLimit((int)PossitonDiffValue.z));
                while (IsOutSide(RandomPossiton))
                {
                  //  SpawnDir = MathUtils.RotateRound(SpawnDir, LastInsStep.position, Vector3.up, 90f);
                  //  SpawnDir =(RandomPossiton - LastInsStep.position).normalized;
                    SpawnDir=new Vector3(Random.Range(-1f,1f),Random.Range(0,1f),Random.Range(-1f,1f));
                    RandomPossiton = LastInsStep.position + SpawnDir*GetRandomLimit(10)+ new Vector3(GetRandomLimit((int)PossitonDiffValue.x),
                                             0+GetRandomLimit((int)PossitonDiffValue.y),
                                             GetRandomLimit((int)PossitonDiffValue.z));
                }
                
                var InsStep=Instantiate(RandomStep,RandomPossiton,Quaternion.identity).transform;
                //SpawnDir = (InsStep.transform.position - LastInsStep.transform.position).normalized;
                //var RandomPossiton=new Vector3(LastInsStep.transform.position.x+GetRandomValue(5)+GetRandomValue(PossitonDiffValue.x),LastInsStep.transform.position.y+GetRandomLimit((int)PossitonDiffValue.y),LastInsStep.transform.position.z+GetRandomValue(5)+GetRandomValue(PossitonDiffValue.z));
                LastInsStep = InsStep;
        }
       // while (LastInsStep.transform.position.y<MaxSpawnStepHeight)
        //{

            
       // }
       
    }

    public void Change()
    {
      //  var MaxX = (transform.position.x + SpawnRange.x) / 2;
     //   if (RandomPossiton.x > MaxX)
      //      RandomPossiton=new Vector3(MaxX,RandomPossiton.y,RandomPossiton.z);
      //  var MinX = (transform.position.x - SpawnRange.x) / 2;
      //  if (RandomPossiton.x < MinX)
       //     RandomPossiton=new Vector3(MinX,RandomPossiton.y,transform.position.x - SpawnRange.x);
      //  var MaxZ = (transform.position.z - SpawnRange.z) / 2;
     //   if (RandomPossiton.z < MaxZ)
     //       RandomPossiton=new Vector3(RandomPossiton.x,RandomPossiton.y, MaxZ);
     //   var MinZ = (transform.position.z + SpawnRange.z) / 2;
     //   if (RandomPossiton.z > MinZ)
      //      RandomPossiton=new Vector3(RandomPossiton.x,RandomPossiton.y, MinZ);
    }

    private bool IsOutSide(Vector3 Pos)
    {
        if (Pos.x > (transform.position.x + SpawnRange.x)/2)
            return true;
        if (Pos.x < (transform.position.x - SpawnRange.x)/2)
            return true;
        if (Pos.z < (transform.position.z - SpawnRange.z)/2)
            return true;
        if (Pos.z > (transform.position.z + SpawnRange.z)/2)
            return true;
        return false;

    }

    static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    static float GetRandomValue(float Limit)
    {
        var LimitValue = (int) Limit;
        return  UnityEngine.Random.Range(-(float)new System.Random(GetRandomSeed()).Next(LimitValue),(float)new System.Random(GetRandomSeed()).Next(LimitValue));
    }
    static int GetRandomLimit(int MaxValue)
    {
        return  new System.Random(GetRandomSeed()).Next(MaxValue);
    }


    private void OnDrawGizmos()
    {
       //Gizmos.DrawWireMesh(Mesh,TargetTrans.position);
       Gizmos.color=Color.blue;
       Gizmos.DrawWireCube(transform.position,SpawnRange);
    }
}
