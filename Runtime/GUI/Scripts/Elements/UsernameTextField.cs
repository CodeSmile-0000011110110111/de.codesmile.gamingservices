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
	/// <summary>
	///     Username text field that either allows only valid input (replacing invalid chars on the fly) or
	///     displays a label if the input is invalid, indicating what's wrong (too short/long, invalid symbol).
	/// </summary>
	[UxmlElement]
	public partial class UsernameTextField : VisualElement
	{
		private const String ElementPath = ResourcePath.UiElementsRoot + nameof(UsernameTextField);
		private const String TextFieldName = "username-textfield";
		private const String ErrorLabelName = "error-label";

		private readonly TextField m_TextField;
		private readonly Label m_ErrorLabel;
		private Username m_Username;

		[Header("Input Handling")]
		/// <summary>
		///     If true, typing invalid characters replaces them with a valid symbol. Otherwise, invalid input is allowed
		///		and while Username is invalid, an error label is shown.
		/// </summary>
		[Tooltip("If checked, typing invalid characters replaces them with a valid symbol. If unchecked, " +
		         "invalid input is allowed and while Username is invalid, an error label is shown.")]
		[UxmlAttribute] private Boolean SanitizeInput { get; set; } = true;

		/// <summary>
		///     The Username instance synchronized with the text field.
		/// </summary>
		public Username Username { get => m_Username; set => m_Username = value ?? new Username(); }

		public UsernameTextField()
			: this(null) {}

		public UsernameTextField(Username username = null)
		{
			m_Username = username ?? new Username();

			LoadDocumentAndStylesheet();
			m_TextField = this.Q<TextField>(TextFieldName) ?? throw new MissingReferenceException(TextFieldName);
			m_ErrorLabel = this.Q<Label>(ErrorLabelName) ?? throw new MissingReferenceException(ErrorLabelName);

			m_TextField.maxLength = m_Username.Requires.MaxLength;
			ShowErrorLabel(false);

			RegisterCallback<AttachToPanelEvent>(e => RegisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<DetachFromPanelEvent>(e => UnregisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<TooltipEvent>(evt => UpdateTooltip(m_Username));
			RegisterCallback<FocusInEvent>(evt => UpdateErrorLabel(m_Username));
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
			m_Username.Value = m_TextField.text;

			if (SanitizeInput)
			{
				m_Username.Value = m_Username.GetSanitized();
				m_TextField.SetValueWithoutNotify(m_Username.Value);
			}
			else
				UpdateErrorLabel(m_Username);
		}

		protected virtual void UpdateErrorLabel(Username username)
		{
			var validationState = username.Validate();
			var isValid = validationState == Username.ValidationState.Valid;
			m_ErrorLabel.text = isValid ? String.Empty : validationState.ToString();
			ShowErrorLabel(!SanitizeInput && !isValid && username.Value != String.Empty);
		}

		protected void ShowErrorLabel(Boolean show) =>
			m_ErrorLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;

		protected virtual void UpdateTooltip(Username username)
		{
			var requires = username.Requires;
			var validSymbols = Regex.Unescape(requires.ValidSymbols);
			tooltip = $"Username is case insensitive; between {requires.MinLength} to {requires.MaxLength} characters; " +
			          $"valid characters are english alphabet letters, digits, and the symbols {validSymbols}";
		}
	}
}
