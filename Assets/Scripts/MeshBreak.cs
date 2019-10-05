using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBreak : MonoBehaviour
{
//Vector3数组，储存顶点的相对位置
    public Vector3[] vertices { get; set; }
//int数组，每三个数字一个三角面的顶点信息，每一个数字都是vertices的索引
    public int[] triangles { get; set; }

    // Start is called before the first frame update
    
    //生成Mesh对象
    private Mesh GenMesh(Mesh mesh, Vector3 hitPoint)
    {
        List<Vector3> mesh_vertices = new List<Vector3>();
        List<Vector3> piece_vertices = new List<Vector3>();

        int[] mesh_triangles = mesh.GetTriangles(0);

        List<Vector3> piece_normals = new List<Vector3>();

        mesh.GetVertices(mesh_vertices);
        mesh.GetNormals(piece_normals);

        foreach (Vector3 item in mesh_vertices)
        {
            piece_vertices.Add(item);
        }
        int i = 0;
        foreach (Vector3 v in mesh_vertices)
        {
            if (v == new Vector3(0.5f, 0.5f, 0.5f) || v == new Vector3(0.5f, 0.5f, -0.5f))
            {
                piece_vertices[i] = new Vector3(hitPoint.x, hitPoint.y, v.z);
            }
            else
            {
                if (v.x == 0.5f)
                {
                    piece_vertices[i] = new Vector3(0, v.y, v.z);
                }
                if (v.y == 0.5f)
                {
                    piece_vertices[i] = new Vector3(v.x, 0, v.z);
                }
            }
            i++;
        }

        Mesh piece = new Mesh();
        piece.SetVertices(piece_vertices);
        piece.SetTriangles(mesh_triangles,0);
//法线信息与光照相关
        piece.SetNormals(piece_normals);

        return piece;
    }
    //生成物体
    private GameObject GenPiece(Mesh piece_mesh,MeshRenderer meshRenderer)
    {
        GameObject piece = new GameObject("piece");
        piece.transform.position = transform.position;
        piece.transform.localScale = transform.localScale;
        MeshRenderer piece_render = piece.AddComponent<MeshRenderer>();
        piece_render.material = meshRenderer.material;
        piece.AddComponent<MeshFilter>().mesh = piece_mesh;
        Rigidbody rig = piece.AddComponent<Rigidbody>();
        piece.AddComponent<MeshCollider>().convex = true;
        return piece;
    }

    private void OnCollisionEnter(Collision other)
    {
        GenPiece(GenMesh(GetComponent<MeshFilter>().mesh, other.GetContact(0).point),GetComponent<MeshRenderer>());
    }
}
