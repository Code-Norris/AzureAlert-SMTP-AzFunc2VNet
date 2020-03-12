$keyTohash = New-Guid;
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($keyTohash)
    $algorithm = [System.Security.Cryptography.HashAlgorithm]::Create('MD5')
    $StringBuilder = New-Object System.Text.StringBuilder 
  
    $algorithm.ComputeHash($bytes) | 
    ForEach-Object { 
        $null = $StringBuilder.Append($_.ToString("x2")) 
    } 
  

    Write-Host  $StringBuilder.ToString()