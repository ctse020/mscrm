            dynamic entity = new ExpandoObject();
            entity.firstname = "test1";
            entity.lastname = "test1";

            var client = CRMWebAPI.CreateOnline("azure-clientid", "crm-username", "crm-password", "https://login.microsoftonline.com/common", "crm-apiurl");

            Task.Run(async () =>
            {
                Guid entityId = await client.Create("contacts", entity);

            }).Wait();
