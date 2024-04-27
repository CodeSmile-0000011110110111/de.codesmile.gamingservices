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
	/// <summary>
	///     Username text field that either allows only valid input (replacing invalid chars on the fly) or
	///     displays a label if the input is invalid, indicating what's wrong (too short/long, invalid symbol).
	/// </summary>
	[UxmlElement]
	public partial class UsernameTextField : TextField
	{
		private readonly Label m_ErrorLabel = new();
		private Username m_Username = new();

		[Header("Input Handling")]
		/// <summary>
		///     If true, typing invalid characters replaces them with a valid symbol. Otherwise, invalid input is allowed
		///		and while Username is invalid, an error label is shown.
		/// </summary>
		[Tooltip("If checked, typing invalid characters replaces them with a valid symbol. If unchecked, " +
		         "invalid input is allowed and while Username is invalid, an error label is shown.")]
		[UxmlAttribute] private Boolean SanitizeInput { get; set; } = true;
		[Header("Error Label")]
		[UxmlAttribute] private Color ErrorLabelColor { get; set; } = new(.5f, 0f, 0f);

		/// <summary>
		///     The Username instance synchronized with the text field.
		/// </summary>
		public Username Username { get => m_Username; set => m_Username = value ?? new Username(); }

		public UsernameTextField()
		{
			style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column);
			maxLength = Username.Requires.MaxLength;

			m_ErrorLabel.style.color = ErrorLabelColor;
			Add(m_ErrorLabel);

			RegisterCallback<AttachToPanelEvent>(e => RegisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<DetachFromPanelEvent>(e => UnregisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<TooltipEvent>(evt => UpdateTooltip(m_Username));
			RegisterCallback<FocusInEvent>(evt => UpdateErrorLabel(m_Username));
			RegisterCallback<FocusOutEvent>(evt => ShowErrorLabel(false));
		}

		protected virtual void OnUserInput(InputEvent evt)
		{
			m_Username.Value = text;

			if (SanitizeInput)
			{
				m_Username.Value = m_Username.GetSanitized();
				SetValueWithoutNotify(m_Username.Value);
			}
			else
				UpdateErrorLabel(m_Username);
		}

		protected virtual void UpdateTooltip(Username username)
		{
			var requires = username.Requires;
			var validSymbols = Regex.Unescape(requires.ValidSymbols);
			tooltip = $"Username is case insensitive; between {requires.MinLength} to {requires.MaxLength} characters; " +
			          $"valid characters are english alphabet letters, digits, and the symbols {validSymbols}";
		}

		protected virtual void UpdateErrorLabel(Username username)
		{
			var validationState = username.Validate();
			var isValid = validationState == Username.ValidationState.Valid;
			m_ErrorLabel.text = isValid ? String.Empty : validationState.ToString();
			ShowErrorLabel(!SanitizeInput && !isValid);
		}

		protected void ShowErrorLabel(Boolean show)
		{
			var displayStyle = show ? DisplayStyle.Flex : DisplayStyle.None;
			m_ErrorLabel.style.display = new StyleEnum<DisplayStyle>(displayStyle);
		}
	}
}
