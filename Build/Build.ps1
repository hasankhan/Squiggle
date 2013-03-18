$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
$releasePath = "bin\x86\Release"

$version = (Select-String $scriptDir\..\Shared\AssemblyInfo.Version.cs -pattern 'AssemblyVersion[(]"((\d\.\d)\.\d\.\d)').Matches[0]
$longVersion = $version.Groups[1].Value
$shortVersion = $version.Groups[2].Value

function Main()
{
    rm *.zip,*.msi

    Write-Host ------------- Building Squiggle $shortVersion -----------------------
    Build-Squiggle
    
	Write-Host ------------- Updating config -------------------------
    Update-Config
    
	Write-Host ------------- Creating setup ---------------------------
    Create-Setup
    
	Write-Host ------------- Packaging ---------------------------
    Package-All

    Write-Host ------------- Done ------------------------------------
}

function Package-All()
{
    Package Squiggle.UI\$releasePath Client $shortVersion
    copy "$scriptDir\..\Squiggle.Setup\bin\Release\Squiggle.Setup.msi" "$scriptDir\Squiggle-$shortVersion Client.msi"

    Package Squiggle.Bridge\$releasePath Bridge $shortVersion
    Package Squiggle.Multicast\$releasePath Multicast $shortVersion
    Package Scripts Scripts $shortVersion
}

function Package()
{
    del ("$scriptDir\..\{0}\*.pdb" -f $args[0])
    $target = ("{0}\Squiggle-{1} {2}.zip" -f $scriptDir, $args[2], $args[1])
    $source = ("{0}\..\{1}\*.*" -f $scriptDir, $args[0])
    Create-Archive $target $source
}

function Create-Setup()
{
    $productName = "Squiggle $shortVersion"
    Change-Setup-Product-Name "Squiggle" $productName
    Change-Setup-Version "1.0.0.0" $longVersion
    Build "Squiggle.Setup\Squiggle.Setup.wixproj"
    Change-Setup-Version $longVersion "1.0.0.0"
    Change-Setup-Product-Name $productName "Squiggle"
}

function Update-Config()
{
    $gitHash = git rev-parse HEAD
    $configPath = "$scriptDir\..\Squiggle.UI\$releasePath\Squiggle.exe.config"
    Replace-In-File $configPath "GitHash`" value=`"`"" "GitHash`" value=`"$gitHash`""
}

function Build-Squiggle()
{
    Build Squiggle.sln    
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

function Create-Archive($target, $source)
{
	& "${Env:ProgramFiles}\7-Zip\7z" a -r -tzip $target $source
}

function Build($file)
{
	& "${Env:WINDIR}\Microsoft.NET\Framework\v4.0.30319\msbuild" "$scriptDir\..\$file" "/p:Configuration=Release" "/t:Clean,Build" "/v:M" "/fl" "/nr:false"
    if (!$?) { 
        #last command (msbuild) failed
        exit
    }
}

Main