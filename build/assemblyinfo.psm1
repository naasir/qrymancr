# assemblyinfo: a module for managing assemblyInfo.cs files

# PUBLIC -----------

# generates an assembly info file
# inspired by: https://github.com/hibernating-rhinos/rhino-esb/blob/master/psake_ext.ps1
function Generate-Assembly-Info
{
    param(
        [Parameter(Mandatory=$true)]
        [String]
        $file,

        [Parameter(Mandatory=$true)]
        [AllowEmptyString()]
        [String]
        $company,

        [Parameter(Mandatory=$true)]
        [String]
        $product,

        [Parameter(Mandatory=$true)]
        [String]
        $copyright,

        [Parameter(Mandatory=$true)]
        [String]
        $version,

        [Parameter(Mandatory=$true)]
        [String]
        $infoVersion,

        [Parameter(Mandatory=$false)]
        [String]
        $clsCompliant = "false"
    )

    $asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompanyAttribute(""$company"")]
[assembly: AssemblyProductAttribute(""$product"")]
[assembly: AssemblyCopyrightAttribute(""$copyright"")]

#if DEBUG
[assembly: AssemblyConfiguration(""Debug"")]
#else
[assembly: AssemblyConfiguration(""Release"")]
#endif

[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyFileVersionAttribute(""$version"")]
[assembly: AssemblyInformationalVersionAttribute(""$infoVersion"")]

[assembly: CLSCompliantAttribute($clsCompliant)]"

    write-host "Generating assembly info file: $file"
    $asmInfo | out-file $file -force
}

# -- EXPORT --------------------------------------

Export-ModuleMember -function Generate-Assembly-Info
