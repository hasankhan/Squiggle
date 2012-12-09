$myDir = Split-Path $script:MyInvocation.MyCommand.Path
$releasePath = "bin\x86\Release"

$version = (Select-String $myDir\..\Shared\AssemblyInfo.Version.cs -pattern 'AssemblyVersion[(]"((\d\.\d)\.\d\.\d)').Matches[0]
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
    & "$myDir\Package.cmd" Squiggle.UI\$releasePath Client $shortVersion
    copy "$myDir\..\Squiggle.Setup\bin\Release\Squiggle.Setup.msi" "$myDir\Squiggle-$shortVersion Client.msi"

    & "$myDir\Package.cmd" Squiggle.Bridge\$releasePath Bridge $shortVersion
    & "$myDir\Package.cmd" Squiggle.Multicast\$releasePath Multicast $shortVersion
    & "$myDir\Package.cmd" Scripts Scripts $shortVersion
}

function Create-Setup()
{
    write-host ------------- Creating setup ---------------------------
    $productName = "Squiggle $shortVersion"
    Change-Setup-Product-Name "Squiggle" $productName
    Change-Setup-Version "1.0.0.0" $longVersion
    & "$myDir\Build.cmd" "Squiggle.Setup\Squiggle.Setup.wixproj"
    Change-Setup-Version $longVersion "1.0.0.0"
    Change-Setup-Product-Name $productName "Squiggle"
}

function Update-Config()
{
    write-host ------------- Updating config -------------------------
    $gitHash = git rev-parse HEAD
    $configPath = "$myDir\..\Squiggle.UI\$releasePath\Squiggle.exe.config"
    Replace-In-File $configPath "GitHash`" value=`"`"" "GitHash`" value=`"$gitHash`""
}

function Build-Squiggle()
{
    write-host ------------- Building Squiggle $shortVersion -----------------------
    & "$myDir\Build.cmd" Squiggle.sln
    if (!$?) { 
        #last command (msbuild) failed
        exit
    }
}

function Change-Setup-Product-Name($from, $to)
{
    Replace-In-File "$myDir\..\Squiggle.Setup\Product.wxs" "Name=`"$from" "Name=`"$to"
}

function Change-Setup-Version($from, $to)
{
    Replace-In-File "$myDir\..\Squiggle.Setup\Product.wxs" "Version=`"$from" "Version=`"$to"
}

function Replace-In-File($filePath, $find, $replace)
{
    (get-content $filePath) -replace $find, $replace | set-content $filePath
}

Main