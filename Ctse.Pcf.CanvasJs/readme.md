# Ctse.Pcf.CanvasJs

## In admin.powerapps.com:
Environment -> Settings -> Product -> Functions -> turn on "Power Apps component framework voor canvas-apps"

## In CanvasApp/CustomPage:

### Add custom component "CanvasJs" 

### App.OnStart
Set(varJs; "")

### CanvasJs1.input
varJs

### Button1.OnSelect
UpdateContext({varJs: "Xrm.Navigation.navigateTo({ pageType: 'dashboard', dashboardId: '61e1c08d-ed35-ee11-bdf4-00224887644a' });"})

