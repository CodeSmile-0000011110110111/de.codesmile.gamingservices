// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GamingServices.GUI.Elements
{
	[UxmlElement]
	public partial class PasswordTextField : TextField
	{
		private readonly Label m_ErrorLabel = new();
		private readonly Button m_UnmaskButton = new();
		private Password m_Password = new();

		[Header("Show/Hide Button")]
		[UxmlAttribute] private Texture2D MaskedIcon { get; set; }
		[UxmlAttribute] private Texture2D UnmaskedIcon { get; set; }
		[UxmlAttribute] private Vector2 IconSize { get; set; } = new(32f, 32f);

		[Header("Error Label")]
		[UxmlAttribute] private Color ErrorLabelColor { get; set; } = new(.5f, 0f, 0f);

		/// <summary>
		///     The Username instance synchronized with the text field.
		/// </summary>
		public Password Username { get => m_Password; set => m_Password = value ?? new Password(); }

		public PasswordTextField()
		{
			style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
			//maxLength = m_Password.LengthMax; // DON'T! The user might unknowingly submit a cropped password!
			isPasswordField = true;

			RegisterCallback<AttachToPanelEvent>(e =>
			{
				SetIsPasswordField(isPasswordField);
			});
			//RegisterCallback<DetachFromPanelEvent>(e => Debug.Log("detach"));

			AddErrorLabel();
			AddUnmaskButton();
			SetIsPasswordField(isPasswordField);
		}

		private void AddErrorLabel()
		{
			var m_ErrorLabel = new Label();
			Add(m_ErrorLabel);
		}

		private void AddUnmaskButton()
		{
			m_UnmaskButton.text = "*";
			m_UnmaskButton.style.position = Position.Absolute;
			m_UnmaskButton.style.right = 0f;
			m_UnmaskButton.style.alignSelf = Align.Center;
			m_UnmaskButton.style.paddingTop = m_UnmaskButton.style.paddingBottom = 0f;
			m_UnmaskButton.style.paddingLeft = m_UnmaskButton.style.paddingRight = 0f;
			m_UnmaskButton.style.marginTop = m_UnmaskButton.style.marginBottom = 2f;
			m_UnmaskButton.style.marginLeft = m_UnmaskButton.style.marginRight = 2f;
			m_UnmaskButton.style.backgroundColor = Color.clear;
			m_UnmaskButton.style.borderTopWidth = m_UnmaskButton.style.borderBottomWidth = 0f;
			m_UnmaskButton.style.borderLeftWidth = m_UnmaskButton.style.borderRightWidth = 0f;
			Add(m_UnmaskButton);

			m_UnmaskButton.UnregisterCallback<ClickEvent>(OnUnmaskButtonClicked);
			m_UnmaskButton.RegisterCallback<ClickEvent>(OnUnmaskButtonClicked);
		}

		private void OnUnmaskButtonClicked(ClickEvent evt) => SetIsPasswordField(!isPasswordField);

		private void SetIsPasswordField(Boolean isMasked)
		{
			isPasswordField = isMasked;
			SetMaskButtonBackgroundImage(isMasked);
		}

		private void SetMaskButtonBackgroundImage(Boolean isMasked)
		{
			var icon = isMasked ? MaskedIcon : UnmaskedIcon;
			m_UnmaskButton.style.backgroundImage = new StyleBackground(icon);
			m_UnmaskButton.style.width = m_UnmaskButton.style.minWidth = IconSize.x;
			m_UnmaskButton.style.height = m_UnmaskButton.style.minHeight = IconSize.y;
		}
	}
}
