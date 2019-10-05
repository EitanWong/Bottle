using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

//using UnityEngine;
//using System.Collections.Generic;
using System;
//using UnityStandardAssets.Characters.FirstPerson; //spent 6 hours trying to make naitive FPSControllers work, to no avail. I'm going to edit the controllers to make them work

// This is in fact just the Water script from Pro Standard Assets,
// just with refraction stuff removed.

// Actually this is now just the mirror script from the Unify Community with some modifications that make it portaly

[ExecuteInEditMode] // Make portal live-update even when not in play mode
public class portal : MonoBehaviour
{

	[Serializable]
	public class advancedSettings
	{
		public bool m_Teleport = true;
		public int m_TextureSize = 512;
		//public int m_RecursionDepth = 0;
		//public float m_CamTeleportBuffer = .3f;
	}

	private bool m_DisablePixelLights = false;
	private float m_ClipPlaneOffset = 0f;//0.07f;

	private LayerMask m_ReflectLayers = -1;

	private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table

	private RenderTexture m_ReflectionTexture = null;
	private int m_OldReflectionTextureSize = 0;

	private static bool portalRendered=false;

	public GameObject partner;
	public advancedSettings AdvancedSettings;

	private int m_TextureSize;
	private bool m_Teleport;
	private int m_RecursionDepth;

	private int depth = 0;

	private Dictionary<int, GameObject> pClones = new Dictionary<int, GameObject>();

	// This is called when it's known that the object will be rendered by some
	// camera. We render reflections and do other updates here.
	// Because the script executes in edit mode, looking through portals in the scene view
	// camera will just work!

	public void OnWillRenderObject()
	{
		//Debug.Log (depth);

		m_TextureSize = AdvancedSettings.m_TextureSize;
		m_Teleport = AdvancedSettings.m_Teleport;
		//m_RecursionDepth = AdvancedSettings.m_RecursionDepth;

		//for (var passthrough = 0; passthrough < m_RecursionDepth+1; passthrough++) {
				
			if (!partner)
				return;

			if (m_TextureSize != partner.GetComponent<portal> ().m_TextureSize) {
				m_TextureSize = Mathf.Max (AdvancedSettings.m_TextureSize, partner.GetComponent<portal> ().AdvancedSettings.m_TextureSize);
				partner.GetComponent<portal> ().m_TextureSize = Mathf.Max (AdvancedSettings.m_TextureSize, partner.GetComponent<portal> ().AdvancedSettings.m_TextureSize);
			}

			var rend = GetComponent<Renderer> ();
			if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled) {
				if (!rend.sharedMaterial) {
					Material portalMat = new Material (Shader.Find ("FX/Portal"));
					//Shader portalShader;
					//if (!Shader.Find ("FX/Portal"))
					//Debug.LogError ("Portal Shader not found!");
					//else
					//portalMat.shader = Shader.Find ("FX/Portal");

					//portalMat.mainTextureScale = new Vector2 (-1,1);
					//portalMat.SetTextureScale(new Vector2(-1,1));
					//portalMat.mainTextureOffset = new Vector2(1,0);
					//portalMat.SetTextureOffset (new Vector2 (1, 0));

					//portalMat.SetTextureScale("_ReflectionTex", new Vector2(-1,1));
					rend.sharedMaterial = portalMat;
				}
				return;
			}
				

			Camera cam = Camera.current;
			if (!cam)
				return;

			// Safeguard from recursive portal rendering.        
			if (portalRendered)
				return;
			portalRendered = true;

			Camera reflectionCamera;
			CreateMirrorObjects (cam, out reflectionCamera);

			// find out the reflection plane: position and normal in world space
			Vector3 pos = partner.transform.position;
			Vector3 normal = partner.transform.up;

			// Optionally disable pixel lights for reflection
			int oldPixelLightCount = QualitySettings.pixelLightCount;
			if (m_DisablePixelLights)
				QualitySettings.pixelLightCount = 0;

			UpdateCameraModes (cam, reflectionCamera);

			//transform camera to other portal

			Transform formerParent = cam.transform.parent;
			cam.transform.SetParent (transform);

			Vector3 localPos = cam.transform.localPosition;
			Quaternion localRot = cam.transform.localRotation;

			//partner.transform.Rotate (Vector3 (180, 0, 0));

			cam.transform.SetParent (partner.transform);


			cam.transform.localPosition = localPos;

			//Solved multiorientation issue:
			//I inverted zpos and zforward for all cases but that should only happen in some cases
			//(generalization needed)
			//additionally, near-plane matrix needs to be flipped correctly as well

			//^that wasn't actually the issue. implicitly setting transforms via direction vectors resulted in ambiguity

			//MO

			//cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, -cam.transform.localPosition.z);
			cam.transform.localRotation = localRot;
			//cam.transform.forward = new Vector3(cam.transform.forward.x, cam.transform.forward.y, -cam.transform.forward.z);

			//cam.transform.position += (partner.transform.position - transform.position);

			// Render reflection
			// Reflect camera around reflection plane
			float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);

			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection, reflectionPlane);

			Vector3 complement = partner.transform.forward;
			float d2 = -Vector3.Dot (complement, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane2 = new Vector4 (complement.x, complement.y, complement.z, d2);

			Matrix4x4 reflection2 = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection2, reflectionPlane2);

			//calculate reflections
			Vector3 oldpos = cam.transform.position;
			Vector3 newpos = reflection2.MultiplyPoint (reflection.MultiplyPoint (oldpos)); //reflection2.MultiplyPoint()
			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection * reflection2;

			//transform camera back
			//cam.transform.position = transform.TransformPoint (localPos);
			//cam.transform.forward = transform.TransformDirection (localForward);
			//cam.transform.up = transform.TransformDirection (localUp);


			//cam.transform.position -= (partner.transform.position - transform.position);

			// Setup oblique projection matrix so that near plane is our reflection
			// plane. This way we clip everything below/above it for free.
			Vector4 clipPlane = CameraSpacePlane (reflectionCamera, pos, normal, 1.0f);
			//Matrix4x4 projection = cam.projectionMatrix;

			//reflectionCamera.projectionMatrix *= Matrix4x4.Scale(new Vector3 (-1, 1, 1));

			Matrix4x4 projection = cam.CalculateObliqueMatrix (clipPlane);
			reflectionCamera.projectionMatrix = projection;// * Matrix4x4.Scale(new Vector3 (-1, 1, 1));  //DV

			cam.transform.SetParent (transform);
			//partner.transform.up = -partner.transform.up;

			//partner.transform.Rotate (Vector3 (180, 0, 0));


			cam.transform.localPosition = localPos;
			cam.transform.localRotation = localRot;
			cam.transform.SetParent (formerParent);

			//reflectionCamera.ResetWorldToCameraMatrix ();
			//reflectionCamera.ResetProjectionMatrix ();
			//reflectionCamera.projectionMatrix = reflectionCamera.projectionMatrix * Matrix4x4.Scale(new Vector3 (-1, 1, 1));


			//reflectionCamera.cullingMask = ~(1<<4) & m_ReflectLayers.value; // never render water layer
			reflectionCamera.transform.position = newpos;
			reflectionCamera.targetTexture = m_ReflectionTexture;
			//GL.SetRevertBackfacing (true);

			Vector3 euler = cam.transform.eulerAngles;
			//reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);  //??

			//reflectionCamera.transform.position = oldpos;
			//GL.SetRevertBackfacing (false);

		//for (var passthrough = 0; passthrough < m_RecursionDepth + 1; passthrough++) {}



		
			Material[] materials = rend.sharedMaterials;
			foreach (Material mat in materials) {
				if (mat.HasProperty ("_ReflectionTex"))
					mat.SetTexture ("_ReflectionTex", m_ReflectionTexture);
			}
			
			depth++;
			reflectionCamera.Render ();

			// Restore pixel light count
			if (m_DisablePixelLights)
				QualitySettings.pixelLightCount = oldPixelLightCount;


		portalRendered = false;
	}

	/*void OnCollisionEnter(Collision other)
	{
		other.gameObject.GetComponent<Rigidbody> ().AddForce (-other.impulse, ForceMode.Impulse); //nullify change in momentum while hitting portal. Now any collider can act as a trigger. //this was stupid
		//stupid but maybe usefull as this could be applied to objects behind the portal to prevent the need for leaving space behind the portal
		OnTriggerEnter (other.collider);
	}*/

	bool isBehindMe(Vector3 loc)
	{
		//if (transform.InverseTransformDirection(cam.transform.forward).y > 0)
		return transform.InverseTransformPoint (loc).y < 0;
	}

	bool isCamBehindMe(GameObject cam)
	{
		//if (transform.InverseTransformDirection(cam.transform.forward).y > 0)
		return transform.InverseTransformPoint (cam.transform.position).y < 0;
	}

	Vector3 getChildCameraPos(GameObject obj)
	{
		//Debug.LogWarning
		Vector3 myCamPos = new Vector3(0,0,0);
		float CamCount = 0;
		foreach(Transform child in obj.transform)
		{
			//child.gameObject
			if (child.gameObject.GetComponent<Camera> ()) {
				if (CamCount == 1)
					Debug.LogWarning("This script doesn't know when exactly to teleport objects that have multiple cameras for the seamless effect. It's going to average the camera's positions and hope for the best.");
				myCamPos += child.gameObject.GetComponent<Camera> ().transform.position;
				CamCount++;
			}
		}
		return myCamPos * (1f / CamCount);
	}

	GameObject getChildCamera(GameObject obj)
	{
		foreach(Transform child in obj.transform)
		{
			//child.gameObject
			if (child.gameObject.GetComponent<Camera> ()) {
				return child.gameObject.GetComponent<Camera> ().gameObject;
			}
		}
		return null;
	}

	bool hasChildCamera(GameObject obj)
	{
		float CamCount = 0;
		foreach(Transform child in obj.transform)
		{
			//child.gameObject
			if (child.gameObject.GetComponent<Camera> ()) {
				CamCount++;
			}
		}
		return CamCount > 0;
	}

	void OnTriggerExit(Collider other)
	{
		if (hasChildCamera (other.gameObject))
			getChildCamera (other.gameObject).GetComponent<Camera> ().nearClipPlane = .3f;
		else {
			Destroy (pClone);
			pClones.Remove (other.gameObject.GetInstanceID());
		}
	}

	/*void OnTriggerStay(Collider other)
	{
		if (!cleared)
			OnTriggerEnter (other);
	}*/

	//void OnTriggerEnter(Collider other)

	GameObject pClone;
	void OnTriggerStay(Collider other)
	{
			//Debug.Log ("Walked in");
			//if (teleportedTo)
				//cleared = true;

		//Debug.Log (getChildCameraPos(other.gameObject));
			//if (!teleportedTo && (!hasChildCamera(other.gameObject) || isBehindMe(getChildCamera(other.gameObject)))) {

		if (hasChildCamera(other.gameObject))
		{
			Camera cam = getChildCamera (other.gameObject).GetComponent<Camera> ();
			cam.nearClipPlane = Mathf.Min(.3f, .007f * (cam.transform.position - transform.position).magnitude);
		}

		/*
		 A Portal Clone (pClone) is different from unity's regular cloning system because an object may only have one pClone and that clone must be easy
		 to find given the original object. The clone is invisible in the unity editor and is set not to save. This is part of the design to give the
		 impression a single object is in two places.
		 */
		foreach (GameObject clone in pClones.Values)
		{
			Destroy(pClone);
		}
		pClones.Clear (); //this is bad //wait why was this bad?

		if (m_Teleport && (!hasChildCamera (other.gameObject) && !isBehindMe (other.transform.position))) {
			//A basic object is colliding with the portal but is not teleported yet

			if (!pClones.ContainsKey (other.gameObject.GetInstanceID()) && !pClones.ContainsValue(other.gameObject)) { //doesn't have clone, and isn't clone
				//this object is *not* labeled as having a pClonepClones

				//init clone
				GameObject clone = Instantiate(other.gameObject, transform.position, Quaternion.identity) as GameObject;
				clone.name = "PClone of " + other.gameObject.name;

				//Destroy (clone.GetComponent<ConstantForce> ());
				//Destroy (clone.GetComponent<Rigidbody> ());
				//clone.GetComponent<Rigidbody> () = false;

				//Destroy (clone.GetComponent<Collider> ()); //this doesn't do anything anyway, and having collisions is nice

				//set flags same as portal camera
				//enable this when disposal system and all that jazz works
				//clone.hideFlags = HideFlags.HideAndDontSave;


				//copy original, except components that can drive motion (colliders, scripts, rigidbody)
				//^bad idea because script could drive motion *and* visuals. I need to constantly override the object's position. I can still remove colliders and rigidbody.

				pClones.Add (other.gameObject.GetInstanceID(), clone);
				pClones.TryGetValue (other.gameObject.GetInstanceID(), out pClone);
				} 


			}	else {
			
				if (isBehindMe (other.transform.position)) {
				//print (other.gameObject.name);
					Destroy (pClone);
				pClones.Remove (other.gameObject.GetInstanceID());
				}
			}

		if (m_Teleport && (!hasChildCamera(other.gameObject) || isCamBehindMe(getChildCamera(other.gameObject)))) {
			//if it doesn't have a camera you need to duplicate the mesh or something

			if (!hasChildCamera (other.gameObject)) {
				if (!isBehindMe (other.transform.position)) {
					//if the object doesn't have a camera and is (mostly) in front of the portal, just stop now and don't teleport
					//^Not anymore
					//return;
				} else {
				}
			}

			Transform previousParent = other.transform.parent;
			// print (previousParent);
			Vector3 prevScale = other.transform.localScale;
			//print (prevScale);
			Vector3 localVel = transform.InverseTransformVector(other.gameObject.GetComponent<Rigidbody>().velocity);



			//Vector3 localPos = transform.InverseTransformPoint (other.transform.position);
			//Vector3 localForward = transform.InverseTransformVector(other.transform.forward);//
			//Vector3 localUp = transform.InverseTransformVector(other.transform.up);

			//print (pClones.ContainsKey (other.gameObject));
			if (!pClones.ContainsKey (other.gameObject.GetInstanceID()))
			{
				other.transform.SetParent (transform);
				Vector3 localPos = other.transform.localPosition;
				Quaternion localRot = other.transform.localRotation;
				other.transform.SetParent (partner.transform);
			//it's supposed to be invert y (and z?)

				other.transform.localPosition = new Vector3(localPos.x,-localPos.y,-localPos.z);  //i think z-invert is right
				other.transform.localRotation = localRot;
				other.transform.SetParent (previousParent);
				//print ("other " + other.transform.parent);
			}
			else
			{
				other.transform.SetParent (transform);
				Vector3 localPos = other.transform.localPosition;
				Quaternion localRot = other.transform.localRotation;
				other.transform.SetParent (previousParent);
				//print (pClone);
				if (pClone) {
					pClone.transform.SetParent (partner.transform);
					pClone.transform.localPosition = new Vector3 (localPos.x, -localPos.y, -localPos.z);
					pClone.transform.localRotation = localRot;
					pClone.transform.SetParent (null);
				}
				//print ("pclone " + pClone.transform.parent);
			}


			//other.transform.localRotation = localRot; //this was never necessary I don't think \\Actually it was



			Vector3 pos = partner.transform.position;
			Vector3 normal = partner.transform.up;

			float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);

			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection, reflectionPlane);


			Vector3 complement = partner.transform.forward;
			float d2 = -Vector3.Dot (complement, pos) - m_ClipPlaneOffset;
			Vector4 reflectionPlane2 = new Vector4 (complement.x, complement.y, complement.z, d2);

			Matrix4x4 reflection2 = Matrix4x4.zero;
			CalculateReflectionMatrix (ref reflection2, reflectionPlane2);


			other.transform.localScale = prevScale;
			if (!pClones.ContainsKey (other.gameObject.GetInstanceID())) {
				other.transform.rotation = Quaternion.LookRotation (reflection2.MultiplyVector (reflection.MultiplyVector (other.transform.forward)), reflection2.MultiplyVector (reflection.MultiplyVector (other.transform.up)));
				other.transform.localScale = prevScale;
			} else if (pClone) {
				pClone.transform.rotation = Quaternion.LookRotation (reflection2.MultiplyVector (reflection.MultiplyVector (other.transform.forward)), reflection2.MultiplyVector (reflection.MultiplyVector (other.transform.up)));
				pClone.transform.localScale = other.transform.localScale;
			}

			if (other.gameObject.GetComponent ("PortalableFirstPersonController")) {
				//Debug.Log ("RB Controller went through");

				other.gameObject.GetComponent ("PortalableFirstPersonController").SendMessage ("UpdateOrientation", other.transform.rotation);

				//Debug.Log (controller);

				//controller.mouseLook.Init(other.transform, Camera.main.transform); //generzlis!

			}

			//other.transform.position = partner.transform.TransformPoint (localPos);
			//other.transform.up = partner.transform.InverseTransformVector (localUp);
			//other.transform.forward = partner.transform.InverseTransformVector (localForward);

			//RigidbodyFirstPersonController myScript = other.gameObject.GetComponent<RigidbodyFirstPersonController> ();


			/*Component[] cs = (Component[])other.gameObject.GetComponents(typeof(Component));
			foreach (Component c in cs)
			{
				Debug.Log("name " + c.name + " type " + c.GetType() + " basetype " + c.GetType().BaseType);

				//this part is where I want to check with the class that was passed to the function as a parameter
				//Debug.Log(c.GetType());

			}*/

			//Debug.Log (other.gameObject.GetComponent ("UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController"));

			//Debug.Log (other.gameObject.GetComponent ("RigidbodyFirstPersonController"));
			//Debug.Log (other.gameObject.GetComponent<MonoBehaviour> ());


			if (!pClones.ContainsKey (other.gameObject.GetInstanceID())) {
				other.gameObject.GetComponent<Rigidbody> ().velocity = partner.transform.TransformVector (-localVel);
			}

			if (isBehindMe(other.transform.position))
				partner.GetComponent<portal> ().OnTriggerStay (other); //call this early to prevent flicker

			//other.gameObject.GetComponent<Rigidbody> ().AddForce (partner.transform.TransformVector (new Vector3 (0, 3, 0)), ForceMode.VelocityChange);

			//just isn't working all that well, reconsider using collider penalties
			/*if (localVel.magnitude < m_OutForceThreshold) {
				other.gameObject.GetComponent<Rigidbody> ().AddForce (partner.transform.TransformVector (new Vector3 (0, 7 * m_OutForceMultiplier, 0)), ForceMode.VelocityChange);
			}*/


			//partner.GetComponent<portal>().SetTeleportedTo();
			//cleared = true;
		}
		//teleportedTo = false;
	}

	//I can get rid of variables teleportedTo and cleared by making a portal recieving something have a solid collider till that thing leaves

	/*void SetTeleportedTo()
	{
		teleportedTo = true;
	}*/

	// Cleanup all the objects we possibly have created
	void OnDisable()
	{
		if( m_ReflectionTexture ) {
			DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = null;
		}
		foreach( DictionaryEntry kvp in m_ReflectionCameras ) //cameras
			DestroyImmediate( ((Camera)kvp.Value).gameObject );
		//foreach( DictionaryEntry kpl in pClones ) //clones
		//	DestroyImmediate( ((Camera)kpl.Value).gameObject );
		m_ReflectionCameras.Clear();
	}


	//TODO: Skybox is actually inverted

	private void UpdateCameraModes( Camera src, Camera dest )
	{
		if( dest == null )
			return;
		// set camera to clear the same way as current camera
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;        
		if( src.clearFlags == CameraClearFlags.Skybox )
		{
			Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
			if( !sky || !sky.material )
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}
		// update other values to match current camera.
		// even if we are supplying custom camera&projection matrices,
		// some of values are used elsewhere (e.g. skybox uses far plane)
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}

	// On-demand create any objects we need
	private void CreateMirrorObjects( Camera currentCamera, out Camera reflectionCamera )
	{
		reflectionCamera = null;

		// Reflection render texture
		if( !m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize )
		{
			if( m_ReflectionTexture )
				DestroyImmediate( m_ReflectionTexture );
			m_ReflectionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
			//m_ReflectionTexture.antiAliasing = 2;
			m_ReflectionTexture.name = "__PortalView" + GetInstanceID();
			m_ReflectionTexture.isPowerOfTwo = true;
			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
			m_OldReflectionTextureSize = m_TextureSize;
		}

		// Camera for reflection
		reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
		if( !reflectionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
		{
			GameObject go = new GameObject( "Portal Camera id" + currentCamera.GetInstanceID() + " for " + gameObject.name, typeof(Camera), typeof(Skybox) );
			reflectionCamera = go.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			//reflectionCamera.transform.position = partner.transform.position;
			//reflectionCamera.transform.rotation = partner.transform.rotation;

			reflectionCamera.gameObject.AddComponent<FlareLayer>();

			go.hideFlags = HideFlags.HideAndDontSave; //TODO: enable on release? yes.
			//go.hideFlags = HideFlags.DontSave;
			m_ReflectionCameras[currentCamera] = reflectionCamera;
		}        
	}

	// Extended sign: returns -1, 0 or 1 based on sign of a
	private static float sgn(float a)
	{
		if (a > 0.0f) return 1.0f;
		if (a < 0.0f) return -1.0f;
		return 0.0f;
	}

	// Given position/normal of the plane, calculates plane in camera space.

	//DV
	//MO
	private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint( offsetPos );
		Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
		return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
	}

	// Calculates reflection matrix around the given plane
	private static void CalculateReflectionMatrix (ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
		reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
		reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
		reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

		reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
		reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
		reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
		reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);

		reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
		reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
		reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
		reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;
	}

	/*private static float getOrthongonalEdgeLength(Vector3 firstPoint, Vector3 direction, Vector3 secondPoint)
	{
		//direction = Vector3.Normalize (direction);
		float beforeBend = (Vector3.Dot (secondPoint, direction) - Vector3.Dot (firstPoint, direction)) / Vector3.Dot (direction, direction);
		//float afterBend = (secondPoint - (firstPoint + direction * beforeBend)).magnitude;
		return beforeBend;
	}*/
}