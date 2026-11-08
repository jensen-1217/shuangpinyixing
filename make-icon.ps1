Add-Type -AssemblyName System.Drawing

function New-IconBitmap([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap($s, $s)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $scale = $s / 256.0
    $g.ScaleTransform($scale, $scale)

    $RR = {
        param([float]$x, [float]$y, [float]$w, [float]$h, [float]$r)
        $p = New-Object System.Drawing.Drawing2D.GraphicsPath
        $d = $r * 2
        $p.AddArc($x, $y, $d, $d, 180, 90)
        $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
        $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
        $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
        $p.CloseFigure()
        return $p
    }

    # 深色圆角底板
    $bg = & $RR 8 8 240 240 48
    $brushBg = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(35, 38, 46))
    $penBg = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(58, 64, 76))
    $penBg.Width = 4
    $g.FillPath($brushBg, $bg)
    $g.DrawPath($penBg, $bg)

    # 2x2 键帽，右下角蓝色高亮
    $keys = @(
        ,@(48, 48, $false)
        ,@(146, 48, $false)
        ,@(48, 146, $false)
        ,@(146, 146, $true)
    )
    foreach ($k in $keys) {
        $x = $k[0]; $y = $k[1]; $hot = $k[2]
        $r = & $RR $x $y 62 62 14
        if ($hot) {
            $fill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(48, 124, 240))
            $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(120, 180, 255))
            $pen.Width = 3
        } else {
            $fill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(48, 53, 64))
            $pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(70, 77, 92))
            $pen.Width = 3
        }
        $g.FillPath($fill, $r)
        $g.DrawPath($pen, $r)
        $fill.Dispose(); $pen.Dispose(); $r.Dispose()
    }
    $bg.Dispose(); $brushBg.Dispose(); $penBg.Dispose()
    $g.Dispose()
    return $bmp
}

$sizes = @(256, 48, 32, 16)
$pngs = @()
foreach ($s in $sizes) {
    $bmp = New-IconBitmap $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngs += ,($ms.ToArray())
    $ms.Dispose()
    $bmp.Dispose()
}

$out = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($out)
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $s = $sizes[$i]
    $data = $pngs[$i]
    $dim = [byte]$(if ($s -ge 256) { 0 } else { $s })
    $bw.Write($dim)
    $bw.Write($dim)
    $bw.Write([byte]0)
    $bw.Write([byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($data in $pngs) { $bw.Write($data) }
$bw.Flush()
[System.IO.File]::WriteAllBytes("$PSScriptRoot\app.ico", $out.ToArray())
$bw.Close()
"app.ico written: $((Get-Item "$PSScriptRoot\app.ico").Length) bytes"
