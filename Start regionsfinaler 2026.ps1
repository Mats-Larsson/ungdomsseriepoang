Set-Location $PSScriptRoot;
$exe = '.\Usp\bin\Debug\net10.0\Usp.exe';

Start-Process $exe -ArgumentList "-l 8880 -s Liveresultat -L 37570 --pointscalc Normal";
Start-Process http://localhost:8880

Start-Process $exe -ArgumentList "-l 8887 -s Liveresultat -L 37505 --pointscalc Normal";
Start-Process http://localhost:8887

Start-Process $exe -ArgumentList "-l 9090 -s Liveresultat -L 37590 --pointscalc Normal";
Start-Process http://localhost:9090
