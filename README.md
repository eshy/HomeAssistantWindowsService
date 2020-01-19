# HomeAssistantWindowsService
A windows service to trigger scenes in Home Assistant when the computer is suspended or resumes

This is a small service I created to turn off my monitors when my laptop goes to sleep and turn them back on when it wakes up.

The service uses the OnPowerEvent's ResumeSuspend and Suspent power statuses to trigger scenes with HomeAssistant's REST Api.

#### Installing the Service
Download the latest release and extract it to a local folder

Open a command prompt or powershell window and use InstallUtil to install the service

C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i D:\HAService\HomeAssistantPowerStateService.exe

#### Using the Service

There are four settings you need to update in the HomeAssistantPowerStateService.config file

##### BaseUrl
The Home Assistant URL

##### Token
A Long-Lived Access Token (create one from your profile page in Home Assistant) 

##### SuspendScene
The scene to trigger when the computer is suspended

##### ResumeSuspendScene
The scene to trigger when the computer wakes up

note: Other statuses are available as well. Just add a key with the [PowerBroadcastStatus](https://docs.microsoft.com/en-us/dotnet/api/system.serviceprocess.powerbroadcaststatus?view=netframework-4.8) name and append Scene to that key.

After updating the config file start the service and your scenes should be triggered.
