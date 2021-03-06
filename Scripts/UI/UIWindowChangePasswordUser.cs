﻿// =======================================================================================
// Wovencore
// by Weaver (Fhiz)
// MIT licensed
// =======================================================================================

using wovencode;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace wovencode
{

	// ===================================================================================
	// UIWindowChangePasswordUser
	// ===================================================================================
	public partial class UIWindowChangePasswordUser : UIRoot
	{
	
		[Header("Window")]
		public Text statusText;
		
		[Header("Input Fields")]
		public InputField usernameInput;
		public InputField oldUserPassInput;
		public InputField newUserPassInput;
		
		[Header("Buttons")]
		public Button changeButton;
		public Button cancelButton;
		public Button backButton;
		
		// -------------------------------------------------------------------------------
		// ThrottledUpdate
		// -------------------------------------------------------------------------------
		protected override void ThrottledUpdate()
		{
			
			if (NetworkManager.singleton.state == NetworkState.Game)
			{
				Hide();
				return;
			}
			
			if (NetworkManager.singleton.IsConnecting())
				statusText.text = "Connecting...";
			else if (!Tools.IsAllowedName(usernameInput.text) || !Tools.IsAllowedPassword(oldUserPassInput.text) || !Tools.IsAllowedPassword(newUserPassInput.text))
				statusText.text = "Check Name/Password";
			else
				statusText.text = "";
			
			usernameInput.readOnly = !NetworkManager.singleton.CanInput();
			oldUserPassInput.readOnly = !NetworkManager.singleton.CanInput();
			newUserPassInput.readOnly = !NetworkManager.singleton.CanInput();
			
			changeButton.interactable = NetworkManager.singleton.CanChangePasswordUser(usernameInput.text, oldUserPassInput.text, newUserPassInput.text);
			changeButton.onClick.SetListener(() => { NetworkManager.singleton.TryChangePasswordUser(usernameInput.text, oldUserPassInput.text, newUserPassInput.text); });
		
			cancelButton.gameObject.SetActive(NetworkManager.singleton.CanCancel());
			cancelButton.onClick.SetListener(() => { NetworkManager.singleton.TryCancel(); });
		
			backButton.onClick.SetListener(() => { Hide(); });

		}
		
		// -------------------------------------------------------------------------------
		
	}

}

// =======================================================================================