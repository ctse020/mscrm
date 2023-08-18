# Implementation Ctse.Pcf.CanvasJs

## In admin.powerapps.com:
Environment -> Settings -> Product -> Functions -> turn on "Power Apps component framework voor canvas-apps"

## In CanvasApp/CustomPage:

### Add custom component "CanvasJs" 

### App.OnStart
Set(varJs; "")

### CanvasJs1.input
varJs

# Examples

## Navigate

### Button1.OnSelect
UpdateContext({varJs: "Xrm.Navigation.navigateTo({ pageType: 'dashboard', dashboardId: '61e1c08d-ed35-ee11-bdf4-00224887644a' });"})

## Calculate

### Button1.OnSelect
UpdateContext({varJs: "return 7-3"});

### Label1.Text
CanvasJs1.output;

## Promise

### Button1.OnSelect
UpdateContext({varJs: "return Xrm.Navigation.openConfirmDialog({ title:"Please confirm", text:"Are you sure?" }).then(function(result){ return result; });"});

### Label1.Text
CanvasJs1.output;