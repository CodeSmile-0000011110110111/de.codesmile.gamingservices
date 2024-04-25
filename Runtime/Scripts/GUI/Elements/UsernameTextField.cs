// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.GamingServices.Authentication;
using System;
using System.Text.RegularExpressions;
using Unity.Services.Authentication;
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
			maxLength = Username.LengthMax;

			m_ErrorLabel.style.color = ErrorLabelColor;
			Add(m_ErrorLabel);

			RegisterCallback<AttachToPanelEvent>(e => RegisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<DetachFromPanelEvent>(e => UnregisterCallback<InputEvent>(OnUserInput));
			RegisterCallback<TooltipEvent>(evt => UpdateTooltip(m_Username));
			RegisterCallback<FocusInEvent>(evt => ShowErrorLabel(!m_Username.IsValid));
			RegisterCallback<FocusOutEvent>(evt => ShowErrorLabel(false));
		}

		protected virtual void OnUserInput(InputEvent evt)
		{
			m_Username.Name = text;

			if (SanitizeInput)
			{
				m_Username.Name = m_Username.SanitizedName;
				SetValueWithoutNotify(m_Username.Name);
			}
			else
				UpdateErrorLabel(m_Username);
		}

		protected virtual void UpdateTooltip(Username username)
		{
			var validSymbols = Regex.Unescape(username.ValidSymbols);
			tooltip = $"Username is case insensitive; between {username.LengthMin} to {username.LengthMax} characters; " +
			          $"english alphabet letters, digits, and the symbols {validSymbols} are valid.";
		}

		protected virtual void UpdateErrorLabel(Username username)
		{
			ShowErrorLabel(SanitizeInput == false && !username.IsValid);
			m_ErrorLabel.text = username.IsValid ? String.Empty : username.State.ToString();
		}

		protected void ShowErrorLabel(Boolean show)
		{
			var displayStyle = show ? DisplayStyle.Flex : DisplayStyle.None;
			m_ErrorLabel.style.display = new StyleEnum<DisplayStyle>(displayStyle);
		}
	}
}
