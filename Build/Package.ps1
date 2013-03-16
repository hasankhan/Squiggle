$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path
del ("$scriptDir\..\{0}\*.pdb" -f $args[0])
$target = ("{0}\Squiggle-{1} {2}.zip" -f $scriptDir, $args[2], $args[1])
$source = ("{0}\..\{1}\*.*" -f $scriptDir, $args[0])
& "${Env:ProgramFiles}\7-Zip\7z" a -r -tzip $target $source