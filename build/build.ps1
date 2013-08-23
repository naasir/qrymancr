# -- PROPERTIES ------------------------------------
properties {# constants
    $author     = "Naasir Ramji"
    $product    = "Qrymancr"

    # *NOTE* the BUILD_NUMBER environment variable is set by the CI server
    $build           = envOrDefault "BUILD_NUMBER" 0
    $version         = "0.1.0"
    $semanticVersion = "$version+build.$build"
    $displayVersion  = "$version (build $build)"
}

properties {# directories
    $baseDir      = resolve-path ..
    $buildDir     = "$baseDir\build"
    $releaseDir   = "$baseDir\release"
    $srcDir       = "$baseDir\src"
    $testDir      = "$baseDir\test"
    $vendorDir    = "$baseDir\vendor"
    $workingDir   = "$baseDir\working"
}

properties {# files
    $buildReadyMarker = "$baseDir\build.ready"
    $solution         = "$srcDir\Qrymancr.sln"
}

properties {# tools
    $nspec   = "$vendorDir\nspec.0.9.67\tools\NSpecRunner.exe"
}

# -- INCLUDES --------------------------------------

import-module ".\assemblyinfo.psm1" -DisableNameChecking

# -- TASKS --------------------------------------

# default task
task default -depends release

# display help
task ? -description "Displays a list of all available tasks" {
    write-documentation
}

# playground to test build script stuff
task debug -description "Playground to test things" {

}

# ensure a clean working directory
task clean -description "Provides a clean environment for the build process" `
{
    # delete all previous build artifacts
    $artifacts = @(
        "$workingDir",
        "$releaseDir",
        "$buildReadyMarker"
    )

    $artifacts | foreach-object {
        remove-item $_ -force -recurse -ErrorAction SilentlyContinue
    }
}

# initialize the build process
task initialize -description "Initializes the build process" `
    -depends clean `
{
    # create neccessary directories
    $directories = @(
        "$releaseDir",
        "$workingDir"
    )

    $directories | foreach-object {
        new-item $_ -itemType directory -ErrorAction SilentlyContinue
    }
}

# set the version number
task set-version -description "Sets the version number for the project" `
    -continueOnError `
{
    $year = [DateTime]::Now.Year
    $infoVersion = [Version]::Parse($version).ToString(2)
    $assemblyVersion = "$version.$build"

    generate-assembly-info `
        -file "$srcDir\SharedAssemblyInfo.cs" `
        -company "" `
        -product $product `
        -copyright "© $year $author" `
        -version $assemblyVersion `
        -infoversion $infoVersion
}

# compile the solution
task compile -description "Compiles the project" `
    -depends initialize, set-version `
{
    exec {
        msbuild $solution /maxcpucount /target:ReBuild /property:Configuration=Release /property:TreatWarningsAsErrors=False /verbosity:minimal /property:OutDir=$workingDir\
    } "Error rebuilding solution: $solution"

    new-item $buildReadyMarker -itemType file
}

# run through test suite
task test -description "Runs through the project's test suite" `
    -depends compile `
{
    $testAssemblies = get-ChildItem $workingDir -include *Test.dll -recurse

    foreach ($assembly in $testAssemblies) {
        # intentionally not wrapping the call to the test runner in an exec block
        # as we want all tests to run (regardless of failure) so we can view all results at once
        & $nspec $assembly --formatter=htmlformatter | out-file "$workingDir\index.html"
    }
}

# create a release
task release -description "Does everything required to create a project release" `
    -depends test

# -- FUNCTIONS ------------------------------------

# return the value of the specified environment variable if found,
# otherwise, return the specified default value
function envOrDefault($name, $default) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if ($value.length -gt 0) {
        return $value
    } else {
        return $default
    }
}