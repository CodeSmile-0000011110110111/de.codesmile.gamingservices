// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.Text.RegularExpressions;
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
		[UxmlAttribute] private Vector2 MaskIconSize { get; set; } = new(32f, 32f);

		[Header("Error Label")]
		[UxmlAttribute] private Color ErrorLabelColor { get; set; } = new(.5f, 0f, 0f);

		/// <summary>
		///     The Username instance synchronized with the text field.
		/// </summary>
		public Password Password { get => m_Password; set => m_Password = value ?? new Password(); }

		public PasswordTextField()
		{
			style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
			//maxLength = m_Password.LengthMax; // DON'T! The user might unknowingly submit a cropped password!
			textEdition.isPassword = true;

			RegisterCallback<AttachToPanelEvent>(e =>
			{
				SetIsPassword(textEdition.isPassword);
				RegisterCallback<InputEvent>(OnUserInput);
			});
			RegisterCallback<DetachFromPanelEvent>(e =>
			{
				UnregisterCallback<InputEvent>(OnUserInput);
			});
			RegisterCallback<TooltipEvent>(evt => UpdateTooltip(m_Password));

			AddErrorLabel();
			AddUnmaskButton();
			SetIsPassword(textEdition.isPassword);
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

		protected virtual void OnUserInput(InputEvent evt)
		{
			m_Password.Value = text;
			UpdateErrorLabel(m_Password);
		}

		protected virtual void UpdateTooltip(Password password)
		{
			var requires = password.Requires;
			var hasRequirements = requires.LowercaseCount > 0 || requires.UppercaseCount > 0 || requires.DigitCount > 0 ||
			                      requires.SymbolCount > 0;
			var validSymbols = Regex.Unescape(requires.ValidSymbols);
			tooltip = $"Password is case sensitive; between {requires.MinLength} to {requires.MaxLength} characters; " +
			          $"valid characters are english alphabet letters, digits, and the symbols {validSymbols}" +
			          (hasRequirements
				          ? "\n\nRequires at least:\n" +
				            (requires.LowercaseCount > 0 ? $"{requires.LowercaseCount} lowercase letter(s)\n" : "") +
				            (requires.UppercaseCount > 0 ? $"{requires.UppercaseCount} uppercase letter(s)\n" : "") +
				            (requires.DigitCount > 0 ? $"{requires.DigitCount} digit(s)\n" : "") +
				            (requires.SymbolCount > 0 ? $"{requires.SymbolCount} symbol(s)" : "")
				          : "");
		}

		protected virtual void UpdateErrorLabel(Password password)
		{
			var validationState = password.Validate();
			var isValid = validationState == Password.ValidationState.Valid;
			m_ErrorLabel.text = isValid ? String.Empty : validationState.ToString();
			ShowErrorLabel(!isValid);
		}

		protected void ShowErrorLabel(Boolean show)
		{
			var displayStyle = show ? DisplayStyle.Flex : DisplayStyle.None;
			m_ErrorLabel.style.display = new StyleEnum<DisplayStyle>(displayStyle);
		}

		private void OnUnmaskButtonClicked(ClickEvent evt) => SetIsPassword(!textEdition.isPassword);

		private void SetIsPassword(Boolean isMasked)
		{
			textEdition.isPassword = isMasked;
			SetMaskButtonBackgroundImage(isMasked);
		}

		private void SetMaskButtonBackgroundImage(Boolean isMasked)
		{
			var icon = isMasked ? MaskedIcon : UnmaskedIcon;
			m_UnmaskButton.style.backgroundImage = new StyleBackground(icon);
			m_UnmaskButton.style.width = m_UnmaskButton.style.minWidth = MaskIconSize.x;
			m_UnmaskButton.style.height = m_UnmaskButton.style.minHeight = MaskIconSize.y;
		}
	}
}
