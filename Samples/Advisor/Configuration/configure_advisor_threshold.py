from msrestazure.azure_active_directory import ServicePrincipalCredentials
from azure.mgmt.advisor import AdvisorManagementClient
from azure.mgmt.advisor.models import ConfigData, ConfigDataProperties


client_id = "<client ID of the Azure AD application that has access to the subscription>"
secret = "<client secret>"
tenant = "<tenant ID of your organization>"
subscription_id = "<subscription ID of the Azure subscription>"

def configure_advisor_threshold():

    creds = ServicePrincipalCredentials(client_id=client_id,
                                        secret=secret,
                                        tenant=tenant)

    creds.set_token()

    client = AdvisorManagementClient(credentials=creds,
                                     subscription_id=subscription_id)

    # create a new configuration to update low CPU threshold to 20
    cfg = ConfigData()
    cfg.properties = ConfigDataProperties(low_cpu_threshold=20,
                                          exclude=False)

    # update the configuration
    client.configurations.create_in_subscription(cfg)

if __name__ == "__main__":
    configure_advisor_threshold()
