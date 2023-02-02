

## Deployment guide

**Create Azure Github credentials**

Run the following

```bash
az ad sp create-for-rbac --name "cyclingstats-sp" --role "contributor" --scopes "/subscriptions/b73995e3-caad-4882-8644-f2175789c3ff/resourceGroups/cycling-stats" --sdk-auth
```