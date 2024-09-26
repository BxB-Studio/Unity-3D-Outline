#region Namespaces

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Utilities.VFX
{
	public class Outline3D : MonoBehaviour
	{
		#region Constants

		public static readonly Color DefaultColor = new(1f / 3f, 1f, .25f, 1f);
		public static readonly Color RedColor = new(1f, .25f, .25f, 1f);

		private const string OutlineShaderName = "Highlight Outline";
		private const string OutlineShaderColorPropertyName = "_Color";
		private const string OutlineShaderEnabledPropertyName = "_Enabled";
		private const string OutlineShaderThicknessPropertyName = "_Thickness";

		#endregion

		#region Variables

		// Use these fields to setup the outline settings before starting the game.
		#region Fields

		[SerializeField]
		private Material material;
		[SerializeField]
		private Color color = DefaultColor;
		[SerializeField]
		private float thickness = .05f;
		[SerializeField]
		private bool enableOnAwake = true;

		#endregion

		// Use these public properties to control the outline at runtime.
		#region Properties

		// Sets a new material for the outline and apply the current settings
		public Material OutlineMaterial
		{
			get => material;
			set
			{
				material = value;

				if (!outlineInitialized)
					return;

				ApplyMaterial();
			}
		}
		// Color of the outline
		public Color Color
		{
			get => outlineInitialized ? currentColor : color;
			set
			{
				if (!outlineInitialized)
				{
					color = value;

					return;
				}

				materialInstance.SetColor(OutlineShaderColorPropertyName, value);

				currentColor = value;
			}
		}
		// Thickness of the outline; Measured in Unity world space units
		public float Thickness
		{
			get => outlineInitialized ? currentThickness : thickness;
			set
			{
				if (!outlineInitialized)
				{
					thickness = value;

					return;
				}

				materialInstance.SetFloat(OutlineShaderThicknessPropertyName, value);

				currentThickness = value;
			}
		}
		// Enables & Disables the outline
		public bool Show
		{
			get => outlineInitialized ? show : enableOnAwake;
			set
			{
				if (!outlineInitialized)
				{
					enableOnAwake = value;

					return;
				}

				materialInstance.SetInt(OutlineShaderEnabledPropertyName, value ? 1 : 0);

				show = value;
			}
		}

		private MeshRenderer[] renderers; // The mesh renderers attached to our gameobject
		private Material materialInstance; // The material instance used for our renderers
		private Color currentColor; // The current color of the material instance
		private float currentThickness; // The current thickness of the outline
		private bool outlineInitialized; // A flag to check whether settings have been setup on awake or not
		private int[] materialIndices; // The indices of outline material instances of each mesh renderer
		private bool show; // The current state of the outline

		#endregion

		#endregion

		#region Utilities

		// Sets the red channel of the outline color
		public void SetColorRed(float red)
		{
			if (!outlineInitialized)
				return;

			currentColor.r = red;

			Color = currentColor;
		}
		// Sets the green channel of the outline color
		public void SetColorGreen(float green)
		{
			if (!outlineInitialized)
				return;

			currentColor.g = green;

			Color = currentColor;
		}
		// Sets the blue channel of the outline color
		public void SetColorBlue(float blue)
		{
			if (!outlineInitialized)
				return;

			currentColor.b = blue;

			Color = currentColor;
		}
		// Sets the alpha channel of the outline color
		public void SetColorAlpha(float alpha)
		{
			if (!outlineInitialized)
				return;

			currentColor.a = alpha;

			Color = currentColor;
		}

		// Checks whether the original shared material is valid or not
		private bool ValidateMaterial()
		{
			if (!material)
				throw new NullReferenceException("You have to assign an `Outline Material` before starting play mode!");
			else if (!material.shader.name.EndsWith($"/{OutlineShaderName}"))
			{
				Debug.LogWarning($"The Outline Highlighter material attached to \"{name}\" might have an invalid shader!");

				return false;
			}

			return true;
		}
		// Adds or Sets the shared material to all renderers
		private void ApplyMaterial()
		{
			// Create a new instance of the shared material
			materialInstance = new(material)
			{
				name = $"{material.name} (Clone)",
			};

			// Apply material instance to all renderers
			for (int i = 0; i < renderers.Length; i++)
			{
				MeshRenderer renderer = renderers[i];
				List<Material> materials = new(renderer.sharedMaterials.Length + 1);
				int materialIndex = materialIndices[i];

				renderer.GetSharedMaterials(materials);

				// If the material index is invalid we add the new material instance, otherwise we set the material instance directly
				if (materialIndex < 0)
				{
					materialIndices[i] = materials.Count;

					materials.Add(materialInstance);
				}
				else
					materials[materialIndex] = materialInstance;

				renderer.sharedMaterials = materials.ToArray();
			}
		}

		#endregion

		#region Methods

		private void Awake()
		{
			if (!ValidateMaterial())
				return;

			// Find all children Mesh Renderers
			renderers = GetComponentsInChildren<MeshRenderer>();
			// Initialize the materialIndices array with invalid indices
			materialIndices = Enumerable.Repeat(-1, renderers.Length).ToArray();

			// Add the outline material to all mesh renderers of our game object
			ApplyMaterial();

			// Apply awake settings
			outlineInitialized = true;
			Show = enableOnAwake;
			Thickness = thickness;
			Color = color;
		}

		#endregion
	}
}
