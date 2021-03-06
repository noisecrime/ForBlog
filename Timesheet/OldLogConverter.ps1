#http://www.vistax64.com/powershell/211869-append-text-top-text-file.html
function Insert-Content {
    param ( [String]$Path )
    process {
        $( ,$_; Get-Content $Path -ea SilentlyContinue) | Out-File $Path
    }
}

#http://www.tellingmachine.com/post/Test-Member-The-Missing-PowerShell-Cmdlet.aspx
function Test-Member()
{
<# 
.Synopsis 
    Verifies whether a specific property or method member exists for a given .NET object 
.Description 
    Verifies whether a specific property or method member exists for a given .NET object  
.Example
    Test-Member -PropertyName "Context" -InputObject (new-object System.Collections.ArrayList)
.Example
    new-object "System.Collections.ArrayList" | Test-Member -PropertyName "ToArray"
.Parameter PropertyName
    Name of a Property to test for 
.Parameter MethodName
    Name of a Method to test for  
.ReturnValue 
    $True or $False
.Link 
    about_functions_advanced 
    about_functions_advanced_methods 
    about_functions_advanced_parameters 
.Notes 
NAME:      Test-Member
AUTHOR:    Klaus Graefensteiner 
LASTEDIT:  04/20/2010 12:12:42
#Requires -Version 2.0 
#> 
     
    [CmdletBinding(DefaultParameterSetName="Properties")]
    PARAM(
         
        [ValidateNotNull()]
        [Parameter(Position=0, Mandatory=$True, ValueFromPipeline=$True)]
        $InputObject,
         
        [ValidateNotNullOrEmpty()]
        [Parameter(Position=1, Mandatory=$True, ValueFromPipeline=$False, ParameterSetName="Properties")]
        [string] $PropertyName,
         
        [ValidateNotNullOrEmpty()]
        [Parameter(Position=1, Mandatory=$True, ValueFromPipeline=$False, ParameterSetName="Methods")]
        [string] $MethodName     
    )
    Process{
         
        switch ($PsCmdlet.ParameterSetName) 
        { 
            "Properties"
            {
                $Members = Get-Member -InputObject $InputObject;
                if ($Members -ne $null -and $Members.count -gt 0)
                {
                    foreach($Member in $Members)
                    {
                        if(($Member.MemberType -eq "Property" ) -and ($Member.Name -eq $PropertyName))
                        {
                            return $true
                        }
                    }
                    return $false
                }
                else
                {
                    return $false;
                }
            }
            "Methods"
            {
                $Members = Get-Member -InputObject $InputObject;
                if ($Members -ne $null -and $Members.count -gt 0)
                {
                    foreach($Member in $Members)
                    {
                        if(($Member.MemberType -eq "Method" ) -and ($Member.Name -eq $MethodName))
                        {
                            return $true
                        }
                    }
                    return $false
                }
                else
                {
                    return $false;
                }
            }
        }
     
    }# End Process
}




$csv = import-csv Log.txt

$prop = $csv[0] | Test-Member -PropertyName "Datetime"

#write-host 'hasprop' $prop 


if ($prop = 'False') {
    #write-host 'got into if'
    "Datetime,User,State" | Insert-Content Log.txt
    $csv = import-csv Log.txt
}

$i=0

$outlines = @()

foreach ($item in $csv)
{
    $i++
    
    $newdatetime = $item.Datetime.substring(0,4) + "-" +
                   $item.Datetime.substring(4,2) + "-" +
                   $item.Datetime.substring(6,2) + "T" +
                   $item.Datetime.substring(9,8) + "+3:00"
    
    $outlines += New-Object -TypeName PSObject -Property @{
                Line = $i
                Datetime = $newdatetime
                User = $item.User
                State = $item.State
                } 
                
                
                
} 

$outlines | select-object Line,Datetime, User, State | ConvertTo-Csv -NoTypeInformation | % {$_.Replace('"','')} | Out-File Lognew.txt

