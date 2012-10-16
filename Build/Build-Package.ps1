$myDir = Split-Path $script:MyInvocation.MyCommand.Path
$releasePath = "bin\x86\Release"
$version = (Select-String $myDir\..\Shared\AssemblyInfo.Version.cs -pattern 'AssemblyVersion[(]"(\d\.\d)').Matches[0].Groups[1].Value

write-host ------------- Building Squiggle $version -----------------------
& "$myDir\Build.cmd" Squiggle.sln
if (!$?) { 
    #last command (msbuild) failed
    exit
}

write-host ------------- Updating config -------------------------
$gitHash = git rev-parse HEAD
$configPath = "$myDir\..\Squiggle.UI\$releasePath\Squiggle.exe.config"
(get-content $configPath) -replace "GitHash`" value=`"`"", "GitHash`" value=`"$gitHash`"" | set-content $configPath

write-host ------------- Creating setup ---------------------------
& "$myDir\Build.cmd" Squiggle.Setup\Squiggle.Setup.wixproj

write-host ------------- Packaging ---------------------------
& "$myDir\Package.cmd" Squiggle.UI\$releasePath Client $version
copy "$myDir\..\Squiggle.Setup\bin\Release\Squiggle.Setup.msi" "$myDir\Squiggle-$version Client.msi"

& "$myDir\Package.cmd" Squiggle.Bridge\$releasePath Bridge $version
& "$myDir\Package.cmd" Squiggle.Multicast\$releasePath Multicast $version
& "$myDir\Package.cmd" Scripts Scripts $version

write-host ------------- Done ------------------------------------