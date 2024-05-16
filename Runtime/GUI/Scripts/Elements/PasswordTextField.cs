// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeSmile.GamingServices.GUI.Elements
{
	[UxmlElement]
	public partial class PasswordTextField : VisualElement
	{
		private const String ElementPath = ResourcePath.UiElementsRoot + nameof(PasswordTextField);
		private const String TextFieldName = "password-textfield";
		private const String UnmaskButtonName = "unmask-button";
		private const String ErrorLabelName = "error-label";

		private readonly TextField m_TextField;
		private readonly Label m_ErrorLabel;
		private readonly Button m_UnmaskButton;
		private readonly Password m_Password;

		[Header("Show/Hide Button")]
		[UxmlAttribute] private Texture2D MaskedIcon { get; set; }
		[UxmlAttribute] private Texture2D UnmaskedIcon { get; set; }
		[UxmlAttribute] private Vector2 MaskIconSize { get; set; } = new(32f, 32f);

		[Header("Error Label")]
		[UxmlAttribute] private Color ErrorLabelColor { get; set; } = new(.5f, 0f, 0f);

		/// <summary>
		///     The Username instance synchronized with the text field.
		/// </summary>
		public Password Password => m_Password;

		public PasswordTextField()
			: this(null) {}

		public PasswordTextField(Password password)
		{
			m_Password = password ?? new Password();

			LoadDocumentAndStylesheet();
			m_TextField = this.Q<TextField>(TextFieldName) ?? throw new MissingReferenceException(TextFieldName);
			m_UnmaskButton = this.Q<Button>(UnmaskButtonName) ?? throw new MissingReferenceException(UnmaskButtonName);
			m_ErrorLabel = this.Q<Label>(ErrorLabelName) ?? throw new MissingReferenceException(ErrorLabelName);

			m_TextField.SetValueWithoutNotify(m_Password.Value ?? String.Empty);
			UpdateErrorLabel(m_Password);

			RegisterCallback<AttachToPanelEvent>(e =>
			{
				RegisterCallback<InputEvent>(OnUserInput);
				m_UnmaskButton.RegisterCallback<ClickEvent>(OnUnmaskButtonClicked);
				SetIsPassword(m_TextField.textEdition.isPassword);
			});
			RegisterCallback<DetachFromPanelEvent>(e =>
			{
				UnregisterCallback<InputEvent>(OnUserInput);
				m_UnmaskButton.UnregisterCallback<ClickEvent>(OnUnmaskButtonClicked);
				ShowErrorLabel(false);
			});
			RegisterCallback<TooltipEvent>(evt => UpdateTooltip(m_Password));
			//RegisterCallback<FocusInEvent>(evt => UpdateErrorLabel(m_Password));
		}

		private void LoadDocumentAndStylesheet()
		{
			var uxml = Resources.Load<VisualTreeAsset>(ElementPath) ?? throw new FileNotFoundException("uxml", ElementPath);
			var uss = Resources.Load<StyleSheet>(ElementPath) ?? throw new FileNotFoundException("uss", ElementPath);

			uxml.CloneTree(this);
			styleSheets.Add(uss);
		}

		protected virtual void OnUserInput(InputEvent evt)
		{
			m_Password.Value = m_TextField.text;
			UpdateErrorLabel(m_Password);
		}

		protected virtual void UpdateErrorLabel(Password password)
		{
			var validationState = password.Validate();
			var isValid = validationState == Password.ValidationState.Valid;
			m_ErrorLabel.text = isValid ? String.Empty : validationState.ToString();
			ShowErrorLabel(!isValid && password.Value != String.Empty);
		}

		protected void ShowErrorLabel(Boolean show) =>
			m_ErrorLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

		private void OnUnmaskButtonClicked(ClickEvent evt) => SetIsPassword(!m_TextField.textEdition.isPassword);

		private void SetIsPassword(Boolean isMasked)
		{
			m_TextField.textEdition.isPassword = isMasked;
			SetMaskButtonBackgroundImage(isMasked);
		}

		private void SetMaskButtonBackgroundImage(Boolean isMasked)
		{
			m_UnmaskButton.style.backgroundImage = isMasked ? MaskedIcon : UnmaskedIcon;
			m_UnmaskButton.style.width = m_UnmaskButton.style.minWidth = MaskIconSize.x;
			m_UnmaskButton.style.height = m_UnmaskButton.style.minHeight = MaskIconSize.y;
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
	}
}
