$myDir = Split-Path $script:MyInvocation.MyCommand.Path
$version = (Select-String $myDir\..\Shared\AssemblyInfo.Version.cs -pattern 'AssemblyVersion[(]"(\d\.\d)').Matches[0].Groups[1].Value
write-host ------------- Building Squiggle $version -----------------------
& "$myDir\Build.cmd"
if (!$?) { 
    #last command (msbuild) failed
    exit
}
write-host ------------- Zipping flies ---------------------------
& "$myDir\Package.cmd" Squiggle.UI\bin\x86\Release Client $version
& "$myDir\Package.cmd" Squiggle.Bridge\bin\x86\Release Bridge $version
& "$myDir\Package.cmd" Squiggle.Multicast\bin\x86\Release Multicast $version
& "$myDir\Package.cmd" Scripts Scripts $version
write-host ------------- Done ------------------------------------