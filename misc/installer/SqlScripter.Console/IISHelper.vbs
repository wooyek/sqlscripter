function CheckIfWebAppExists
	'MsgBox Session.Property("WEBAPPNAME")
	Dim objIIS 'ADSI IIS Object
	On Error Resume Next
	Set objIIS = GetObject("IIS://localhost/" & Session.Property("WEBSITEPATH") & "/" & Session.Property("WEBAPPNAME"))
	If Err.Number = 0 Then
		Session.Property("WEBAPPEXISTS") = "true"
	Else
		Session.Property("WEBAPPEXISTS") = "false"
	End If
	Set objIIS = Nothing
	'MsgBox Session.Property("WEBAPPEXISTS")
end function

function ChangeWebAppLocalDirectory
	Dim objIIS 'ADSI IIS Object
	Set objIIS = GetObject("IIS://localhost/" & Session.Property("WEBSITEPATH") & "/" & Session.Property("WEBAPPNAME"))
	objIIS.Path = Session.Property("INSTALLDIR")
	objIIS.SetInfo
end function