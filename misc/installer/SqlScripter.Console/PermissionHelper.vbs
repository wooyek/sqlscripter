function CheckIfReguiredUsersForWebExist
    Set oNetwork = CreateObject("WScript.Network")
    ComputerName = Cstr(oNetwork.ComputerName)
    Domain = Cstr(oNetwork.UserDomain)
    CheckIfUserExists "group", Domain, "KO_ADMINISTRATORBIZNESOWY"
    CheckIfUserExists "group", ComputerName , "IIS_WPG"
    CheckIfUserExists "user", ComputerName, "IWAM_" & ComputerName
    CheckIfUserExists "user", Domain, Session.Property("IDENTITYNAME")
    'CheckIfNetworkServiceExists
end function

function CheckIfReguiredUsersForClientExist
    Set oNetwork = CreateObject("WScript.Network")
    ComputerName = Cstr(oNetwork.ComputerName)
    Domain = Cstr(oNetwork.UserDomain)
    CheckIfUserExists "group", ComputerName, Session.Property("USERGROUP_USERS")
end function


function CheckIfUserExists (nType, domain, user)
    Set oNetwork = CreateObject("WScript.Network")
	sName = Cstr(domain)
	Set oContainer = GetObject("WinNT://" & sName)
	oContainer.Filter = Array(Cstr(nType))

	bUserExists = False
	For Each oUser in oContainer
	If lcase(trim(oUser.Name)) = lcase(trim(Cstr(user))) Then
		bUserExists = True
		Exit For
	End If
	Next
	
	If bUserExists = False Then
	    rc = MsgBox("Required " & nType & ": " & domain & "\" & user & " not found.", 0 + vbCritical, "Instalation problem")
	    Session.Property("USERNOTEXISTS") = "true"
	End If
end function