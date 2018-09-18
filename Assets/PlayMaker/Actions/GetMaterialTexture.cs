// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.
// Thanks to: Giyomu
// http://hutonggames.com/playmakerforum/index.php?topic=401.0

using UnityEngine;
using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Material)]
[HutongGames.PlayMaker.Tooltip("Get a texture from a material on a GameObject")]
public class GetMaterialTexture : FsmStateAction
{
	[RequiredField]
	[CheckForComponent(typeof(Renderer))]
	public FsmOwnerDefault gameObject;
	public FsmInt materialIndex;
	[UIHint(UIHint.NamedTexture)]
	public FsmString namedTexture;
	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmTexture storedTexture;
	public bool getFromSharedMaterial;

	public override void Reset()
	{
		gameObject = null;
		materialIndex = 0;
		namedTexture = "_MainTex";
		storedTexture = null;
		getFromSharedMaterial = false;
	}
	
	public override void OnEnter ()
	{
		DoGetMaterialTexture();
		Finish();
	}
	
	void DoGetMaterialTexture()
	{
		var go = Fsm.GetOwnerDefaultTarget(gameObject);
		if (go == null)
		{
			return;
		}

		if (go.GetComponent<Renderer>() == null)
		{
			LogError("Missing Renderer!");
			return;
		}
		
		string namedTex = namedTexture.Value;
		if (namedTex == "")
		{
			namedTex = "_MainTex";
		}
		
		if (materialIndex.Value == 0 && !getFromSharedMaterial)
		{
			storedTexture.Value = go.GetComponent<Renderer>().material.GetTexture(namedTex);
		}
		
		else if(materialIndex.Value == 0 && getFromSharedMaterial)
		{
			storedTexture.Value = go.GetComponent<Renderer>().sharedMaterial.GetTexture(namedTex);
		}
		
		else if (go.GetComponent<Renderer>().materials.Length > materialIndex.Value && !getFromSharedMaterial)
		{
			var materials = go.GetComponent<Renderer>().materials;
			storedTexture.Value = go.GetComponent<Renderer>().materials[materialIndex.Value].GetTexture(namedTex);
			go.GetComponent<Renderer>().materials = materials;
		}
		
		else if (go.GetComponent<Renderer>().materials.Length > materialIndex.Value && getFromSharedMaterial)
		{
			var materials = go.GetComponent<Renderer>().sharedMaterials;
			storedTexture.Value = go.GetComponent<Renderer>().sharedMaterials[materialIndex.Value].GetTexture(namedTex);
			go.GetComponent<Renderer>().materials = materials;
		}
	}
}
