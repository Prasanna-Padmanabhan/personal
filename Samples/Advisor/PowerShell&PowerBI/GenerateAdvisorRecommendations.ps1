##
##  <copyright file="GenerateAdvisorRecommendations.ps1" company="Microsoft">
##    Copyright (C) Microsoft. All rights reserved.
##  </copyright>
##
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
# ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
# WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
# IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
# INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
# PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
# LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
# USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#

<#
.SYNOPSIS
Gets Azure Advisor recommendations for the given Azure subscriptions.
.DESCRIPTION
Gets Azure Advisor recommendations for the given Azure subscriptions. The subscriptions can be passed directly or piped in.
.PARAMETER SubscriptionIds One or more subscription IDs.
.EXAMPLE
Get-AzureRmSubscription | Get-AzureRmAdvisorRecommendations

Get Azure Advisor recommendations for all subscriptions.
.EXAMPLE
Get-AzureRmAdvisorRecommendations -SubscriptionIds foo

Get Azure Advisor recommendations for subscription with ID foo.
.NOTES
You must run Login-AzureRmAccount before running this function.

Also, in order to get the latest list of recommendations, you must run Update-AzureRmAdvisorRecommendations before running this.
#>
function Get-AzureRmAdvisorRecommendations
{
    [CmdletBinding()]
    param(
        [parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)][string[]]$SubscriptionIds
    )

    begin
    {
        $ErrorActionPreference = 'Stop'
        $ProgressPreference = 'SilentlyContinue'

        $headers = Get-AuthorizationHeader
        if ($headers -eq $null) {break}
    }

    process
    {
        foreach ($sub in $SubscriptionIds)
        {
            # Initial URI for getting recommendations
            $listUri = ("https://management.azure.com/subscriptions/{0}/providers/Microsoft.Advisor/recommendations?api-version=2017-03-31" -f $sub)

            $output = @()

            do
            {
                $results = Get-Recommendations -url $listUri -headers $headers

                if ($results -eq $null) {break}

                # Append the list of results
                $output += $results.value

                # the nextLink to follow to get more recommendations
                $nextLink = Get-Member -InputObject $results -Name nextLink -MemberType Properties
                if ($nextLink -ne $null)
                {
                    $listUri = $results.nextLink
                }
                else
                {
                    $listUri = $null
                }
            }
            while ($listUri -ne $null);

            return $output
        }
    }

    end {}
}

<#
.SYNOPSIS
Generates Azure Advisor recommendations for the given Azure subscriptions.
.DESCRIPTION
Generates Azure Advisor recommendations for the given Azure subscriptions. The subscriptions can be passed directly or piped in.
.PARAMETER SubscriptionIds One or more subscription IDs.
.PARAMETER Timeout The number of seconds to wait for generation to complete (defaults to 60 seconds).
.EXAMPLE
Get-AzureRmSubscription | Update-AzureRmAdvisorRecommendations

Generate Azure Advisor recommendations for all subscriptions.
.EXAMPLE
Update-AzureRmAdvisorRecommendations -SubscriptionIds foo

Generate Azure Advisor recommendations for subscription with ID foo.
.NOTES
You must run Login-AzureRmAccount before running this function.
#>
function Update-AzureRmAdvisorRecommendations
{
    [CmdletBinding()]
    param(
        [parameter(Mandatory=$true, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)][string[]]$SubscriptionIds,
        [parameter(Mandatory=$false)][int]$Timeout = 60
    )

    begin
    {
        $ErrorActionPreference = 'Stop'
        $ProgressPreference = 'SilentlyContinue'

        $headers = Get-AuthorizationHeader
        if ($headers -eq $null) {break}
    }

    process
    {
        foreach ($sub in $SubscriptionIds)
        {
            $secondsElapsed = 0
            $result = New-Object PSObject -Property @{"SubscriptionId" = $sub; "Status" = "Success"; "SecondsElapsed" = $secondsElapsed}

            # Register the subscription with Advisor
            $registerUri = ("https://management.azure.com/subscriptions/{0}/providers/Microsoft.Advisor/register?api-version=2017-06-01" -f $sub)
            Write-Verbose ("POST {0}" -f $registerUri)

            try
            {
                $response = Invoke-WebRequest -Uri $registerUri -Method Post -Headers $headers
            }
            catch
            {
                $result.Status = $_.Exception.Response.StatusCode
                return $result
            }

            # Kick off generate recommendations
            $generateUri = ("https://management.azure.com/subscriptions/{0}/providers/Microsoft.Advisor/generateRecommendations?api-version=2017-03-31" -f $sub)
            Write-Verbose ("POST {0}" -f $generateUri)
            $response = Invoke-WebRequest -Uri $generateUri -Method Post -Headers $headers

            # the URI to poll is specified in the Location field of the response header
            $statusUri = $response.Headers.Location
            Write-Verbose ("GET {0}" -f $statusUri)

            while ($secondsElapsed -lt $Timeout)
            {

                $response = Invoke-WebRequest -Uri $statusUri -Method Get -Headers $headers
                if ($response.StatusCode -eq 204) {break}
                Write-Verbose ("Waiting for generation to complete for subscription {0}..." -f $sub)
                Start-Sleep -Seconds 1
                $secondsElapsed++
            }

            $result.SecondsElapsed = $secondsElapsed
            if ($secondsElapsed -ge $Timeout)
            {
                $result.Status = "Timeout"
            }

            return $result
        }
    }

    end {}
}

function Get-AuthorizationHeader
{
    if (-not (Get-Module AzureRm.Profile))
    {
        Import-Module AzureRm.Profile
    }

    $azureRmProfileModuleVersion = (Get-Module AzureRm.Profile).Version
    # refactoring performed in AzureRm.Profile v3.0 or later
    if ($azureRmProfileModuleVersion.Major -ge 3)
    {
        $azureRmProfile = [Microsoft.Azure.Commands.Common.Authentication.Abstractions.AzureRmProfileProvider]::Instance.Profile
        if (-not $azureRmProfile.Accounts.Count)
        {
            Write-Error "Please run Login-AzureRmAccount before calling this function."
            return $null
        }
    }
    else
    {
        # AzureRm.Profile < v3.0
        $azureRmProfile = [Microsoft.WindowsAzure.Commands.Common.AzureRmProfileProvider]::Instance.Profile
        if (-not $azureRmProfile.Context.Account.Count)
        {
            Write-Error "Please run Login-AzureRmAccount before calling this function."
            return $null
        }
    }

    $currentAzureContext = Get-AzureRmContext
    $profileClient = New-Object Microsoft.Azure.Commands.ResourceManager.Common.RMProfileClient($azureRmProfile)
    Write-Verbose ("Getting access token for tenant" + $currentAzureContext.Subscription.TenantId)
    $token = $profileClient.AcquireAccessToken($currentAzureContext.Subscription.TenantId)
    $headers = @{"Authorization"="Bearer " + $token.AccessToken}
    Write-Debug $token.AccessToken
    return $headers
}

function Get-Recommendations
{
    param(
        [parameter(Mandatory=$true)][string]$url,
        [parameter(Mandatory=$true)]$headers
    )

    Write-Verbose ("GET {0}" -f $listUri)
    $response = Invoke-WebRequest -Uri $url -Method Get -Headers $headers

    if ($response.StatusCode -ne 200)
    {
        return $null
    }

    $json = ConvertFrom-Json $response.Content

    $json
}