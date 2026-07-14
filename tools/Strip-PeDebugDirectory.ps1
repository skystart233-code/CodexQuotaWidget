param(
    [Parameter(Mandatory = $true)]
    [string] $EmojiWpfPath,
    [Parameter(Mandatory = $true)]
    [string] $GlyphLayoutPath,
    [Parameter(Mandatory = $true)]
    [string] $OpenFontPath,
    [Parameter(Mandatory = $true)]
    [string] $StfuPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-UInt16
{
    param([byte[]] $bytes, [int] $offset)
    return [BitConverter]::ToUInt16($bytes, $offset)
}

function Read-UInt32
{
    param([byte[]] $bytes, [int] $offset)
    return [BitConverter]::ToUInt32($bytes, $offset)
}

function Write-Zeroes([byte[]] $bytes, [int] $offset, [int] $count)
{
    if ($offset -lt 0 -or $count -lt 0 -or $offset + $count -gt $bytes.Length)
    {
        throw "PE offset is outside the file: $offset + $count"
    }

    [Array]::Clear($bytes, $offset, $count)
}

foreach ($path in @($EmojiWpfPath, $GlyphLayoutPath, $OpenFontPath, $StfuPath))
{
    if (-not (Test-Path -LiteralPath $path -PathType Leaf))
    {
        throw "PE file was not found: $path"
    }

    $bytes = [IO.File]::ReadAllBytes($path)
    if ($bytes.Length -lt 0x100 -or $bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A)
    {
        throw "Not a PE file: $path"
    }

    $peOffset = [int](Read-UInt32 $bytes 0x3C)
    if ($peOffset -lt 0 -or $peOffset + 24 -gt $bytes.Length -or
        $bytes[$peOffset] -ne 0x50 -or $bytes[$peOffset + 1] -ne 0x45)
    {
        throw "Invalid PE header: $path"
    }

    $coffOffset = $peOffset + 4
    $sectionCount = [int](Read-UInt16 $bytes ($coffOffset + 2))
    $optionalSize = [int](Read-UInt16 $bytes ($coffOffset + 16))
    $optionalOffset = $coffOffset + 20
    $magic = Read-UInt16 $bytes $optionalOffset
    $dataDirectoryOffset = switch ($magic)
    {
        0x10B { $optionalOffset + 96; break }
        0x20B { $optionalOffset + 112; break }
        default { throw "Unsupported PE optional header: $path" }
    }
    $sectionOffset = $optionalOffset + $optionalSize
    if ($sectionOffset + ($sectionCount * 40) -gt $bytes.Length)
    {
        throw "Invalid PE section table: $path"
    }

    $debugDirectoryOffset = $dataDirectoryOffset + (6 * 8)
    $debugRva = Read-UInt32 $bytes $debugDirectoryOffset
    $debugSize = Read-UInt32 $bytes ($debugDirectoryOffset + 4)
    if ($debugRva -eq 0 -or $debugSize -eq 0)
    {
        continue
    }

    $debugFileOffset = $null
    for ($index = 0; $index -lt $sectionCount; $index++)
    {
        $offset = $sectionOffset + ($index * 40)
        $virtualSize = Read-UInt32 $bytes ($offset + 8)
        $virtualAddress = Read-UInt32 $bytes ($offset + 12)
        $rawSize = Read-UInt32 $bytes ($offset + 16)
        $rawOffset = Read-UInt32 $bytes ($offset + 20)
        $sectionSize = [Math]::Max($virtualSize, $rawSize)
        if ($debugRva -ge $virtualAddress -and $debugRva -lt ($virtualAddress + $sectionSize))
        {
            $debugFileOffset = [int]($rawOffset + ($debugRva - $virtualAddress))
            break
        }
    }

    if ($null -eq $debugFileOffset -or $debugSize % 28 -ne 0)
    {
        throw "Invalid PE debug directory: $path"
    }

    for ($offset = $debugFileOffset; $offset -lt $debugFileOffset + $debugSize; $offset += 28)
    {
        $dataSize = [int](Read-UInt32 $bytes ($offset + 16))
        $dataOffset = [int](Read-UInt32 $bytes ($offset + 24))
        if ($dataSize -gt 0 -and $dataOffset -gt 0)
        {
            Write-Zeroes $bytes $dataOffset $dataSize
        }
    }

    Write-Zeroes $bytes $debugFileOffset ([int]$debugSize)
    Write-Zeroes $bytes $debugDirectoryOffset 8
    [IO.File]::WriteAllBytes($path, $bytes)
}
