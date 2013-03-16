$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$releasePath = "bin\x86\Release"

$version = (Select-String $scriptDir\..\Shared\AssemblyInfo.Version.cs -pattern 'AssemblyVersion[(]"((\d\.\d)\.\d\.\d)').Matches[0]
$longVersion = $version.Groups[1].Value
$shortVersion = $version.Groups[2].Value

function Main()
{
    rm *.zip,*.msi

    Build-Squiggle
    Update-Config
    Create-Setup
    Package-All

    write-host ------------- Done ------------------------------------
}

function Package-All()
{
    write-host ------------- Packaging ---------------------------
    & "$scriptDir\Package.ps1" Squiggle.UI\$releasePath Client $shortVersion
    copy "$scriptDir\..\Squiggle.Setup\bin\Release\Squiggle.Setup.msi" "$scriptDir\Squiggle-$shortVersion Client.msi"

    & "$scriptDir\Package.ps1" Squiggle.Bridge\$releasePath Bridge $shortVersion
    & "$scriptDir\Package.ps1" Squiggle.Multicast\$releasePath Multicast $shortVersion
    & "$scriptDir\Package.ps1" Scripts Scripts $shortVersion
}

function Create-Setup()
{
    write-host ------------- Creating setup ---------------------------
    $productName = "Squiggle $shortVersion"
    Change-Setup-Product-Name "Squiggle" $productName
    Change-Setup-Version "1.0.0.0" $longVersion
    & "$scriptDir\Build.cmd" "Squiggle.Setup\Squiggle.Setup.wixproj"
    Change-Setup-Version $longVersion "1.0.0.0"
    Change-Setup-Product-Name $productName "Squiggle"
}

function Update-Config()
{
    write-host ------------- Updating config -------------------------
    $gitHash = git rev-parse HEAD
    $configPath = "$scriptDir\..\Squiggle.UI\$releasePath\Squiggle.exe.config"
    Replace-In-File $configPath "GitHash`" value=`"`"" "GitHash`" value=`"$gitHash`""
}

function Build-Squiggle()
{
    write-host ------------- Building Squiggle $shortVersion -----------------------
    & "$scriptDir\Build.cmd" Squiggle.sln
    if (!$?) { 
        #last command (msbuild) failed
        exit
    }
}

function Change-Setup-Product-Name($from, $to)
{
    Replace-In-File "$scriptDir\..\Squiggle.Setup\Product.wxs" "Name=`"$from" "Name=`"$to"
}

function Change-Setup-Version($from, $to)
{
    Replace-In-File "$scriptDir\..\Squiggle.Setup\Product.wxs" "Version=`"$from" "Version=`"$to"
}

function Replace-In-File($filePath, $find, $replace)
{
    (get-content $filePath) -replace $find, $replace | set-content $filePath
}

Main