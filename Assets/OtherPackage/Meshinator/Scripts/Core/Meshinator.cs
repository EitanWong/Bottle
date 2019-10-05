/*
 * Meshinator
 * Copyright Mike Mahoney 2013
 */

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Rigidbody))]
public class Meshinator : MonoBehaviour
{
	#region Fields & Properties

	[SerializeField] public UnityEvent OnBreak;
	
	public enum CacheOptions
	{
		None = 0,
		CacheAfterCollision = 1,
		CacheOnLoad = 2,
	};
	
	public enum ImpactShapes
	{
		FlatImpact = 0,
		SphericalImpact = 1,
	};
	
	public enum ImpactTypes
	{
		Compression = 0,
		Fracture = 1,
	};
	
	// This is how many FixedUpdates we require before we will do anything in the OnCollisionEnter function.
	// If this many FixedUpdate calls have occurred and we have not collided with anything, then the 
	// OnCollisionEnter function will the work normally. This is meant to prevent SubHulls creating new
	// GameObjects with colliders that overlap from the start and cause bad behavior.
	private const int c_FixedUpdateCountToIgnoreCollisions = 3;
	
	// This determines when we load up the Hull object.
	// - "None" means that nothing is loaded until a collision occurs; this has no extra memory or CPU
	// cost except when a collision is occuring, but the collision will take longer to compute.
	// - "CacheAfterCollision" loads a copy of the mesh during the first collision and maintains that
	// information from that point forward. This will mean a heavier memory footprint for each object
	// that has already had at least one collsion, but each collsion after the first will compute faster.
	// - "CacheOnLoad" loads a copy of the mesh on Start, which means a heavier footprint from the
	// beginning, but every deformation computation will be faster than they would without caching.
	public CacheOptions m_CacheOption = CacheOptions.CacheAfterCollision;
	
	// This determines what kind of deformation calculations will occur on a collision.
	// - "FlatImpact" treats the deformation as if the collision ocurred against a flat plane. This is
	// generally fine for collisions between objects of roughly the same size.
	// - "Spherical Impact" treats the deformation is if the collision occured against a sphere. This is
	// good when objects colliding against this one are expected to be much smaller than this object.
	public ImpactShapes m_ImpactShape = ImpactShapes.FlatImpact;
	
	// This determines what the result of the deformation calculations will be after a collision.
	// - "Compression" compresses the mesh on impact, but does not create any extra debris
	// - "Fracture" shatters a mesh into seperate meshes on seperate game objects
	public ImpactTypes m_ImpactType = ImpactTypes.Compression;

	// Subtracted from the force of an impact before any deformation is done. If the force becomes negative
	// due to this, no deformation will occur (the material resisted the impact).
	public float m_ForceResistance = 10f;
	
	// This is the maximum force that can affect a mesh on any given impact. Any force beyond this amount is
	// negated. If m_MaxForcePerImpact is less than or equal to m_ForceResistance, then no impact will ever
	// deform this GameObject's mesh.
	public float m_MaxForcePerImpact = 12f;
	//乘以冲击力，以确定冲击爆炸等的深度
//值表示密度较小的材质（因此变形较大），而较小的值表示
//更致密的材料（从而减少变形）。
	public float m_ForceMultiplier = 0.25f;
	
	// Is an impact currently being calculated? If so, we'll end up ignoring other Impact calls to prevent
	// concurrent modifications to the mesh.
	private bool m_Calculating = false;
	
	//这是缓存外壳对象，我们可以使用，也可以不使用，基于上面的设置cacheoptions。
	private Hull m_Hull = null;
	
	//这些是用于引导网格变形的边界框，这样网格变形就不会收缩或展开
//大小不合理。它们要么在开始时建立（如果cacheoptions.none），要么在第一个
//collation（如果cacheoptions.cacheonload或cacheoptions.cacheaftercollision）。
	private bool m_BoundsSet = false;
	private Bounds m_InitialBounds;
	
	// Collision-enabling info
	private bool m_ClearedForCollisions = true;
	private int m_CollisionCount = 0;
	private int m_FixedUpdatesSinceLastCollision = 0;
	
	#endregion Fields & Properties
	
	#region Unity Functions
	
	public void Start()
	{
		if (m_CacheOption == CacheOptions.CacheOnLoad)
		{
			// Make sure we have a MeshFilter
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null || meshFilter.sharedMesh == null)
				return;
			
			// Get the initial bounding box for this mesh
			m_InitialBounds = meshFilter.sharedMesh.bounds;
			m_BoundsSet = true;
			
			// Generate a hull to work with
			m_Hull = new Hull(meshFilter.sharedMesh);
		}
	}
	
	public void FixedUpdate()
	{
		//跟踪MyCeleReDeFraseCo是错误的时发生多少固定更新。

//如果发生了足够多的fixedupdate调用，并且我们没有与任何内容冲突，那么

//oncollisionenter函数将正常工作。这是为了防止副标题产生新的

//gameobjects与碰撞器从一开始就重叠并导致不良行为。
		if (m_ClearedForCollisions == false)
		{
			if (m_CollisionCount != 0)
				m_FixedUpdatesSinceLastCollision = 0;
			else
				m_FixedUpdatesSinceLastCollision++;
			
			if (m_FixedUpdatesSinceLastCollision > c_FixedUpdateCountToIgnoreCollisions)
				m_ClearedForCollisions = true;
		}
	}
	
	#endregion Unity Functions
	
	#region Collision Callback Functions
	
	public void OnCollisionEnter(Collision collision)
	{
		m_CollisionCount++;

		if (m_ClearedForCollisions && collision.impactForceSum.magnitude >= m_ForceResistance)
		{
			// 找到撞击点
			foreach (ContactPoint contact in collision.contacts)
			{
				if (contact.otherCollider == collision.collider)
				{
					Impact(contact.point, collision.impactForceSum, m_ImpactShape, m_ImpactType);
					break;
				}
			}
		}
	}
	
	public void OnCollisionExit()
	{
		m_CollisionCount--;
	}
	
	#endregion Collision Callback Functions
	
	#region Impact Functions
	
	public void DelayCollisions()
	{
		m_ClearedForCollisions = false;
	}

	public void Impact(Vector3 point, Vector3 force, ImpactShapes impactShape, ImpactTypes impactType)
	{
		// 看看我们现在能不能做到
		if (!CanDoImpact(point, force))
			return;

		// 我们现在开始计算撞击变形
		m_Calculating = true;
		
		// Set up m_Hull
		InitializeHull();

		// Figure out the true impact force
		if (force.magnitude > m_MaxForcePerImpact)
			force = force.normalized * m_MaxForcePerImpact;
		float impactFactor = (force.magnitude - m_ForceResistance) * m_ForceMultiplier;
		if (impactFactor <= 0)
			return;

		// Localize the point and the force to account for transform scaling (and maybe rotation or translation)
		Vector3 impactPoint = transform.InverseTransformPoint(point);
		Vector3 impactForce = transform.InverseTransformDirection(force.normalized) * impactFactor;
		
		// Limit the force by the extents of the initial bounds to keep things reasonable
		float impactForceX = Mathf.Max(Mathf.Min(impactForce.x, m_InitialBounds.extents.x), -m_InitialBounds.extents.x);
		float impactForceY = Mathf.Max(Mathf.Min(impactForce.y, m_InitialBounds.extents.y), -m_InitialBounds.extents.y);
		float impactForceZ = Mathf.Max(Mathf.Min(impactForce.z, m_InitialBounds.extents.z), -m_InitialBounds.extents.z);
		impactForce = new Vector3(impactForceX, impactForceY, impactForceZ);
		
		// Run the mesh deformation on another thread
		ThreadManager.RunAsync(()=>
		{
			// Do all the math to deform this mesh
			m_Hull.Impact(impactPoint, impactForce, impactShape, impactType);
			
			// Queue the final mesh setting on the main thread once the deformations are done
			ThreadManager.QueueOnMainThread(()=>
			{
				// Clear out the current Mesh and MeshCollider (if we have one) for now
				MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
				if (meshFilter != null)
					meshFilter.sharedMesh = null;
				MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
				if (meshCollider != null)
					meshCollider.sharedMesh = null;
				
				// Get the newly-adjusted Mesh so we can work with it
				Mesh newMesh = m_Hull.GetMesh();
				
				
				// If this is a fracture, then create a new GameObject for the chunk that broke off
				if (impactType == ImpactTypes.Fracture)
				{
					Mesh subHullMesh = m_Hull.GetSubHullMesh();

					if (subHullMesh != null)
					{
						// Create the new GameObject
						GameObject newGO = (GameObject)GameObject.Instantiate(gameObject);
						try
						{
							Destroy(newGO.GetComponent<Movement>());
							Destroy(newGO.GetComponent<StepCheck>());
							Destroy(newGO.GetComponent<AutoSaver>());
						}
						catch (Exception e)
						{
						}
			
						// Set the new Mesh onto the MeshFilter and MeshCollider
						MeshFilter newMeshFilter = newGO.GetComponent<MeshFilter>();
						MeshCollider newMeshCollider = newGO.GetComponent<MeshCollider>();
						if (newMeshFilter != null)
							newMeshFilter.sharedMesh = subHullMesh;
						if (newMeshCollider != null)
							newMeshCollider.sharedMesh = subHullMesh;
						
						// If using convex MeshColliders, it's possible for colliders to overlap right after a
						// fracture, which causes exponentially more fractures... So, right after a fracture,
						// temporarily disable impacts to both the old GameObject, and the new one we just created
						DelayCollisions();
						Meshinator subHullMeshinator = newGO.GetComponent<Meshinator>();
						if (subHullMeshinator != null)
							subHullMeshinator.DelayCollisions();

						// Figure out the approximate volume of our Hull and SubHull meshes. This
						// will be used to calculate the rigidbody masses (if a rigidbodies are present)
						// for the old and new GameObjects.
						if (gameObject.GetComponent<Rigidbody>() != null && newGO.GetComponent<Rigidbody>() != null)
						{
							Vector3 hullSize = newMesh.bounds.size;
							float hullVolume = hullSize.x * hullSize.y * hullSize.z;
							
							Vector3 subHullSize = subHullMesh.bounds.size;
							float subHullVolume = subHullSize.x * subHullSize.y * subHullSize.z;
							
							float totalVolume = hullVolume + subHullVolume;
							float totalMass = gameObject.GetComponent<Rigidbody>().mass;
							
							gameObject.GetComponent<Rigidbody>().mass = totalMass * (hullVolume / totalVolume);
							newGO.GetComponent<Rigidbody>().mass = totalMass * (subHullVolume / totalVolume);
							
							// Set the old velocity onto the new GameObject's rigidbody
							newGO.GetComponent<Rigidbody>().velocity = gameObject.GetComponent<Rigidbody>().velocity;
							
							// Set the centers of mass to be within the new meshes
							gameObject.GetComponent<Rigidbody>().centerOfMass = newMesh.bounds.center;
							newGO.GetComponent<Rigidbody>().centerOfMass = subHullMesh.bounds.center;
						}
					}
				}
				
				// Set the hull's new mesh back onto this game object
				if (meshFilter != null)
					meshFilter.sharedMesh = newMesh;
				
				// If this GameObject has a MeshCollider, put the new mesh there too
				if (meshCollider != null)
					meshCollider.sharedMesh = newMesh;
				
				// Drop our cached Hull if we're not supposed to keep it around
				if (m_CacheOption == CacheOptions.None)
					m_Hull = null;
				
				// Our calculations are done
				m_Calculating = false;

			});
		});
		OnBreak?.Invoke();
		Handheld.Vibrate();
		GC.Collect();
	}
	
	private void InitializeHull()
	{
		// See if we already have a hull
		if (m_Hull == null)
		{
			// Make sure we have a MeshFilter
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null || meshFilter.sharedMesh == null)
			{
				m_Calculating = false;
				return;
			}
			
			// Get the initial bounding box for this mesh if needed
			if (m_BoundsSet == false)
			{
				m_InitialBounds = meshFilter.sharedMesh.bounds;
				m_BoundsSet = true;
			}
			
			// Generate a hull to work with
			m_Hull = new Hull(meshFilter.sharedMesh);
		}
	}
	
	private bool CanDoImpact(Vector3 point, Vector3 force)
	{
		//如果我们已经在处理对FlatImpact的调用，我们将忽略
		// 防止同时修改网格。
		if (m_Calculating)
			return false;

		// 确保此力可以影响此对象
		float forceMagnitude = force.magnitude;
		if (forceMagnitude - m_ForceResistance <= 0)
			return false;
		
		// 计算出真实的冲击力，并确保它大于零
		float impactFactor = (forceMagnitude - m_ForceResistance) * m_ForceMultiplier;
		if (impactFactor <= 0)
			return false;
		
		return true;
	}
	
	#endregion Impact Functions
}
