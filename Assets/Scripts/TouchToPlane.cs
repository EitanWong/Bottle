/*
 * @authors: liangjian
 * @desc:	
*/

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
 
public class TouchToPlane : MonoBehaviour {
    private bool m_IsClearSamePoint = false;
    private Vector3 m_TouchBeganPos;
 
    private Vector3 m_ClipPlaneNormal;
    private Vector3 m_ClipPlanePoint;
    [SerializeField] private bool UseRigid;
 
    // Use this for initialization
    void Start () {
 
    }
 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_TouchBeganPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 touchBeganPoint_screen = new Vector3(m_TouchBeganPos.x, m_TouchBeganPos.y, Camera.main.nearClipPlane);
            Vector3 touchEndPoint_screen = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            Vector3 normalEndPoint_screen = new Vector3(m_TouchBeganPos.x, m_TouchBeganPos.y, Camera.main.nearClipPlane + 10);
            
            Debug.Log(touchBeganPoint_screen.x);
            if(UpdateClipPlane(touchBeganPoint_screen, touchEndPoint_screen, normalEndPoint_screen))
            {
                ClipMesh();
            }
        }
    }

    public void RandomBroken()
    {
        
    }

    bool UpdateClipPlane(Vector3 touchBeganPoint_sceen, Vector3 touchEndPoint_sceen, Vector3 normalEndPoint_screen)
    {
        if(Vector3.Distance(touchBeganPoint_sceen, touchEndPoint_sceen) == 0.0f)
        {
            return false;
        }
        //转世界坐标
        Vector3 touchBeganPoint_world = Camera.main.ScreenToWorldPoint(touchBeganPoint_sceen);
        Vector3 touchEndPoint_world = Camera.main.ScreenToWorldPoint(touchEndPoint_sceen);
        Vector3 noramlEndPoint_world = Camera.main.ScreenToWorldPoint(normalEndPoint_screen);
 
        //世界坐标转本地坐标
        Vector3 touchBeginPoint_local = transform.worldToLocalMatrix.MultiplyPoint(touchBeganPoint_world);
        Vector3 touchEndPoint_local = transform.worldToLocalMatrix.MultiplyPoint(touchEndPoint_world);
        Vector3 normalEngPoint_local = transform.worldToLocalMatrix.MultiplyPoint(noramlEndPoint_world);
 
        m_ClipPlaneNormal = Vector3.Cross(normalEngPoint_local - touchBeginPoint_local, touchEndPoint_local - touchBeginPoint_local).normalized;
        m_ClipPlanePoint = touchEndPoint_local;
 
        return true;
    }
 
    void ClipMesh()
    {
        MeshFilter mf = this.gameObject.GetComponent<MeshFilter>();
        //顶点数组转顶点容器
        List<Vector3> verticeList = new List<Vector3>();
        int verticeCount = mf.mesh.vertices.Length;
        for (int verticeIndex = 0; verticeIndex < verticeCount; ++verticeIndex)
        {
            verticeList.Add(mf.mesh.vertices[verticeIndex]);
        }
        //三角形数组转三角形容器
        List<int> triangleList = new List<int>();
        int triangleCount = mf.mesh.triangles.Length;
        for (int triangleIndex = 0; triangleIndex < triangleCount; ++triangleIndex)
        {
            triangleList.Add(mf.mesh.triangles[triangleIndex]);
        }
        //uv坐标数组转uv坐标容器
        List<Vector2> uvList = new List<Vector2>();
        int uvCount = mf.mesh.uv.Length;
        for (int uvIndex = 0; uvIndex < uvCount; ++uvIndex)
        {
            uvList.Add(mf.mesh.uv[uvIndex]);
        }
        //顶点颜色数组转顶点颜色容器
        List<Vector3> normalList = new List<Vector3>();
        int normalCount = mf.mesh.normals.Length;
        for (int normalIndex = 0; normalIndex < normalCount; ++normalIndex)
        {
            normalList.Add(mf.mesh.normals[normalIndex]);
        }
        //Debug.Log("顶点数" + verticeList.Count);
        //Debug.Log("顶点索引数" + triangleList.Count);
 
        //检查每个三角面，是否存在两个顶点连接正好在直线上
        for (int triangleIndex = 0; triangleIndex < triangleList.Count;)
        {
            //Debug.Log("次数记录");
            int trianglePointIndex0 = triangleList[triangleIndex];
            int trianglePointIndex1 = triangleList[triangleIndex + 1];
            int trianglePointIndex2 = triangleList[triangleIndex + 2];
 
            Vector3 trianglePointCoord0 = verticeList[trianglePointIndex0];
            Vector3 trianglePointCoord1 = verticeList[trianglePointIndex1];
            Vector3 trianglePointCoord2 = verticeList[trianglePointIndex2];
 
            //0-1，1-2相连线段被切割
            if(GetPointToClipPlaneDis(trianglePointCoord0)* GetPointToClipPlaneDis(trianglePointCoord1) < 0 && 
                GetPointToClipPlaneDis(trianglePointCoord1) * GetPointToClipPlaneDis(trianglePointCoord2) < 0)
            {
                //求得0-1与切平面的交点
                Vector3 newVertice01 = GetLinePlaneCrossPoint(trianglePointCoord0, trianglePointCoord1);
                int index01 = IsContainsVertice(verticeList, newVertice01);
                if (index01 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice01);
                    index01 = verticeList.Count - 1;
                    float k01 = 0;
                    if (!IsEqual(newVertice01.x, trianglePointCoord0.x) && !IsEqual(trianglePointCoord1.x, trianglePointCoord0.x))
                    {
                        k01 = (newVertice01.x - trianglePointCoord0.x) / (trianglePointCoord1.x - trianglePointCoord0.x);
                    }
                    else if (!IsEqual(newVertice01.y, trianglePointCoord0.y) && !IsEqual(trianglePointCoord1.y, trianglePointCoord0.y))
                    {
                        k01 = (newVertice01.y - trianglePointCoord0.y) / (trianglePointCoord1.y - trianglePointCoord0.y);
                    }
                    else
                    {
                        k01 = (newVertice01.z - trianglePointCoord0.z) / (trianglePointCoord1.z - trianglePointCoord0.z);
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv0 = uvList[trianglePointIndex0];
                        Vector2 uv1 = uvList[trianglePointIndex1];
                        
                        float newUV_x = (uv1.x - uv0.x) * k01 + uv0.x;
                        float newUV_y = (uv1.y - uv0.y) * k01 + uv0.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        //Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX0 = normalList[trianglePointIndex0];
                    Vector3 normalX1 = normalList[trianglePointIndex1];
                    float newNoramlX01 = (normalX1.x - normalX0.x) * k01 + normalX0.x;
                    float newNoramlY01 = (normalX1.y - normalX0.y) * k01 + normalX0.y;
                    float newNoramlZ01 = (normalX1.z - normalX0.z) * k01 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX01, newNoramlY01, newNoramlZ01));
                }
 
                //求得1-2与切平面的交点
                Vector3 newVertice12 = GetLinePlaneCrossPoint(trianglePointCoord1, trianglePointCoord2);
                int index12 = IsContainsVertice(verticeList, newVertice12);
                if (index12 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice12);
                    index12 = verticeList.Count - 1;
                    float k12 = 0;
                    if (!IsEqual(newVertice12.x, trianglePointCoord1.x) && !IsEqual(trianglePointCoord2.x, trianglePointCoord1.x))
                    {
                        k12 = (newVertice12.x - trianglePointCoord1.x) / (trianglePointCoord2.x - trianglePointCoord1.x);
                    }
                    else if (!IsEqual(newVertice12.y, trianglePointCoord1.y) && !IsEqual(trianglePointCoord2.y, trianglePointCoord1.y))
                    {
                        k12 = (newVertice12.y - trianglePointCoord1.y) / (trianglePointCoord2.y - trianglePointCoord1.y);
 
                    }
                    else
                    {
                        k12 = (newVertice12.z - trianglePointCoord1.z) / (trianglePointCoord2.z - trianglePointCoord1.z);
 
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv1 = uvList[trianglePointIndex1];
                        Vector2 uv2 = uvList[trianglePointIndex2];
 
                        float newUV_x = (uv2.x - uv1.x) * k12 + uv1.x;
                        float newUV_y = (uv2.y - uv1.y) * k12 + uv1.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        //Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX1 = normalList[trianglePointIndex1];
                    Vector3 normalX2 = normalList[trianglePointIndex2];
                    float newNoramlX12 = (normalX2.x - normalX1.x) * k12 + normalX1.x;
                    float newNoramlY12 = (normalX2.y - normalX1.y) * k12 + normalX1.y;
                    float newNoramlZ12 = (normalX2.z - normalX1.z) * k12 + normalX1.z;
                    normalList.Add(new Vector3(newNoramlX12, newNoramlY12, newNoramlZ12));
                }
                //插入顶点索引，以此构建新三角形
                triangleList.Insert(triangleIndex + 1, index01);
                triangleList.Insert(triangleIndex + 2, index12);
 
                triangleList.Insert(triangleIndex + 3, index12);
                triangleList.Insert(triangleIndex + 4, index01);
 
                triangleList.Insert(triangleIndex + 6, trianglePointIndex0);
                triangleList.Insert(triangleIndex + 7, index12);
                triangleIndex += 9;
            }
            //1-2，2-0相连线段被切割
            else if (GetPointToClipPlaneDis(trianglePointCoord1) * GetPointToClipPlaneDis(trianglePointCoord2) < 0 &&
                GetPointToClipPlaneDis(trianglePointCoord2) * GetPointToClipPlaneDis(trianglePointCoord0) < 0)
            {
                //求得1-2与切平面的交点
                Vector3 newVertice12 = GetLinePlaneCrossPoint(trianglePointCoord1, trianglePointCoord2);
                int index12 = IsContainsVertice(verticeList, newVertice12);
                if (index12 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice12);
                    index12 = verticeList.Count - 1;
                    float k12 = 0;
                    if(!IsEqual(newVertice12.x, trianglePointCoord1.x) && !IsEqual(trianglePointCoord2.x, trianglePointCoord1.x))
                    {
                        k12 = (newVertice12.x - trianglePointCoord1.x) / (trianglePointCoord2.x - trianglePointCoord1.x);
                    }
                    else if (!IsEqual(newVertice12.y, trianglePointCoord1.y) && !IsEqual(trianglePointCoord2.y, trianglePointCoord1.y))
                    {
                        k12 = (newVertice12.y - trianglePointCoord1.y) / (trianglePointCoord2.y - trianglePointCoord1.y);
                    }
                    else
                    {
                        k12 = (newVertice12.z - trianglePointCoord1.z) / (trianglePointCoord2.z - trianglePointCoord1.z);
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv1 = uvList[trianglePointIndex1];
                        Vector2 uv2 = uvList[trianglePointIndex2];
 
                        float newUV_x = (uv2.x - uv1.x) * k12 + uv1.x;
                        float newUV_y = (uv2.y - uv1.y) * k12 + uv1.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        //Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX1 = normalList[trianglePointIndex1];
                    Vector3 normalX2 = normalList[trianglePointIndex2];
                    float newNoramlX12 = (normalX2.x - normalX1.x) * k12 + normalX1.x;
                    float newNoramlY12 = (normalX2.y - normalX1.y) * k12 + normalX1.y;
                    float newNoramlZ12 = (normalX2.z - normalX1.z) * k12 + normalX1.z;
                    normalList.Add(new Vector3(newNoramlX12, newNoramlY12, newNoramlZ12));
                }
                //求得0-2与切平面的交点
                Vector3 newVertice02 = GetLinePlaneCrossPoint(trianglePointCoord0, trianglePointCoord2);
                int index02 = IsContainsVertice(verticeList, newVertice02);
                if (index02 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice02);
                    index02 = verticeList.Count - 1;
                    float k02 = 0;
                    if (!IsEqual(newVertice02.x, trianglePointCoord0.x) && !IsEqual(trianglePointCoord2.x, trianglePointCoord0.x))
                    {
                        k02 = (newVertice02.x - trianglePointCoord0.x) / (trianglePointCoord2.x - trianglePointCoord0.x);
                        
                    }
                    else if (!IsEqual(newVertice02.y, trianglePointCoord0.y) && !IsEqual(trianglePointCoord2.y, trianglePointCoord0.y))
                    {
                        k02 = (newVertice02.y - trianglePointCoord0.y) / (trianglePointCoord2.y - trianglePointCoord0.y);
                    }
                    else
                    {
                        k02 = (newVertice02.z - trianglePointCoord0.z) / (trianglePointCoord2.z - trianglePointCoord0.z);
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv0 = uvList[trianglePointIndex0];
                        Vector2 uv2 = uvList[trianglePointIndex2];
 
                        float newUV_x = (uv2.x - uv0.x) * k02 + uv0.x;
                        float newUV_y = (uv2.y - uv0.y) * k02 + uv0.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        //Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX0 = normalList[trianglePointIndex0];
                    Vector3 normalX2 = normalList[trianglePointIndex2];
                    float newNoramlX02 = (normalX2.x - normalX0.x) * k02 + normalX0.x;
                    float newNoramlY02 = (normalX2.y - normalX0.y) * k02 + normalX0.y;
                    float newNoramlZ02 = (normalX2.z - normalX0.z) * k02 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX02, newNoramlY02, newNoramlZ02));
                }
                //插入顶点索引，以此构建新三角形
 
                //{0}
                //{1}
                triangleList.Insert(triangleIndex + 2, index12);
 
                triangleList.Insert(triangleIndex + 3, index02);
                triangleList.Insert(triangleIndex + 4, index12);
                //{2}
 
                triangleList.Insert(triangleIndex + 6, index02);
                triangleList.Insert(triangleIndex + 7, trianglePointIndex0);
                triangleList.Insert(triangleIndex + 8, index12);
                triangleIndex += 9;
            }
            //0-1，2-0相连线段被切割
            else if (GetPointToClipPlaneDis(trianglePointCoord0) * GetPointToClipPlaneDis(trianglePointCoord1) < 0 &&
                GetPointToClipPlaneDis(trianglePointCoord2) * GetPointToClipPlaneDis(trianglePointCoord0) < 0)
            {
                //求得0-1与切平面的交点
                Vector3 newVertice01 = GetLinePlaneCrossPoint(trianglePointCoord0, trianglePointCoord1);
                int index01 = IsContainsVertice(verticeList, newVertice01);
                if (index01 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice01);
                    index01 = verticeList.Count - 1;
                    float k01 = 0;
                    if (!IsEqual(newVertice01.x, trianglePointCoord0.x) && !IsEqual(trianglePointCoord1.x, trianglePointCoord0.x))
                    {
                        k01 = (newVertice01.x - trianglePointCoord0.x) / (trianglePointCoord1.x - trianglePointCoord0.x);
                    }
                    else if(!IsEqual(newVertice01.y, trianglePointCoord0.y) && !IsEqual(trianglePointCoord1.y, trianglePointCoord0.y))
                    {
                        k01 = (newVertice01.y - trianglePointCoord0.y) / (trianglePointCoord1.y - trianglePointCoord0.y);
                    }
                    else
                    {
                        k01 = (newVertice01.z - trianglePointCoord0.z) / (trianglePointCoord1.z - trianglePointCoord0.z);
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv0 = uvList[trianglePointIndex0];
                        Vector2 uv1 = uvList[trianglePointIndex1];
 
                        float newUV_x = (uv1.x - uv0.x) * k01 + uv0.x;
                        float newUV_y = (uv1.y - uv0.y) * k01 + uv0.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX0 = normalList[trianglePointIndex0];
                    Vector3 normalX1 = normalList[trianglePointIndex1];
                    float newNoramlX01 = (normalX1.x - normalX0.x) * k01 + normalX0.x;
                    float newNoramlY01 = (normalX1.y - normalX0.y) * k01 + normalX0.y;
                    float newNoramlZ01 = (normalX1.z - normalX0.z) * k01 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX01, newNoramlY01, newNoramlZ01));
                }
                //求得0-2与切平面的交点
                Vector3 newVertice02 = GetLinePlaneCrossPoint(trianglePointCoord0, trianglePointCoord2);
                int index02 = IsContainsVertice(verticeList, newVertice02);
                if (index02 == -1 || !m_IsClearSamePoint)
                {
                    verticeList.Add(newVertice02);
                    index02 = verticeList.Count - 1;
                    float k02 = 0;
                    if (!IsEqual(newVertice02.x, trianglePointCoord0.x) && !IsEqual(trianglePointCoord2.x, trianglePointCoord0.x))
                    {
                        k02 = (newVertice02.x - trianglePointCoord0.x) / (trianglePointCoord2.x - trianglePointCoord0.x);
                    }
                    else if(!IsEqual(newVertice02.y, trianglePointCoord0.y) && !IsEqual(trianglePointCoord2.y, trianglePointCoord0.y))
                    {
                        k02 = (newVertice02.y - trianglePointCoord0.y) / (trianglePointCoord2.y - trianglePointCoord0.y);
                    }
                    else
                    {
                        k02 = (newVertice02.z - trianglePointCoord0.z) / (trianglePointCoord2.z - trianglePointCoord0.z);
                    }
                    if (uvList.Count > 0)
                    {
                        Vector2 uv0 = uvList[trianglePointIndex0];
                        Vector2 uv2 = uvList[trianglePointIndex2];
 
                        float newUV_x = (uv2.x - uv0.x) * k02 + uv0.x;
                        float newUV_y = (uv2.y - uv0.y) * k02 + uv0.y;
                        uvList.Add(new Vector2(newUV_x, newUV_y));
                        Debug.Log("纹理坐标" + uvList[uvList.Count - 1]);
                    }
                    //法向量
                    Vector3 normalX0 = normalList[trianglePointIndex0];
                    Vector3 normalX2 = normalList[trianglePointIndex2];
                    float newNoramlX02 = (normalX2.x - normalX0.x) * k02 + normalX0.x;
                    float newNoramlY02 = (normalX2.y - normalX0.y) * k02 + normalX0.y;
                    float newNoramlZ02 = (normalX2.z - normalX0.z) * k02 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX02, newNoramlY02, newNoramlZ02));
                }
                //插入顶点索引，以此构建新三角形
 
                //{0}
                triangleList.Insert(triangleIndex + 1, index01);
                triangleList.Insert(triangleIndex + 2, index02);
 
                triangleList.Insert(triangleIndex + 3, index01);
                //{1}
                //{2}
 
                triangleList.Insert(triangleIndex + 6, trianglePointIndex2);
                triangleList.Insert(triangleIndex + 7, index02);
                triangleList.Insert(triangleIndex + 8, index01);
                triangleIndex += 9;
            }
            else
            {
                triangleIndex += 3;
            }
        }
        //Debug.Log("顶点数" + verticeList.Count);
        //Debug.Log("顶点索引数" + triangleList.Count);
 
        //筛选出切割面两侧的顶点索引
        List<int> triangles1 = new List<int>();
        List<int> triangles2 = new List<int>();
        for (int triangleIndex = 0; triangleIndex < triangleList.Count; triangleIndex += 3)
        {
            int trianglePoint0 = triangleList[triangleIndex];
            int trianglePoint1 = triangleList[triangleIndex + 1];
            int trianglePoint2 = triangleList[triangleIndex + 2];
 
            Vector3 point0 = verticeList[trianglePoint0];
            Vector3 point1 = verticeList[trianglePoint1];
            Vector3 point2 = verticeList[trianglePoint2];
            //切割面
            float dis0 = GetPointToClipPlaneDis(point0);
            float dis1 = GetPointToClipPlaneDis(point1);
            float dis2 = GetPointToClipPlaneDis(point2);
 
            if ((dis0 < 0 || IsEqual(dis0, 0)) && (dis1 < 0 || IsEqual(dis1, 0)) && (dis2 < 0 || IsEqual(dis2, 0)))
            {
                triangles1.Add(trianglePoint0);
                triangles1.Add(trianglePoint1);
                triangles1.Add(trianglePoint2);
            }
            else
            {
                triangles2.Add(trianglePoint0);
                triangles2.Add(trianglePoint1);
                triangles2.Add(trianglePoint2);
            }
        }
 
        //新生顶点数
        int newVerticeCount = verticeList.Count - verticeCount;
        //再次添加一遍新增顶点，用于缝合切口
        for (int newVerticeIndex = 0; newVerticeIndex < newVerticeCount; ++newVerticeIndex)
        {
            Vector3 newVertice = verticeList[verticeCount + newVerticeIndex];
            Vector3 qiekouVertice = new Vector3(newVertice.x, newVertice.y, newVertice.z);
            verticeList.Add(qiekouVertice);
            //uv
            if (uvList.Count > 0)
            {
                Vector2 newUv = uvList[verticeCount + newVerticeIndex];
                Vector2 qiekouUv = new Vector3(0.99f, 0.99f);
                uvList.Add(qiekouUv);
            }
 
 
            //法线
            Vector3 newNormal = normalList[verticeCount + newVerticeIndex];
            Vector3 qiekouNormal = new Vector3(newNormal.x, newNormal.y, newNormal.z);
            normalList.Add(qiekouNormal);
        }
        verticeCount = verticeCount + newVerticeCount;
        //重新排序新生成的顶点,按照角度
        List<SortAngle> SortAngleList = new List<SortAngle>();
        for (int verticeIndex = verticeCount + 1; verticeIndex < verticeList.Count; verticeIndex++)
        {
            //计算角度,以0-1为参照
            Vector3 vec1to0 = verticeList[verticeCount + 1] - verticeList[verticeCount];
            Vector3 indexTo0 = verticeList[verticeIndex] - verticeList[verticeCount];
 
            float moIndexto0 = indexTo0.magnitude;
            float mo1to0 = vec1to0.magnitude;
            float dotRes = Vector3.Dot(indexTo0, vec1to0);
            if (moIndexto0 == 0.0f)
            {
                continue;
            }
            float angle = Mathf.Acos(dotRes / (mo1to0 * moIndexto0)); //Vector3.Angle(indexTo0.normalized, vec1to0.normalized);
            bool isExis = false;
            for (int i = 0; i < SortAngleList.Count; ++i)
            {
                //同样角度，距离近的被剔除
                if (Mathf.Abs(SortAngleList[i].Angle * 180.0f / Mathf.PI - angle * 180.0f / Mathf.PI) < 0.1f)
                {
                    float dis1 = Vector3.Distance(verticeList[SortAngleList[i].Index], verticeList[verticeCount]);
                    float dis2 = Vector3.Distance(verticeList[verticeIndex], verticeList[verticeCount]);
                    if (dis2 >= dis1)
                    {
                        SortAngleList[i].Index = verticeIndex;
                    }
                    isExis = true;
                    break;
                }
            }
            if (!isExis)
            {
                //Debug.Log(angle);
                SortAngle sortAngle = new SortAngle();
                sortAngle.Index = verticeIndex;
                sortAngle.Angle = angle;
                SortAngleList.Add(sortAngle);
            }
        }
        SortAngleList.Sort();
        //缝合切口
        for (int verticeIndex = 0; verticeIndex < SortAngleList.Count - 1;)
        {
            triangles1.Add(SortAngleList[verticeIndex + 1].Index);
            triangles1.Add(SortAngleList[verticeIndex].Index);
            triangles1.Add(verticeCount);
 
            triangles2.Add(verticeCount);
            triangles2.Add(SortAngleList[verticeIndex].Index);
            triangles2.Add(SortAngleList[verticeIndex + 1].Index);
 
 
            verticeIndex++;
        }
 
        mf.mesh.vertices = verticeList.ToArray();
        mf.mesh.triangles = triangles1.ToArray();
        if (uvList.Count > 0)
        {
            mf.mesh.uv = uvList.ToArray();
        }
        mf.mesh.normals = normalList.ToArray();
        //分割模型
        GameObject newModel = new GameObject("New Model");
        MeshFilter meshFilter = newModel.AddComponent<MeshFilter>();
        meshFilter.mesh.vertices = mf.mesh.vertices;
        meshFilter.mesh.triangles = triangles2.ToArray();
        meshFilter.mesh.uv = mf.mesh.uv;
        meshFilter.mesh.normals = mf.mesh.normals;
        Renderer newRenderer = newModel.AddComponent<MeshRenderer>();
        newRenderer.material = this.gameObject.GetComponent<MeshRenderer>().material;
        newModel.transform.localPosition = transform.localPosition;
        newModel.AddComponent<TouchToPlane>();
        if (UseRigid)
            newModel.AddComponent<Rigidbody>();
    }
 
    float GetPointToClipPlaneDis(Vector3 point)
    {
        return Vector3.Dot((point - m_ClipPlanePoint), m_ClipPlaneNormal);
    }
 
    Vector3 GetLinePlaneCrossPoint(Vector3 lineBegin, Vector3 lineEnd)
    {
        //Debug.Log("起点：" + lineBegin + "," + "终点：" + lineEnd);
        float x = 0;
        float y = 0;
        float z = 0;
        float offsetZ = lineEnd.z - lineBegin.z;
        float offsetY = lineEnd.y - lineBegin.y;
        if (offsetZ != 0)
        {
            float k1 = (m_ClipPlaneNormal.x * (lineEnd.x - lineBegin.x) + m_ClipPlaneNormal.y * (lineEnd.y - lineBegin.y)) / offsetZ + m_ClipPlaneNormal.z;
            float k2 = (m_ClipPlaneNormal.x * lineBegin.z * (lineEnd.x - lineBegin.x) + m_ClipPlaneNormal.y * lineBegin.z * (lineEnd.y - lineBegin.y)) / offsetZ;
            float k3 = m_ClipPlaneNormal.x * lineBegin.x - m_ClipPlaneNormal.x * m_ClipPlanePoint.x + m_ClipPlaneNormal.y * lineBegin.y - m_ClipPlaneNormal.y * m_ClipPlanePoint.y - m_ClipPlaneNormal.z * m_ClipPlanePoint.z;
            z = (k2 - k3) / k1;
            x = (z - lineBegin.z) * (lineEnd.x - lineBegin.x) / offsetZ + lineBegin.x;
            y = (z - lineBegin.z) * (lineEnd.y - lineBegin.y) / offsetZ + lineBegin.y;
        }
        else if(offsetY != 0)
        {
            z = lineBegin.z;
            float k1 = m_ClipPlaneNormal.y + m_ClipPlaneNormal.x * (lineEnd.x - lineBegin.x) / (lineEnd.y - lineBegin.y);
            float k2 = m_ClipPlaneNormal.x * lineBegin.y * (lineEnd.x - lineBegin.x) / (lineEnd.y - lineBegin.y);
            float k3 = lineBegin.x* m_ClipPlaneNormal.x - m_ClipPlanePoint.x * m_ClipPlaneNormal.x - m_ClipPlaneNormal.y * m_ClipPlanePoint.y + m_ClipPlaneNormal.z * z - m_ClipPlaneNormal.z * m_ClipPlanePoint.z;
            y = (k2 - k3) / k1;
            x = (y - lineBegin.y) * (lineEnd.x - lineBegin.x) / (lineEnd.y - lineBegin.y) + lineBegin.x;
        }
        else
        {
            z = lineBegin.z;
            y = lineBegin.y;
            x = (m_ClipPlaneNormal.x * m_ClipPlanePoint.x - m_ClipPlaneNormal.y * (y - m_ClipPlanePoint.y) - m_ClipPlaneNormal.z * (z - m_ClipPlanePoint.z)) / m_ClipPlaneNormal.x;
        }
 
 
 
        // Debug.Log(x + "," + y + "," + z);
        return new Vector3(x, y, z);
    }
 
    int IsContainsVertice(List<Vector3> list, Vector3 vertice)
    {
        int count = list.Count;
        for(int i = 0; i < count; ++i)
        {
            Vector3 ii = list[i];
            if (IsEqual(ii.x, vertice.x) && IsEqual(ii.y, vertice.y) && IsEqual(ii.z, vertice.z))
            {
                return i;
            }
        }
 
        return -1;
    }
 
    bool IsEqual(float num1, float num2)
    {
        float absX = Mathf.Abs(num1 - num2);
        return absX < 0.00001f;
    }
}

