# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$ErrorActionPreference = 'Stop'

& "$PSScriptRoot/set-env.ps1"
$all_ok = $True

Write-Host "##[info]Test Native simulator"
pushd (Join-Path $PSScriptRoot "../src/Simulation/Native/build")
ctest -C $BUILD_CONFIGURATION
$script:all_ok = ($LastExitCode -eq 0) -and $script:all_ok
popd


function Test-One {
    Param($project)

    Write-Host "##[info]Testing $project..."
    dotnet test (Join-Path $PSScriptRoot $project) `
        -c $Env:BUILD_CONFIGURATION `
        -v $Env:BUILD_VERBOSITY `
        --logger trx `
        /property:DefineConstants=$Env:ASSEMBLY_CONSTANTS `
        /property:Version=$Env:ASSEMBLY_VERSION

    $script:all_ok = ($LastExitCode -eq 0) -and $script:all_ok
}

Test-One '../src/Simulation/CsharpGeneration.Tests/Tests.CsharpGeneration.fsproj'

Test-One '../src/Simulation/RoslynWrapper.Tests/Tests.RoslynWrapper.fsproj'

Test-One '../src/Simulation/QCTraceSimulator.Tests/Tests.Microsoft.Quantum.Simulation.QCTraceSimulatorRuntime.csproj'

Test-One '../src/Simulation/Simulators.Tests/Tests.Microsoft.Quantum.Simulation.Simulators.csproj'

if (-not $all_ok) 
{
    throw "At least one project failed to compile. Check the logs."
}

