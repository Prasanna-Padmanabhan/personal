How to use Power BI to visualize Azure Advisor recommendations

Azure Advisor provides a REST API to programmatically access best practice recommendations for your Azure resources, more details at https://docs.microsoft.com/en-us/rest/api/advisor/.

In order to visualize these recommendations in Power BI, you need to do the following:

1. Copy the GenerateAdvisorRecommendations.ps1 locally.

2. (Not required if you have done this already) Open a Windows PowerShell prompt with Administrator access and run:

	Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

3. Open a Windows PowerShell prompt without Administrator access, navigate to the folder where you have copied the ps1 file and run:

	Login-AzureRmAccount
	Get-AzureRmSubscription | Update-AzureRmAdvisorRecommendations

4. Copy the Azure Advisor Recommendations.pbit file locally.

5. Open the file in Power BI, log in using Organizational account (i.e. the same account that you use to log in to Azure).

6. The template will refresh with latest recommendations for your Azure resources across all Azure subscriptions that you have access to.

7. You can save the file as a regular Power BI file and open that directly next time (it will use your saved credentials to log in).


The PowerShell script is used to refresh recommendations in the service so in order to get the latest set of recommendations, you will need to run the PowerShell script (i.e. step 3 above) before refreshing the Power BI file.