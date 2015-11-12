# november 12, 2015 | soren granfeldt
#	- version 1.0.0.0947
param
(
)

$maxmlfilename = "Granfeldt.SQL.MA.xml"
$mafilename = "Granfeldt.SQL.MA.dll"
try
{
	$location = get-itemproperty "hklm:\software\microsoft\forefront identity manager\2010\synchronization service" -erroraction stop | select -expand location
}
catch
{
	write-error "Cannot get FIM/MIM installation folder path from registry"
	break
}

$location = join-path $location "synchronization service"
write-debug "install-location: $location"
$extensionsfolder = join-path $location "extensions"
write-debug "install-location: $extensionsfolder"
$packagedmafolder = join-path $location "uishell\xmls\packagedmas"
write-debug "install-location: $packagedmafolder"

write-debug "copying $mafilename to $extensionsfolder"
copy-item "$mafilename" -destination "$extensionsfolder"
write-debug "copying $maxmlfilename to $packagedmafolder"
copy-item "$maxmlfilename" -destination "$packagedmafolder"
