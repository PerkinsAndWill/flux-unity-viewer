﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class GeomParser : MonoBehaviour {

	public string PublicDataKey;
	Mesh mesh;

	bool flippedXAndZ = false; 

	List<Vector3> vertices = new List<Vector3>();
	List<Vector2> uv = new List<Vector2> ();
	List<int> triangles = new List<int>();

	float smallestX;
	float smallestY;
	float smallestZ;
	float largestX;
	float largestY;
	float largestZ;
	float maximumEdgeSize;
	public float MaxSize = 1;
	bool smallestCoordsUnset;

	public static bool repositioningGeometry = false;

	public void MakeGeometry(JSONObject unknownJson) {
		Mesh mesh = new Mesh();
		gameObject.GetComponent<MeshFilter> ().mesh = mesh;
		mesh.Clear();

		// Reset old values to prevent weird geometry from appearing
		vertices.ForEach((Vector3 obj) => obj = new Vector3(0,0,0));
		triangles.ForEach((int obj) => obj = 0);

		vertices = new List<Vector3> (); 
		triangles = new List<int> ();

		// If the user wishes to reposition the geometry close to them,
		// we have to check what the smallest X, Y and Z values are.
		smallestCoordsUnset = !repositioningGeometry;
		if (repositioningGeometry) {
			RecursiveGeometrySizingPass (unknownJson);
			maximumEdgeSize = largestX - smallestX;
			maximumEdgeSize = (maximumEdgeSize < (largestY - smallestY)) ? largestY - smallestY : maximumEdgeSize;
			maximumEdgeSize = (maximumEdgeSize < (largestZ - smallestZ)) ? largestZ - smallestZ : maximumEdgeSize;
		}

		// Records geometry to vertices and triangles lists.
		RecursiveGeometryRecord (unknownJson);

		// Creating an array and assigning mesh.vertices to it is 
		// much faster because of code 'behind the scenes'
		// http://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);

		Vector2[] uv = new Vector2[vertices.Count];
		for (int i = 0; i < uv.Length; i++) {
			uv [i] = new Vector2 (vertices [i].x, vertices [i].y + vertices[i].z);
		}
		mesh.uv = uv;

		mesh.RecalculateNormals ();

		gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
		gameObject.GetComponent<MeshFilter> ().mesh = mesh;

		// Flux flips X and Z. The first time geometry is loaded, the 
		if (!flippedXAndZ) {
			flippedXAndZ = true;
			transform.position = new Vector3 (0, 0, 0);
			transform.eulerAngles = (new Vector3 (270, 0, 0));
		}
	}

	void RecursiveGeometryRecord(JSONObject unknownJson) {
		if (unknownJson.type == JSONObject.Type.ARRAY) {
			for (int y = 0; y < unknownJson.list.Count; y++) {
				RecursiveGeometryRecord (unknownJson.list [y]);
			}
		} else if (unknownJson.type == JSONObject.Type.OBJECT) {
			bool itsAMesh = false;
			int facesIdx = 0;
			int verticesIdx = 0;
			for (int i = 0; i < unknownJson.keys.Count; i++) {
				if (unknownJson.keys [i] == "vertices") {
					verticesIdx = i;
					itsAMesh = true;
				} else if (unknownJson.keys [i] == "faces") {
					facesIdx = i;
				}
			}

			if (itsAMesh) {
				// We have to add the face # to the current total # of vertices.
				JSONObject faces = unknownJson.list [facesIdx];
				for (int z = 0; z < faces.list.Count; z++) {
					JSONObject face = faces.list [z];

					if ( face.list.Count == 3 ) {
						triangles.Add ((int)face.list [0].n + vertices.Count);
						triangles.Add ((int)face.list [1].n + vertices.Count);
						triangles.Add ((int)face.list [2].n + vertices.Count);
						// Do it again for the second side of the mesh
						triangles.Add ((int)face.list [2].n + vertices.Count);
						triangles.Add ((int)face.list [1].n + vertices.Count);
						triangles.Add ((int)face.list [0].n + vertices.Count);
					} else if ( face.list.Count > 3 ) {
						for ( var j=0; j+2<face.list.Count; j++) {
							triangles.Add ((int)face.list [0].n + vertices.Count);
							triangles.Add ((int)face.list [j+1].n + vertices.Count);
							triangles.Add ((int)face.list [j+2].n + vertices.Count);
							// Do it again for the second side of the mesh
							triangles.Add ((int)face.list [j+2].n + vertices.Count);
							triangles.Add ((int)face.list [j+1].n + vertices.Count);
							triangles.Add ((int)face.list [0].n + vertices.Count);
						}
					}

					for (int u = 0; u < face.list.Count; u++) {
						triangles.Add ((int)face.list [u].n + vertices.Count);       
					}

					// if triangles isn't divisible by 3, add some padding.
					for (int xy = 0; xy < triangles.Count % 3; xy++) {
						triangles.Add (triangles[triangles.Count-1]);
					}
				}

				JSONObject inspectedVertices = unknownJson.list [verticesIdx];
				for (int i = 0; i < inspectedVertices.list.Count; i++) {
					JSONObject vertex = inspectedVertices.list [i];

					float multiplier = .06f;
					multiplier *= MaxSize;
						
					if (repositioningGeometry) {
						// Y axis is flipped, for some reason.
						vertices.Add (new Vector3 (
							((vertex.list [0].n) * multiplier) / maximumEdgeSize - smallestX, 
							(((vertex.list [1].n) * multiplier) / maximumEdgeSize - smallestY) * -1, 
							((vertex.list [2].n) * multiplier) / maximumEdgeSize - smallestZ));
					} else {
						// Y axis still flipped.
						vertices.Add (new Vector3 (
							vertex.list [0].n * multiplier,
							vertex.list [1].n * multiplier * -1,
							vertex.list [2].n * multiplier));
					}
				}       
			}
		}
	}

	// Find the lowest x, y and z points.
	// Note: this function is not used because it messes up overlapping data keys.
	void RecursiveGeometrySizingPass(JSONObject unknownJson) {
		if (unknownJson.type == JSONObject.Type.ARRAY) {
			for (int y = 0; y < unknownJson.list.Count; y++) {
				RecursiveGeometrySizingPass (unknownJson.list [y]);
			}

		} else if (unknownJson.type == JSONObject.Type.OBJECT) {
			bool itsAMesh = false;
			int verticesIdx = 0;
			for (int i = 0; i < unknownJson.keys.Count; i++) {
				if (unknownJson.keys [i] == "vertices") {
					verticesIdx = i;
					itsAMesh = true;
				} 
			}

			if (itsAMesh) {
				JSONObject inspectedVertices = unknownJson.list [verticesIdx];
				for (int i = 0; i < inspectedVertices.list.Count; i++) {
					JSONObject vertex = inspectedVertices.list [i];
					if (smallestCoordsUnset || vertex.list[0].n < smallestX) smallestX = vertex.list[0].n;
					if (smallestCoordsUnset || vertex.list[1].n < smallestY) smallestY = vertex.list[1].n;
					if (smallestCoordsUnset || vertex.list[2].n < smallestZ) smallestZ = vertex.list[2].n;
					if (smallestCoordsUnset || vertex.list[0].n > largestX) largestX = vertex.list[0].n;
					if (smallestCoordsUnset || vertex.list[1].n > largestY) largestY = vertex.list[1].n;
					if (smallestCoordsUnset || vertex.list[2].n > largestZ) largestZ = vertex.list[2].n;
					smallestCoordsUnset = false;
				}       
			}
		}
	}

	public void SwitchRepositioning() {
		repositioningGeometry = !repositioningGeometry;
	}
}
